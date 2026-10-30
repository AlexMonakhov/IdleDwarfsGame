using System.Linq;

public class CombatService(
    IWorldFactory worldFactory,
    ICombatEngine combatEngine,
    IEnumerable<ICombatScenario> scenarios
)
{

    public async Task<CombatResult> StartTowerFight(Guid playerId, int floor)
    {
        var scenario = scenarios.OfType<TowerFloorScenario>().First();
        return await ProcessCombat(playerId, floor, scenario, new List<IGlobalEffect> { new LavaArenaEffect() });
    }

    public async Task<CombatResult> StartBossFight(Guid playerId, int difficulty)
    {
        var scenario = scenarios.OfType<BossScenario>().First();
        return await ProcessCombat(playerId, difficulty, scenario, new List<IGlobalEffect> { new LavaArenaEffect() });
    }

    // Эффект арены берётся из БД вместе с уровнем
    public async Task<CombatResult> StartCampaignFight(Guid playerId, CampaignStage stage)
    {
        var scenario = scenarios.OfType<CampaignScenario>().First();
        var globalEffects = new List<IGlobalEffect>();
        if (ArenaStatusEffect.FromArena(stage.Arena) is { } arenaEffect)
            globalEffects.Add(arenaEffect);

        return await ProcessCombat(playerId, stage.Id, scenario, globalEffects);
    }

    // Сценарий создаётся под бой (у него общий остаток HP босса); после боя сценарий знает, сколько HP осталось.
    // Бой идёт на арене босса — с её эффектом
    public async Task<CombatResult> StartGuildBossFight(Guid playerId, GuildBossScenario scenario, Arena arena)
    {
        var globalEffects = new List<IGlobalEffect>();
        if (ArenaStatusEffect.FromArena(arena) is { } arenaEffect)
            globalEffects.Add(arenaEffect);

        return await ProcessCombat(playerId, scenario.Definition.Level, scenario, globalEffects);
    }

    // Отряд игрока против сохранённого отряда другого игрока на арене PvP
    public async Task<CombatResult> StartPvpFight(Guid playerId, PvpScenario scenario)
    {
        return await ProcessCombat(playerId, 1, scenario, new List<IGlobalEffect> { new RisingFuryArenaEffect() });
    }

    // Рейтинговая арена: отряд нападавшего — не из БД, а закреплённый снимок, поэтому его команду 1
    // уже расставили в world; сценарий добавляет снимок защитника
    public async Task<CombatResult> StartRankedFight(World world, PvpScenario scenario)
    {
        await scenario.PopulateEnemiesAsync(world, 1);
        world.AddGlobalEffect(new RisingFuryArenaEffect());
        return await RunCombat(world, 1, scenario);
    }

    private async Task<CombatResult> ProcessCombat(Guid playerId, int level, ICombatScenario scenario, List<IGlobalEffect> globalEffects)
    {
        // 1. Собираем мир (WorldFactory достаёт героев из БД)
        var world = await worldFactory.CreateWorldAsync(playerId, scenario, level, globalEffects);
        return await RunCombat(world, level, scenario);
    }

    private async Task<CombatResult> RunCombat(World world, int level, ICombatScenario scenario)
    {
        // 2. Снимок участников до боя — фронт строит сцену по нему
        var units = CombatSnapshot.Units(world);

        // 3. Проводим бой
        var context = combatEngine.Run(world, scenario.MaxRounds);

        // 4. Награды режима для показа после боя. Кампания их не показывает: она выдаёт награду сама (CampaignRewards),
        //    и игрок забирает её отдельно
        RewardEntity? rewards = null;
        if (context.Win)
            rewards = await scenario.CalculateRewardsAsync(level, context.Logs);

        return new CombatResult
        {
            Units = units,
            Logs = context.Logs,
            WinningTeam = context.Win ? 1 : 2,
            Rounds = context.CurrentRound,
            Rewards = rewards
        };
    }
}
