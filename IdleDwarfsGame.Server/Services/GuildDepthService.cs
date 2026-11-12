using Microsoft.EntityFrameworkCore;

// Причина отказа — фронт показывает по коду переведённый текст (guild.depth.errors.<code>)
public enum GuildDepthError
{
    NotInGuild,
    RaceNotActive,
    HeroNotFound,
    HeroInSquad,
    HeroBusy,
    // Дворф этой недели уже стоит в шахте другой гильдии
    PlacedInOtherGuild,
    NoDwarf,
    MaxLevel,
    NotEnoughGold,
    NothingToCollect,
    NothingToClaim,
    Changed
}

public class GuildDepthException(GuildDepthError code, string message) : InvalidOperationException(message)
{
    public GuildDepthError Code { get; } = code;
}

// «Гонка глубины» (правила — GuildDepthRules). Добыча ленивая, как у общих шахт: перед каждым изменением дворф
// досчитывается по старой скорости, а улучшение подъёмника сначала досчитывает всех дворфов гильдии.
// Концепт: сервис работает с GameDbContext напрямую, без отдельного репозитория
public class GuildDepthService(
    GameDbContext db,
    IGuildRepository guilds,
    IHeroesRepository heroes,
    IExpeditionRepository expeditions,
    IPlayerRepository players,
    TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<GuildDepthOverview> GetAsync(Guid userId)
    {
        var membership = await RequireMembershipAsync(userId);
        return await DescribeAsync(userId, membership, Now);
    }

    public async Task<GuildDepthOverview> PlaceAsync(Guid userId, Guid heroId)
    {
        var now = Now;
        var week = GuildDepthRules.WeekOf(now);
        var membership = await RequireMembershipAsync(userId);
        RequireActive(week, now);
        var playerId = await PlayerIdAsync(userId);

        var hero = (await heroes.GetByPlayerAsync(playerId)).FirstOrDefault(h => h.Id == heroId)
            ?? throw new GuildDepthException(GuildDepthError.HeroNotFound, "Hero not found");
        if (hero.PlaceInSquad != null)
            throw new GuildDepthException(GuildDepthError.HeroInSquad, "Heroes from the squad can't work in a mine");
        var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
        busy.UnionWith(await heroes.GetMiningHeroIdsAsync(playerId));
        if (busy.Contains(heroId))
            throw new GuildDepthException(GuildDepthError.HeroBusy, "This dwarf is busy");

        var shaft = await EnsureShaftAsync(membership.GuildId, week);
        var dwarf = await db.GuildDepthDwarves.FirstOrDefaultAsync(d => d.WeekStart == week && d.UserId == userId);
        if (dwarf == null)
        {
            db.GuildDepthDwarves.Add(new GuildDepthDwarf
            {
                WeekStart = week,
                UserId = userId,
                GuildId = membership.GuildId,
                PlayerId = playerId,
                HeroId = heroId,
                BaseRate = CommunityMineRules.DwarfRate(hero),
                UpdatedAt = now,
                ReleaseAt = GuildDepthRules.EndOf(week)
            });
        }
        else
        {
            if (dwarf.GuildId != membership.GuildId)
                throw new GuildDepthException(GuildDepthError.PlacedInOtherGuild, "Your dwarf already works for another guild this week");
            // Замена дворфа: добытое старым остаётся, дальше копает новый
            GuildDepthRules.Settle(dwarf, shaft.Level, now);
            dwarf.HeroId = heroId;
            dwarf.BaseRate = CommunityMineRules.DwarfRate(hero);
            dwarf.Version++;
        }
        await SaveAsync();
        return await DescribeAsync(userId, membership, now);
    }

    public async Task<GuildDepthOverview> RecallAsync(Guid userId)
    {
        var now = Now;
        var week = GuildDepthRules.WeekOf(now);
        var membership = await RequireMembershipAsync(userId);
        var dwarf = await RequireDwarfAsync(userId, week);
        var shaft = await EnsureShaftAsync(dwarf.GuildId, week);

        GuildDepthRules.Settle(dwarf, shaft.Level, now);
        dwarf.HeroId = null;
        dwarf.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, membership, now);
    }

    // Обменять руду со склада на золото
    public async Task<GuildDepthOverview> CollectAsync(Guid userId)
    {
        var now = Now;
        var week = GuildDepthRules.WeekOf(now);
        var membership = await RequireMembershipAsync(userId);
        var dwarf = await RequireDwarfAsync(userId, week);
        var shaft = await EnsureShaftAsync(dwarf.GuildId, week);
        var progress = await players.GetProgressAsync(dwarf.PlayerId);

        GuildDepthRules.Settle(dwarf, shaft.Level, now);
        long gold = DailyQuests.GoldFor(dwarf.Stored * GuildDepthRules.GoldStagesPerOre, progress);
        if (gold <= 0)
            throw new GuildDepthException(GuildDepthError.NothingToCollect, "Nothing to collect yet");

        dwarf.Stored = 0;
        dwarf.Version++;
        await using var transaction = await db.Database.BeginTransactionAsync();
        await SaveAsync();
        await AddGoldAsync(dwarf.PlayerId, gold);
        await transaction.CommitAsync();
        return await DescribeAsync(userId, membership, now);
    }

    // Личное улучшение: кирка своего дворфа
    public async Task<GuildDepthOverview> UpgradePickAsync(Guid userId)
    {
        var now = Now;
        var week = GuildDepthRules.WeekOf(now);
        var membership = await RequireMembershipAsync(userId);
        RequireActive(week, now);
        var dwarf = await RequireDwarfAsync(userId, week);
        var shaft = await EnsureShaftAsync(dwarf.GuildId, week);
        var progress = await players.GetProgressAsync(dwarf.PlayerId);

        double stages = GuildDepthRules.PickCostStages(dwarf.PickLevel)
            ?? throw new GuildDepthException(GuildDepthError.MaxLevel, "The pick is fully upgraded");
        long gold = Math.Max(1, DailyQuests.GoldFor(stages, progress));

        GuildDepthRules.Settle(dwarf, shaft.Level, now);
        dwarf.PickLevel++;
        dwarf.Version++;
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (!await TrySpendGoldAsync(dwarf.PlayerId, gold))
            throw new GuildDepthException(GuildDepthError.NotEnoughGold, $"The upgrade costs {gold} gold");
        await SaveAsync();
        await transaction.CommitAsync();
        return await DescribeAsync(userId, membership, now);
    }

    // Общее улучшение: подъёмник шахты гильдии — ускоряет всех её дворфов. Платит тот, кто улучшает
    public async Task<GuildDepthOverview> UpgradeShaftAsync(Guid userId)
    {
        var now = Now;
        var week = GuildDepthRules.WeekOf(now);
        var membership = await RequireMembershipAsync(userId);
        RequireActive(week, now);
        var playerId = await PlayerIdAsync(userId);
        var shaft = await EnsureShaftAsync(membership.GuildId, week);
        var progress = await players.GetProgressAsync(playerId);

        double stages = GuildDepthRules.ShaftCostStages(shaft.Level)
            ?? throw new GuildDepthException(GuildDepthError.MaxLevel, "The shaft is fully upgraded");
        long gold = Math.Max(1, DailyQuests.GoldFor(stages, progress));

        // Все дворфы гильдии досчитываются по старой скорости — новая действует с этой минуты
        var dwarves = await db.GuildDepthDwarves.Where(d => d.WeekStart == week && d.GuildId == membership.GuildId).ToListAsync();
        foreach (var dwarf in dwarves)
        {
            GuildDepthRules.Settle(dwarf, shaft.Level, now);
            dwarf.Version++;
        }
        shaft.Level++;
        shaft.Version++;

        await using var transaction = await db.Database.BeginTransactionAsync();
        if (!await TrySpendGoldAsync(playerId, gold))
            throw new GuildDepthException(GuildDepthError.NotEnoughGold, $"The upgrade costs {gold} gold");
        await SaveAsync();
        await transaction.CommitAsync();
        return await DescribeAsync(userId, membership, now);
    }

    // Награда за место гильдии: за закончившуюся гонку этой недели или прошлой
    public async Task<GuildDepthOverview> ClaimAsync(Guid userId)
    {
        var now = Now;
        var membership = await RequireMembershipAsync(userId);
        var pending = await PendingAsync(userId, now)
            ?? throw new GuildDepthException(GuildDepthError.NothingToClaim, "Nothing to claim");

        db.GuildDepthRewardClaims.Add(new GuildDepthRewardClaim
        {
            UserId = userId,
            WeekStart = pending.WeekStart,
            GuildId = pending.GuildId,
            Place = pending.Place,
            HonorBadges = pending.HonorBadges,
            ClaimedAt = now
        });
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new GuildDepthException(GuildDepthError.NothingToClaim, "The reward has already been claimed");
        }
        await players.AddCurrenciesAsync(await PlayerIdAsync(userId), 0, pending.HonorBadges);
        await transaction.CommitAsync();
        return await DescribeAsync(userId, membership, now);
    }

    /* ===== Рейтинг ===== */

    private record Standing(Guid GuildId, double Score, int Members, int Place);

    // Очки гильдий недели на момент now (не дальше конца гонки) — без записи
    private async Task<(List<Standing> Standings, List<GuildDepthDwarf> Dwarves, Dictionary<Guid, int> Shafts)> StandingsAsync(DateTime week, DateTime now)
    {
        var dwarves = await db.GuildDepthDwarves.AsNoTracking().Where(d => d.WeekStart == week).ToListAsync();
        var shafts = await db.GuildDepthShafts.AsNoTracking().Where(s => s.WeekStart == week).ToDictionaryAsync(s => s.GuildId, s => s.Level);
        foreach (var dwarf in dwarves)
            GuildDepthRules.Settle(dwarf, shafts.GetValueOrDefault(dwarf.GuildId, 1), now);

        var standings = dwarves
            .GroupBy(d => d.GuildId)
            .Select(g => (GuildId: g.Key, Score: g.Sum(d => d.Mined), Members: g.Count()))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select((x, index) => new Standing(x.GuildId, x.Score, x.Members, index + 1))
            .ToList();
        return (standings, dwarves, shafts);
    }

    private record Pending(DateTime WeekStart, Guid GuildId, int Place, int HonorBadges);

    // Сначала — гонка этой недели, если её итоги уже подведены, потом прошлая
    private async Task<Pending?> PendingAsync(Guid userId, DateTime now)
    {
        var current = GuildDepthRules.WeekOf(now);
        foreach (var week in new[] { current, current - GuildDepthRules.Week })
        {
            if (now < GuildDepthRules.EndOf(week))
                continue;
            var mine = await db.GuildDepthDwarves.AsNoTracking().FirstOrDefaultAsync(d => d.WeekStart == week && d.UserId == userId);
            if (mine == null || await db.GuildDepthRewardClaims.AnyAsync(c => c.UserId == userId && c.WeekStart == week))
                continue;
            var (standings, _, _) = await StandingsAsync(week, now);
            var standing = standings.FirstOrDefault(s => s.GuildId == mine.GuildId);
            if (standing == null)
                continue;
            return new Pending(week, mine.GuildId, standing.Place, GuildDepthRules.HonorBadgesFor(standing.Place));
        }
        return null;
    }

    /* ===== Ответ ===== */

    private async Task<GuildDepthOverview> DescribeAsync(Guid userId, GuildMember membership, DateTime now)
    {
        var week = GuildDepthRules.WeekOf(now);
        var (standings, dwarves, shafts) = await StandingsAsync(week, now);
        int shaftLevel = shafts.GetValueOrDefault(membership.GuildId, 1);
        var playerId = await PlayerIdAsync(userId);
        var progress = await players.GetProgressAsync(playerId);

        var guildIds = standings.Take(GuildDepthRules.LeaderboardSize).Select(s => s.GuildId).Append(membership.GuildId).Distinct().ToList();
        var guildNames = await db.Guilds.AsNoTracking().Where(g => guildIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name);

        var guildDwarves = dwarves.Where(d => d.GuildId == membership.GuildId).ToList();
        var userIds = guildDwarves.Select(d => d.UserId).ToList();
        var usernames = await db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username);
        var heroIds = guildDwarves.Where(d => d.HeroId != null).Select(d => d.HeroId!.Value).ToList();
        var heroesById = await db.Heroes.AsNoTracking().Where(h => heroIds.Contains(h.Id)).ToDictionaryAsync(h => h.Id);

        var mine = dwarves.FirstOrDefault(d => d.UserId == userId);
        var myStanding = standings.FirstOrDefault(s => s.GuildId == membership.GuildId);
        var pending = await PendingAsync(userId, now);
        double? pickStages = mine == null ? null : GuildDepthRules.PickCostStages(mine.PickLevel);
        double? shaftStages = GuildDepthRules.ShaftCostStages(shaftLevel);
        var myHero = mine?.HeroId is { } myHeroId
            ? heroesById.GetValueOrDefault(myHeroId) ?? await db.Heroes.AsNoTracking().FirstOrDefaultAsync(h => h.Id == myHeroId)
            : null;

        return new GuildDepthOverview
        {
            ServerTime = now,
            WeekStart = week,
            EndsAt = GuildDepthRules.EndOf(week),
            NextWeekAt = week + GuildDepthRules.Week,
            Active = GuildDepthRules.IsActive(week, now),
            ShaftLevel = shaftLevel,
            MaxShaftLevel = GuildDepthRules.MaxShaftLevel,
            ShaftFactor = GuildDepthRules.ShaftFactor(shaftLevel),
            NextShaftFactor = GuildDepthRules.ShaftFactor(shaftLevel + 1),
            ShaftCostGold = shaftStages is { } s ? Math.Max(1, DailyQuests.GoldFor(s, progress)) : null,
            MyDwarf = mine == null ? null : DescribeDwarf(mine, shafts.GetValueOrDefault(mine.GuildId, 1), myHero, progress, pickStages),
            PlacedElsewhere = mine != null && mine.GuildId != membership.GuildId,
            GuildScore = myStanding?.Score ?? 0,
            GuildPlace = myStanding?.Place ?? 0,
            GuildCount = standings.Count,
            Members = guildDwarves
                .OrderByDescending(d => d.Mined)
                .Select(d =>
                {
                    var hero = d.HeroId is { } id ? heroesById.GetValueOrDefault(id) : null;
                    return new GuildDepthMemberInfo
                    {
                        UserId = d.UserId,
                        Username = usernames.GetValueOrDefault(d.UserId, string.Empty),
                        HeroName = hero?.Name,
                        Rank = hero?.Level ?? 0,
                        Element = hero?.Element ?? HeroElement.Basic,
                        PickLevel = d.PickLevel,
                        Rate = GuildDepthRules.RateOf(d, shaftLevel),
                        Mined = d.Mined,
                        IsMe = d.UserId == userId
                    };
                })
                .ToList(),
            Leaderboard = standings
                .Take(GuildDepthRules.LeaderboardSize)
                .Select(s => new GuildDepthStandingInfo
                {
                    GuildId = s.GuildId,
                    Name = guildNames.GetValueOrDefault(s.GuildId, "—"),
                    Score = s.Score,
                    Members = s.Members,
                    Place = s.Place,
                    HonorBadges = GuildDepthRules.HonorBadgesFor(s.Place),
                    IsMine = s.GuildId == membership.GuildId
                })
                .ToList(),
            Rewards = new[] { 1, 2, 3, 4, 6, 11 }
                .Select(from => new GuildDepthRewardTier { FromPlace = from, HonorBadges = GuildDepthRules.HonorBadgesFor(from) })
                .ToList(),
            Pending = pending == null ? null : new GuildDepthPendingInfo
            {
                WeekStart = pending.WeekStart,
                Place = pending.Place,
                HonorBadges = pending.HonorBadges
            },
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }

    private static GuildDepthDwarfInfo DescribeDwarf(GuildDepthDwarf dwarf, int shaftLevel, Hero? hero, PlayerProgress progress, double? pickStages) => new()
    {
        HeroId = dwarf.HeroId,
        Name = hero?.Name,
        Rank = hero?.Level ?? 0,
        Element = hero?.Element ?? HeroElement.Basic,
        Profession = hero?.Profession,
        BaseRate = dwarf.HeroId == null ? 0 : dwarf.BaseRate,
        Rate = GuildDepthRules.RateOf(dwarf, shaftLevel),
        PickLevel = dwarf.PickLevel,
        MaxPickLevel = GuildDepthRules.MaxPickLevel,
        PickFactor = GuildDepthRules.PickFactor(dwarf.PickLevel),
        NextPickFactor = GuildDepthRules.PickFactor(dwarf.PickLevel + 1),
        PickCostGold = pickStages is { } s ? Math.Max(1, DailyQuests.GoldFor(s, progress)) : null,
        Mined = dwarf.Mined,
        Stored = dwarf.Stored,
        Storage = GuildDepthRules.StorageOf(dwarf, shaftLevel),
        StoredGold = DailyQuests.GoldFor(dwarf.Stored * GuildDepthRules.GoldStagesPerOre, progress),
        GoldPerHour = DailyQuests.GoldFor(GuildDepthRules.RateOf(dwarf, shaftLevel) * GuildDepthRules.GoldStagesPerOre, progress)
    };

    /* ===== Вспомогательное ===== */

    private async Task<GuildMember> RequireMembershipAsync(Guid userId) =>
        await guilds.GetMembershipAsync(userId)
            ?? throw new GuildDepthException(GuildDepthError.NotInGuild, "You are not in a guild");

    private static void RequireActive(DateTime week, DateTime now)
    {
        if (!GuildDepthRules.IsActive(week, now))
            throw new GuildDepthException(GuildDepthError.RaceNotActive, "The depth race is over for this week");
    }

    private async Task<GuildDepthDwarf> RequireDwarfAsync(Guid userId, DateTime week) =>
        await db.GuildDepthDwarves.FirstOrDefaultAsync(d => d.WeekStart == week && d.UserId == userId)
            ?? throw new GuildDepthException(GuildDepthError.NoDwarf, "You have no dwarf in the shaft");

    private async Task<Guid> PlayerIdAsync(Guid userId) =>
        await db.Users.Where(u => u.Id == userId).Select(u => u.PlayerId).FirstOrDefaultAsync();

    private async Task<GuildDepthShaft> EnsureShaftAsync(Guid guildId, DateTime week)
    {
        var shaft = await db.GuildDepthShafts.FirstOrDefaultAsync(s => s.GuildId == guildId && s.WeekStart == week);
        if (shaft != null)
            return shaft;
        shaft = new GuildDepthShaft { GuildId = guildId, WeekStart = week };
        db.GuildDepthShafts.Add(shaft);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Шахту только что создал параллельный запрос
            db.ChangeTracker.Clear();
            shaft = await db.GuildDepthShafts.FirstAsync(s => s.GuildId == guildId && s.WeekStart == week);
        }
        return shaft;
    }

    private async Task SaveAsync()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            throw new GuildDepthException(GuildDepthError.Changed, "The shaft has just changed, try again");
        }
        catch (DbUpdateException)
        {
            // Параллельный запрос успел поставить дворфа этой недели
            db.ChangeTracker.Clear();
            throw new GuildDepthException(GuildDepthError.Changed, "The shaft has just changed, try again");
        }
    }

    private Task<int> AddGoldAsync(Guid playerId, long gold) =>
        db.Players.Where(p => p.Id == playerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, p => p.Gold + gold));

    private async Task<bool> TrySpendGoldAsync(Guid playerId, long gold) =>
        await db.Players.Where(p => p.Id == playerId && p.Gold >= gold)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, p => p.Gold - gold)) == 1;
}
