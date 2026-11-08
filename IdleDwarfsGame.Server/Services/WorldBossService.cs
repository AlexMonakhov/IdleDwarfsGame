// Причина отказа — фронт показывает по коду переведённый текст (worldBoss.errors.<code>)
public enum WorldBossError
{
    // Сейчас не неделя мировых боссов или ивент уже кончился (день лавки)
    EventNotActive,
    NoSquad,
    OnCooldown,
    NothingToClaim,
    // Босса только что убили или награды только что забрали — фронт перечитает окно
    Changed
}

public class WorldBossException(WorldBossError code, string message) : InvalidOperationException(message)
{
    public WorldBossError Code { get; } = code;
}

// Ивент «Мировые боссы» (правила — WorldBossRules). Босс на сервере один; следующего этапа создаёт первый запрос
// после убийства предыдущего, поэтому фоновая задача не нужна. Награды за убийство забираются кнопкой —
// посчитаны по урону игрока в этом боссе
public class WorldBossService(
    IWorldBossRepository bosses,
    IHeroesRepository heroes,
    IEntityFactory entityFactory,
    CombatService combatService,
    IFixedShopRepository wallets,
    IActivityRepository activity,
    TimeProvider clock)
{
    private const int SpawnAttempts = 3;

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    private static bool IsBossWeek(WorldEventWindow window) => window.Kind == WorldEventKind.WorldBosses;

    public async Task<WorldBossOverview> GetAsync(Guid playerId)
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        var boss = IsBossWeek(window) ? await EnsureBossAsync(window, now) : null;
        return await DescribeAsync(playerId, window, boss, now);
    }

    public async Task<WorldBossAttackResult> AttackAsync(Guid playerId)
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        if (!IsBossWeek(window) || !window.IsActive(now))
            throw new WorldBossException(WorldBossError.EventNotActive, "World bosses are not here this week");

        var boss = await EnsureBossAsync(window, now);
        if (boss == null || boss.DefeatedAt != null)
            throw new WorldBossException(WorldBossError.Changed, "The boss has just been defeated, try again");

        // Без отряда бой не начнётся — проверяем до отката, чтобы не сжечь удар впустую
        if ((await heroes.GetSquadAsync(playerId)).Count == 0)
            throw new WorldBossException(WorldBossError.NoSquad, "Your squad is empty");

        var variant = WorldBossRules.VariantOf(boss.Stage);
        var arena = WorldBossRules.ArenaOf(boss.Stage);
        var template = await heroes.GetByIdAsync(variant.TemplateHeroId)
            ?? throw new InvalidOperationException($"World boss template {variant.TemplateHeroId} not found");

        var readyAt = now + WorldBossRules.AttackCooldown;
        if (await bosses.TryStartCooldownAsync(playerId, now, readyAt) is { } blockedUntil)
            throw new WorldBossException(WorldBossError.OnCooldown, $"You can attack again at {AsUtc(blockedUntil):HH:mm} UTC");

        var scenario = new WorldBossScenario(entityFactory, template, WorldBossRules.LevelOf(boss.Stage), boss.MaxHp, boss.CurrentHp);
        var combat = await combatService.StartWorldBossFight(playerId, scenario, arena);
        var result = await bosses.ApplyDamageAsync(boss, playerId, boss.CurrentHp - scenario.BossHpLeft, WorldBossRules.AttackCurrency, now);

        return new WorldBossAttackResult
        {
            Stage = boss.Stage,
            Damage = result.AppliedDamage,
            Killed = result.Killed,
            BossHp = result.CurrentHp,
            MaxHp = boss.MaxHp,
            Currency = WorldBossRules.AttackCurrency,
            NextAttackAt = readyAt,
            Background = arena.Background,
            Arena = ArenaInfo.From(arena),
            Combat = combat
        };
    }

    // Забрать награды за всех убитых боссов ивента разом. Пока открыта лавка ивента — после неё валюта всё равно сгорит
    public async Task<WorldBossClaimResult> ClaimAsync(Guid playerId)
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        if (!IsBossWeek(window))
            throw new WorldBossException(WorldBossError.EventNotActive, "World bosses are not here this week");

        var pending = await PendingAsync(playerId, window.StartsAt);
        if (pending.Count == 0)
            throw new WorldBossException(WorldBossError.NothingToClaim, "Nothing to claim");

        var claims = pending
            .Select(p => new WorldBossRewardClaim { BossId = p.BossId, PlayerId = playerId, Currency = p.Currency, ClaimedAt = now })
            .ToList();
        if (!await bosses.ClaimAsync(playerId, window.StartsAt, claims))
            throw new WorldBossException(WorldBossError.Changed, "The rewards have just been claimed");

        var boss = await bosses.GetLatestAsync(window.StartsAt);
        return new WorldBossClaimResult
        {
            Currency = claims.Sum(c => c.Currency),
            Overview = await DescribeAsync(playerId, window, boss, now)
        };
    }

    /* ===== Босс ===== */

    // Живой босс ивента; если последнего убили — появляется следующий этап. После конца ивента новых нет
    private async Task<WorldBoss?> EnsureBossAsync(WorldEventWindow window, DateTime now)
    {
        for (int attempt = 0; attempt < SpawnAttempts; attempt++)
        {
            var latest = await bosses.GetLatestAsync(window.StartsAt);
            if (latest is { DefeatedAt: null } || !window.IsActive(now))
                return latest;

            int stage = (latest?.Stage ?? 0) + 1;
            int active = await activity.CountActivePlayersAsync(DailyQuests.DayOf(now - WorldBossRules.ActiveWindow));
            int hp = WorldBossRules.HpOf(stage, active);
            var boss = new WorldBoss
            {
                Id = Guid.NewGuid(),
                EventStart = window.StartsAt,
                Stage = stage,
                MaxHp = hp,
                CurrentHp = hp,
                SpawnedAt = now
            };
            if (await bosses.TryCreateAsync(boss))
                return boss;
        }
        return await bosses.GetLatestAsync(window.StartsAt);
    }

    private async Task<List<WorldBossPendingReward>> PendingAsync(Guid playerId, DateTime eventStart) =>
        (await bosses.GetUnclaimedKillsAsync(playerId, eventStart))
            .Select(x =>
            {
                bool lastHit = x.Boss.KilledByPlayerId == playerId;
                return new WorldBossPendingReward
                {
                    BossId = x.Boss.Id,
                    Stage = x.Boss.Stage,
                    Damage = x.Contribution.Damage,
                    LastHit = lastHit,
                    Currency = WorldBossRules.KillReward(x.Boss.Stage, x.Contribution.Damage, x.Boss.MaxHp, lastHit)
                };
            })
            .ToList();

    /* ===== Ответ ===== */

    private async Task<WorldBossOverview> DescribeAsync(Guid playerId, WorldEventWindow window, WorldBoss? boss, DateTime now)
    {
        var wallet = await wallets.GetEventWalletAsync(playerId, window.StartsAt);
        var readyAt = await bosses.GetCooldownAsync(playerId);
        bool bossWeek = IsBossWeek(window);
        var (myEventDamage, myRank) = bossWeek ? await bosses.GetEventRankAsync(window.StartsAt, playerId) : (0, 0);
        int stage = boss?.Stage ?? 1;

        return new WorldBossOverview
        {
            Event = WorldEventInfo.From(window, now, wallet),
            Open = bossWeek && window.IsActive(now),
            Boss = boss == null ? null : await DescribeBossAsync(playerId, boss),
            NextAttackAt = readyAt is { } ready && ready > now ? AsUtc(ready) : null,
            AttackCooldownMinutes = (int)WorldBossRules.AttackCooldown.TotalMinutes,
            AttackCurrency = WorldBossRules.AttackCurrency,
            KillBase = WorldBossRules.KillBase(stage),
            SharePool = WorldBossRules.SharePool(stage),
            LastHitBonus = WorldBossRules.LastHitBonus,
            Pending = bossWeek ? await PendingAsync(playerId, window.StartsAt) : [],
            Leaderboard = bossWeek
                ? (await bosses.GetEventTopAsync(window.StartsAt, WorldBossRules.LeaderboardSize)).Select(ToInfo).ToList()
                : [],
            MyEventDamage = myEventDamage,
            MyRank = myRank,
            ServerTime = now
        };
    }

    private async Task<WorldBossInfo> DescribeBossAsync(Guid playerId, WorldBoss boss)
    {
        var variant = WorldBossRules.VariantOf(boss.Stage);
        var arena = WorldBossRules.ArenaOf(boss.Stage);
        var template = await heroes.GetByIdAsync(variant.TemplateHeroId)
            ?? throw new InvalidOperationException($"World boss template {variant.TemplateHeroId} not found");
        int level = WorldBossRules.LevelOf(boss.Stage);

        // Портрет босса собирается той же фабрикой, что и в бою, чтобы статы совпадали
        var world = new World();
        new WorldBossScenario(entityFactory, template, level, boss.MaxHp, boss.CurrentHp).SpawnBoss(world);

        var top = await bosses.GetTopAsync(boss.Id, WorldBossRules.LeaderboardSize);
        long myDamage = (await bosses.GetContributionAsync(boss.Id, playerId))?.Damage ?? 0;

        return new WorldBossInfo
        {
            Id = boss.Id,
            Stage = boss.Stage,
            Unit = CombatSnapshot.Units(world).Single(),
            Level = level,
            Background = arena.Background,
            Arena = ArenaInfo.From(arena),
            MaxHp = boss.MaxHp,
            CurrentHp = boss.CurrentHp,
            DefeatedAt = boss.DefeatedAt is { } defeatedAt ? AsUtc(defeatedAt) : null,
            KilledBy = boss.KilledByPlayerId is { } killer ? top.FirstOrDefault(d => d.PlayerId == killer)?.Name : null,
            MyDamage = myDamage,
            MyKillReward = WorldBossRules.KillReward(boss.Stage, myDamage, boss.MaxHp, lastHit: false),
            Top = top.Select(ToInfo).ToList()
        };
    }

    private static WorldBossDamageInfo ToInfo(WorldBossDamageEntry entry) =>
        new() { PlayerId = entry.PlayerId, Name = entry.Name, Damage = entry.Damage };

    // БД возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
