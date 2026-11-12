using Microsoft.EntityFrameworkCore;

// Причина отказа — фронт показывает по коду переведённый текст (friends.help.errors.<code>)
public enum FriendHelpError
{
    NotFriends,
    MineNotFound,
    // В шахте нет дворфов — склад не копится, помогать нечему
    NoDwarves,
    MineFull,
    AlreadyHelped,
    HelpLimit,
    FriendHelpLimit,
    HireLimit,
    AlreadyHired,
    HeroNotFound,
    InvalidSlot,
    NoSquad
}

public class FriendHelpException(FriendHelpError code, string message) : InvalidOperationException(message)
{
    public FriendHelpError Code { get; } = code;
}

// Правила социальных действий между друзьями
public static class FriendHelpRules
{
    // Помощь в шахте: склад шахты друга получает столько часов её работы (не выше вместимости)
    public const double HelpHours = 0.5;
    public const int MaxHelpsGiven = 5;
    public const int MaxHelpsReceived = 10;
    // Помощнику — золото в уровнях кампании (DailyQuests.GoldFor)
    public const double HelperGoldStages = 0.25;

    // Наёмники: столько боёв с наёмником в сутки; другу за каждого — печати старейшин, но не больше чем за MaxLendRewards
    public const int MaxHiresPerDay = 3;
    public const int LenderElderSeals = 5;
    public const int MaxLendRewards = 5;
    public const int SquadSlots = 5;

    public static DateTime DayOf(DateTime utc) => utc.Date;
}

// Друзья помогают друг другу: стучат киркой в шахте друга и берут его героев наёмниками в кампанию.
// Концепт: сервис работает с GameDbContext напрямую, без отдельного репозитория
public class FriendHelpService(
    GameDbContext db,
    IMiningRepository mining,
    IHeroesRepository heroes,
    IPlayerRepository players,
    IEntityFactory entityFactory,
    ICombatEngine engine,
    IEnumerable<ICombatScenario> scenarios,
    CampaignService campaign,
    TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    /* ===== Шахты друга ===== */

    public async Task<FriendMinesOverview> GetMinesAsync(Guid userId, Guid friendId)
    {
        await RequireFriendAsync(userId, friendId);
        var now = Now;
        var day = FriendHelpRules.DayOf(now);
        var friendPlayerId = await PlayerIdAsync(friendId);
        var mines = await mining.GetMinesAsync(friendPlayerId);

        return new FriendMinesOverview
        {
            FriendId = friendId,
            FriendName = await db.Users.Where(u => u.Id == friendId).Select(u => u.Username).FirstAsync(),
            HelpHours = FriendHelpRules.HelpHours,
            HelpedToday = await db.FriendMineHelps.AnyAsync(h => h.HelperUserId == userId && h.TargetUserId == friendId && h.Day == day),
            HelpsGiven = await db.FriendMineHelps.CountAsync(h => h.HelperUserId == userId && h.Day == day),
            MaxHelpsGiven = FriendHelpRules.MaxHelpsGiven,
            FriendHelpsReceived = await db.FriendMineHelps.CountAsync(h => h.TargetUserId == friendId && h.Day == day),
            MaxHelpsReceived = FriendHelpRules.MaxHelpsReceived,
            Mines = mines.Select(m =>
            {
                double stored = MiningService.StoredAt(m, now);
                int capacity = MiningService.MineCapacity(m);
                return new FriendMineInfo
                {
                    MineId = m.Id,
                    Plot = m.Plot,
                    Type = m.Type,
                    Level = m.Level,
                    Dwarves = m.Dwarves.Count,
                    Stored = Math.Floor(stored),
                    Capacity = capacity,
                    HelpAmount = m.Dwarves.Count == 0 ? 0 : Math.Floor(Math.Min(capacity, stored + HelpAmount(m)) - stored)
                };
            }).ToList()
        };
    }

    public async Task<FriendHelpResult> HelpMineAsync(Guid userId, Guid friendId, Guid mineId)
    {
        await RequireFriendAsync(userId, friendId);
        var now = Now;
        var day = FriendHelpRules.DayOf(now);

        if (await db.FriendMineHelps.AnyAsync(h => h.HelperUserId == userId && h.TargetUserId == friendId && h.Day == day))
            throw new FriendHelpException(FriendHelpError.AlreadyHelped, "You have already helped this friend today");
        if (await db.FriendMineHelps.CountAsync(h => h.HelperUserId == userId && h.Day == day) >= FriendHelpRules.MaxHelpsGiven)
            throw new FriendHelpException(FriendHelpError.HelpLimit, $"Up to {FriendHelpRules.MaxHelpsGiven} helps a day");
        if (await db.FriendMineHelps.CountAsync(h => h.TargetUserId == friendId && h.Day == day) >= FriendHelpRules.MaxHelpsReceived)
            throw new FriendHelpException(FriendHelpError.FriendHelpLimit, "Your friend has had enough help today");

        var friendPlayerId = await PlayerIdAsync(friendId);
        var mine = await mining.GetAsync(friendPlayerId, mineId)
            ?? throw new FriendHelpException(FriendHelpError.MineNotFound, "Mine not found");
        if (mine.Dwarves.Count == 0)
            throw new FriendHelpException(FriendHelpError.NoDwarves, "No dwarves work in this mine");

        double stored = MiningService.StoredAt(mine, now);
        double next = Math.Min(MiningService.MineCapacity(mine), stored + HelpAmount(mine));
        if (next - stored < 1)
            throw new FriendHelpException(FriendHelpError.MineFull, "This mine's storage is full");

        var myPlayerId = await PlayerIdAsync(userId);
        long gold = DailyQuests.GoldFor(FriendHelpRules.HelperGoldStages, await players.GetProgressAsync(myPlayerId));

        await using var transaction = await db.Database.BeginTransactionAsync();
        // Склад шахты меняем только если её не поменяли параллельно (Version) — иначе пусть друг попробует ещё раз
        int updated = await db.Mines
            .Where(m => m.Id == mine.Id && m.Version == mine.Version)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Stored, next)
                .SetProperty(m => m.UpdatedAt, now)
                .SetProperty(m => m.Version, m => m.Version + 1));
        if (updated == 0)
            throw new FriendHelpException(FriendHelpError.MineFull, "The mine has just changed, try again");

        db.FriendMineHelps.Add(new FriendMineHelp
        {
            HelperUserId = userId,
            TargetUserId = friendId,
            Day = day,
            MineId = mine.Id,
            Amount = next - stored,
            HelperGold = gold,
            HelpedAt = now
        });
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new FriendHelpException(FriendHelpError.AlreadyHelped, "You have already helped this friend today");
        }
        if (gold > 0)
            await db.Players.Where(p => p.Id == myPlayerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, p => p.Gold + gold));
        await transaction.CommitAsync();

        return new FriendHelpResult
        {
            Added = Math.Floor(next - stored),
            Gold = gold,
            Resources = await players.GetResourcesAsync(myPlayerId) ?? new PlayerResources(),
            Mines = await GetMinesAsync(userId, friendId)
        };
    }

    private static double HelpAmount(Mine mine) => MiningService.MineRate(mine) * FriendHelpRules.HelpHours;

    /* ===== Наёмники ===== */

    public async Task<MercenaryOverview> GetMercenariesAsync(Guid userId)
    {
        var day = FriendHelpRules.DayOf(Now);
        var hires = await db.FriendHires.AsNoTracking().Where(h => h.BorrowerUserId == userId && h.Day == day).ToListAsync();
        var friends = await db.Friendships.AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => new { f.FriendId, f.Friend.Username, f.Friend.PlayerId })
            .ToListAsync();

        var result = new MercenaryOverview
        {
            HiresLeft = Math.Max(0, FriendHelpRules.MaxHiresPerDay - hires.Count),
            MaxHires = FriendHelpRules.MaxHiresPerDay,
            LenderElderSeals = FriendHelpRules.LenderElderSeals
        };
        foreach (var friend in friends)
        {
            var squad = await heroes.GetSquadAsync(friend.PlayerId);
            if (squad.Count == 0)
                continue;
            var world = new World();
            entityFactory.CreateSquad(world, squad, teamId: 1);
            var units = CombatSnapshot.Units(world);
            result.Friends.Add(new MercenaryFriendInfo
            {
                UserId = friend.FriendId,
                Username = friend.Username,
                HiredToday = hires.Any(h => h.LenderUserId == friend.FriendId),
                Heroes = squad.Zip(units, (hero, unit) => new MercenaryHeroInfo { HeroId = hero.Id, Unit = unit }).ToList()
            });
        }
        result.Friends = result.Friends.OrderBy(f => f.HiredToday).ThenBy(f => f.Username, StringComparer.OrdinalIgnoreCase).ToList();
        return result;
    }

    // Бой кампании, где герой друга занимает место slot в отряде (или пустое место)
    public async Task<MercenaryFightResult> FightCampaignAsync(Guid userId, int stageId, Guid lenderUserId, Guid heroId, int slot)
    {
        await RequireFriendAsync(userId, lenderUserId);
        if (slot < 1 || slot > FriendHelpRules.SquadSlots)
            throw new FriendHelpException(FriendHelpError.InvalidSlot, $"Slot must be from 1 to {FriendHelpRules.SquadSlots}");

        var now = Now;
        var day = FriendHelpRules.DayOf(now);
        if (await db.FriendHires.AnyAsync(h => h.BorrowerUserId == userId && h.LenderUserId == lenderUserId && h.Day == day))
            throw new FriendHelpException(FriendHelpError.AlreadyHired, "You have already hired a hero of this friend today");
        if (await db.FriendHires.CountAsync(h => h.BorrowerUserId == userId && h.Day == day) >= FriendHelpRules.MaxHiresPerDay)
            throw new FriendHelpException(FriendHelpError.HireLimit, $"Up to {FriendHelpRules.MaxHiresPerDay} mercenaries a day");

        var lenderSquad = await heroes.GetSquadAsync(await PlayerIdAsync(lenderUserId));
        var hired = lenderSquad.FirstOrDefault(h => h.Id == heroId)
            ?? throw new FriendHelpException(FriendHelpError.HeroNotFound, "This hero is not in your friend's squad");

        var myPlayerId = await PlayerIdAsync(userId);
        var mySquad = await heroes.GetSquadAsync(myPlayerId);
        // Копия героя друга на нужном месте: сущность из БД не трогаем, чтобы её место в отряде друга не сохранилось
        var mercenary = hired.WithUpgradeLevel(hired.UpgradeLevel);
        mercenary.PlaceInSquad = slot;
        var squad = mySquad
            .Select((h, index) => (Hero: h, Place: h.PlaceInSquad ?? index + 1))
            .Where(x => x.Place != slot)
            .Select(x => x.Hero)
            .Append(mercenary)
            .ToList();

        var fight = await campaign.FightAsync(myPlayerId, stageId, stage =>
        {
            var world = new World();
            entityFactory.CreateSquad(world, squad, teamId: 1);
            var scenario = scenarios.OfType<CampaignScenario>().First();
            scenario.SpawnEnemies(world, stage);
            PreparedFight.AddArena(world, stage.Arena);
            return Task.FromResult(PreparedFight.Run(engine, world, ((ICombatScenario)scenario).MaxRounds));
        });

        // Бой состоялся — наёмник потрачен. Другу — печати, пока не набрал дневной предел
        bool reward = await db.FriendHires.CountAsync(h => h.LenderUserId == lenderUserId && h.Day == day && h.LenderRewarded) < FriendHelpRules.MaxLendRewards;
        db.FriendHires.Add(new FriendHire
        {
            BorrowerUserId = userId,
            LenderUserId = lenderUserId,
            Day = day,
            HeroId = hired.Id,
            HeroName = hired.Name,
            StageId = stageId,
            LenderRewarded = reward,
            HiredAt = now
        });
        try
        {
            await db.SaveChangesAsync();
            if (reward)
            {
                var lenderPlayerId = await PlayerIdAsync(lenderUserId);
                await db.Players.Where(p => p.Id == lenderPlayerId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.ElderSeals, p => p.ElderSeals + FriendHelpRules.LenderElderSeals));
            }
        }
        catch (DbUpdateException)
        {
            // Параллельный бой с тем же наёмником уже записан — второй бой засчитан, но друг получит печати один раз
            db.ChangeTracker.Clear();
        }

        return new MercenaryFightResult { Fight = fight, MercenaryName = hired.Name, Slot = slot };
    }

    /* ===== Вспомогательное ===== */

    private async Task RequireFriendAsync(Guid userId, Guid friendId)
    {
        if (!await db.Friendships.AnyAsync(f => f.UserId == userId && f.FriendId == friendId))
            throw new FriendHelpException(FriendHelpError.NotFriends, "This player is not your friend");
    }

    private async Task<Guid> PlayerIdAsync(Guid userId) =>
        await db.Users.Where(u => u.Id == userId).Select(u => u.PlayerId).FirstOrDefaultAsync();
}
