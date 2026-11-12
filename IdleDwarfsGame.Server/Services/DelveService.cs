using Microsoft.EntityFrameworkCore;

// Причина отказа — фронт показывает по коду переведённый текст (delve.errors.<code>)
public enum DelveError
{
    NoSquad,
    AlreadyRanToday,
    NoActiveRun,
    // Сначала нужно выбрать реликвию или решить, что делать на привале
    ChoicePending,
    NothingPending,
    InvalidMove,
    InvalidRelic,
    InvalidCampAction,
    NoFallen,
    Changed
}

public class DelveException(DelveError code, string message) : InvalidOperationException(message)
{
    public DelveError Code { get; } = code;
}

// «Глубинный спуск» (правила — DelveRules). Концепт: сервис работает с GameDbContext напрямую.
// Бой собирается вручную: отряд-снимок с сохранённым HP и реликвиями против монстров уровня кампании
public class DelveService(
    GameDbContext db,
    IHeroesRepository heroes,
    ICampaignRepository campaign,
    IPlayerRepository players,
    IEntityFactory entityFactory,
    ICombatEngine engine,
    IEnumerable<ICombatScenario> scenarios,
    TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<DelveOverview> GetAsync(Guid userId) => await DescribeAsync(userId, Now);

    public async Task<DelveOverview> StartAsync(Guid userId)
    {
        var now = Now;
        var day = DelveRules.DayOf(now);
        var playerId = await PlayerIdAsync(userId);
        if (await db.DelveRuns.AnyAsync(r => r.UserId == userId && r.Day == day))
            throw new DelveException(DelveError.AlreadyRanToday, "You have already delved today");

        var squad = await heroes.GetSquadAsync(playerId);
        if (squad.Count == 0)
            throw new DelveException(DelveError.NoSquad, "Your squad is empty");

        // Незаконченный вчерашний забег закрывается: глубина засчитана, активный забег всегда один
        await db.DelveRuns
            .Where(r => r.UserId == userId && r.Status == DelveRunStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, DelveRunStatus.Retreated)
                .SetProperty(r => r.Pending, DelvePending.None)
                .SetProperty(r => r.EndedAt, now));

        var run = new DelveRun
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerId = playerId,
            WeekStart = DelveRules.WeekOf(now),
            Day = day,
            Status = DelveRunStatus.Active,
            SquadJson = RankedSquad.Serialize(squad),
            StartedAt = now
        };
        run.SetHp(squad.ToDictionary(h => h.Id, _ => 1.0));
        db.DelveRuns.Add(run);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new DelveException(DelveError.AlreadyRanToday, "You have already delved today");
        }
        return await DescribeAsync(userId, now);
    }

    // Спуститься в узел следующего этажа. Бой — сразу проводится, привал и сокровищница — ждут решения
    public async Task<DelveMoveResult> MoveAsync(Guid userId, int column)
    {
        var now = Now;
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.None)
            throw new DelveException(DelveError.ChoicePending, "Make your choice first");

        var node = DelveRules.MapOf(run.WeekStart).FirstOrDefault(n => n.Floor == run.Floor + 1 && n.Column == column);
        if (node == null || !DelveRules.CanMove(run.Floor, run.Column, node))
            throw new DelveException(DelveError.InvalidMove, "You can't go there");

        run.Floor = node.Floor;
        run.Column = node.Column;
        run.Version++;
        var result = new DelveMoveResult { Node = node.Type };

        switch (node.Type)
        {
            case DelveNodeType.Camp:
                run.Pending = DelvePending.Camp;
                break;
            case DelveNodeType.Treasure:
                OfferRelics(run);
                break;
            default:
                await FightAsync(run, node, now, result);
                break;
        }

        await SaveAsync();
        if (result.Gold > 0)
            await db.Players.Where(p => p.Id == run.PlayerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, p => p.Gold + result.Gold));
        result.Overview = await DescribeAsync(userId, now);
        return result;
    }

    public async Task<DelveOverview> ChooseRelicAsync(Guid userId, string key)
    {
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.Relic)
            throw new DelveException(DelveError.NothingPending, "There is nothing to choose");
        if (!run.Offer().Contains(key))
            throw new DelveException(DelveError.InvalidRelic, "This relic is not offered");

        var relics = run.Relics();
        relics.Add(key);
        run.SetRelics(relics);
        // Феникс действует сразу: павшие встают
        if (key == "phoenix")
            run.SetHp(run.Hp().ToDictionary(x => x.Key, x => x.Value <= 0 ? DelveRules.PhoenixHp : x.Value));
        run.Pending = DelvePending.None;
        run.SetOffer([]);
        run.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, Now);
    }

    // Привал: rest — лечение живых, revive — воскресить одного павшего (heroId)
    public async Task<DelveOverview> CampAsync(Guid userId, string action, Guid? heroId)
    {
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.Camp)
            throw new DelveException(DelveError.NothingPending, "You are not at a camp");

        var hp = run.Hp();
        if (action == "rest")
        {
            double heal = DelveRules.CampHeal + (run.Relics().Contains("ale") ? DelveRules.AleHeal : 0);
            hp = hp.ToDictionary(x => x.Key, x => x.Value > 0 ? Math.Min(1, x.Value + heal) : 0);
        }
        else if (action == "revive")
        {
            var fallen = heroId is { } id && hp.TryGetValue(id, out var value) && value <= 0 ? id
                : throw new DelveException(DelveError.NoFallen, "Choose a fallen hero");
            hp[fallen] = DelveRules.CampRevive;
        }
        else
        {
            throw new DelveException(DelveError.InvalidCampAction, "Unknown camp action");
        }

        run.SetHp(hp);
        run.Pending = DelvePending.None;
        run.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, Now);
    }

    // Выйти из спуска: забег окончен, глубина засчитана
    public async Task<DelveOverview> RetreatAsync(Guid userId)
    {
        var now = Now;
        var run = await RequireActiveAsync(userId);
        run.Status = DelveRunStatus.Retreated;
        run.Pending = DelvePending.None;
        run.EndedAt = now;
        run.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, now);
    }

    /* ===== Бой ===== */

    private async Task FightAsync(DelveRun run, DelveNode node, DateTime now, DelveMoveResult result)
    {
        var progress = await players.GetProgressAsync(run.PlayerId);
        int maxStage = (await campaign.GetStageIdsAsync()).DefaultIfEmpty(1).Max();
        int stageId = DelveRules.StageFor(progress.CurrentStage?.Id ?? 1, node, maxStage);
        var stage = await campaign.GetStageAsync(stageId)
            ?? throw new InvalidOperationException($"Campaign stage {stageId} not found");

        var world = BuildSquadWorld(run, node.Type is DelveNodeType.Elite or DelveNodeType.Boss);
        var scenario = scenarios.OfType<CampaignScenario>().First();
        int before = world.Entities.Count;
        scenario.SpawnEnemies(world, stage);
        double hpFactor = node.Type switch { DelveNodeType.Elite => DelveRules.EliteHp, DelveNodeType.Boss => DelveRules.BossHp, _ => 1 };
        foreach (var enemy in world.Entities.Skip(before))
        {
            var health = world.Health[enemy.Id];
            health.Max = Math.Max(1, (int)Math.Round(health.Max * hpFactor));
            health.Current = health.Max;
            if (node.Type == DelveNodeType.Boss)
                world.Info[enemy.Id].IsBoss = true;
        }
        PreparedFight.AddArena(world, stage.Arena);

        var combat = PreparedFight.Run(engine, world, ((ICombatScenario)scenario).MaxRounds);
        result.Combat = combat;
        result.Background = stage.Arena?.Background ?? "lava";
        result.Won = combat.WinningTeam == 1;

        // HP после боя — доля от максимума с реликвиями; павшие — 0
        var hp = run.Hp();
        foreach (var entity in world.Entities)
        {
            var info = world.Info[entity.Id];
            if (world.Team[entity.Id].TeamId != 1 || info.HeroId is not { } heroId)
                continue;
            var health = world.Health[entity.Id];
            hp[heroId] = health.Max > 0 ? Math.Clamp((double)health.Current / health.Max, 0, 1) : 0;
        }

        if (result.Won == true)
        {
            if (run.Relics().Contains("bloodthirst"))
                hp = hp.ToDictionary(x => x.Key, x => x.Value > 0 ? Math.Min(1, x.Value + DelveRules.BloodthirstHeal) : 0);
            result.Gold = DailyQuests.GoldFor(DelveRules.GoldStagesFor(node), progress);
            run.GoldEarned += result.Gold;
            if (node.Type == DelveNodeType.Boss)
            {
                run.Status = DelveRunStatus.Completed;
                run.EndedAt = now;
            }
            else if (node.Type == DelveNodeType.Elite)
            {
                OfferRelics(run);
            }
        }
        else
        {
            // Поражение — весь отряд пал (или кончились раунды): забег окончен на этом этаже
            hp = hp.ToDictionary(x => x.Key, _ => 0.0);
            run.Status = DelveRunStatus.Dead;
            run.EndedAt = now;
            // Этаж не пройден — в рейтинг идёт предыдущий
            run.Floor--;
        }
        run.SetHp(hp);
    }

    private static void OfferRelics(DelveRun run)
    {
        var offer = DelveRules.OfferFor(run.Id, run.Floor, run.Relics());
        if (offer.Count == 0)
            return;
        run.SetOffer(offer);
        run.Pending = DelvePending.Relic;
    }

    // Отряд забега: живые герои снимка с их HP и реликвиями. strongFoe — бой с элитой или боссом (истребитель гигантов)
    private World BuildSquadWorld(DelveRun run, bool strongFoe)
    {
        var world = new World();
        var hp = run.Hp();
        var alive = run.Squad().Where(h => hp.GetValueOrDefault(h.Id, 1) > 0).ToList();
        entityFactory.CreateSquad(world, alive, teamId: 1);

        var relics = run.Relics();
        double attack = 1 + (relics.Contains("rage") ? DelveRules.RageAttack : 0) + (strongFoe && relics.Contains("giantslayer") ? DelveRules.GiantslayerAttack : 0);
        double maxHp = 1 + (relics.Contains("stoneskin") ? DelveRules.StoneskinHp : 0);
        foreach (var entity in world.Entities)
        {
            var info = world.Info[entity.Id];
            var health = world.Health[entity.Id];
            health.Max = (int)Math.Round(health.Max * maxHp);
            health.Current = Math.Max(1, (int)Math.Round(health.Max * hp.GetValueOrDefault(info.HeroId ?? Guid.Empty, 1)));

            var atk = world.Attack[entity.Id];
            atk.Damage = (int)Math.Round(atk.Damage * attack);
            atk.Base = atk.Damage;

            if (relics.Contains("swiftboots"))
                world.Speed[entity.Id].Value += DelveRules.SwiftSpeed;
            if (relics.Contains("spark"))
            {
                var energy = world.Energy[entity.Id];
                energy.Current = Math.Min(energy.Max, energy.Current + (int)Math.Round(energy.Max * DelveRules.SparkEnergy));
            }
        }
        return world;
    }

    /* ===== Ответ ===== */

    private async Task<DelveOverview> DescribeAsync(Guid userId, DateTime now)
    {
        var week = DelveRules.WeekOf(now);
        var day = DelveRules.DayOf(now);
        var run = await db.DelveRuns.AsNoTracking()
            .Where(r => r.UserId == userId && r.WeekStart == week)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync();

        var best = await db.DelveRuns.AsNoTracking()
            .Where(r => r.WeekStart == week)
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, Floor = g.Max(r => r.Floor) })
            .Where(x => x.Floor > 0)
            .OrderByDescending(x => x.Floor)
            .Take(DelveRules.LeaderboardSize)
            .ToListAsync();
        var ids = best.Select(b => b.UserId).ToList();
        var names = await db.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username);

        DelveRunInfo? runInfo = null;
        if (run != null)
        {
            var hp = run.Hp();
            var world = BuildSquadWorld(run, false);
            var units = CombatSnapshot.Units(world);
            // Павших в мире нет — показываем их отдельно с нулевым HP
            var fallen = run.Squad().Where(h => hp.GetValueOrDefault(h.Id, 1) <= 0)
                .Select(h => new DelveFallenInfo { HeroId = h.Id, Name = h.Name, UnitType = h.UnitType, Element = h.Element, Level = h.Level })
                .ToList();
            runInfo = new DelveRunInfo
            {
                Id = run.Id,
                Status = run.Status,
                Floor = run.Floor,
                Column = run.Column,
                Pending = run.Pending,
                Offer = run.Offer(),
                Relics = run.Relics(),
                Units = units,
                Fallen = fallen,
                GoldEarned = run.GoldEarned,
                IsToday = run.Day == day
            };
        }

        return new DelveOverview
        {
            ServerTime = now,
            WeekStart = week,
            NextDayAt = day.AddDays(1),
            Floors = DelveRules.Floors,
            Columns = DelveRules.Columns,
            Map = DelveRules.MapOf(week).Select(n => new DelveNodeInfo { Floor = n.Floor, Column = n.Column, Type = n.Type }).ToList(),
            Run = runInfo,
            CanStart = run?.Day != day,
            BestFloor = await db.DelveRuns.Where(r => r.UserId == userId && r.WeekStart == week).MaxAsync(r => (int?)r.Floor) ?? 0,
            Leaderboard = best.Select((b, i) => new DelveLeaderInfo
            {
                Place = i + 1,
                UserId = b.UserId,
                Username = names.GetValueOrDefault(b.UserId, string.Empty),
                Floor = b.Floor,
                IsMe = b.UserId == userId
            }).ToList(),
            Relics = DelveRules.Relics.Select(r => new DelveRelicInfo { Key = r.Key, Rare = r.Rare }).ToList()
        };
    }

    /* ===== Вспомогательное ===== */

    private async Task<DelveRun> RequireActiveAsync(Guid userId) =>
        await db.DelveRuns.FirstOrDefaultAsync(r => r.UserId == userId && r.Status == DelveRunStatus.Active)
            ?? throw new DelveException(DelveError.NoActiveRun, "You have no active delve");

    private async Task SaveAsync()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            throw new DelveException(DelveError.Changed, "The delve has just changed, try again");
        }
    }

    private async Task<Guid> PlayerIdAsync(Guid userId) =>
        await db.Users.Where(u => u.Id == userId).Select(u => u.PlayerId).FirstOrDefaultAsync();
}
