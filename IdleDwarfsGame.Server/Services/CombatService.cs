using System.Linq;

public class CombatService(
    IWorldFactory worldFactory,
    ICombatEngine combatEngine,
    IEnumerable<ICombatScenario> scenarios,
    IRewardService rewardService
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

    private async Task<CombatResult> ProcessCombat(Guid playerId, int level, ICombatScenario scenario, List<IGlobalEffect> globalEffects)
    {
        // 1. Собираем мир (WorldFactory достаёт героев из БД)
        var world = await worldFactory.CreateWorldAsync(playerId, scenario, level, globalEffects);

        // 2. Снимок участников до боя — фронт строит сцену по нему
        var units = CombatSnapshot.Units(world);

        // 3. Проводим бой
        var context = combatEngine.Run(world);

        // 4. Начисляем награды, если победа
        RewardEntity? rewards = null;
        if (context.Win)
        {
            await rewardService.GenerateAndSaveRewards(playerId, scenario, level);
            rewards = await scenario.CalculateRewardsAsync(level, context.Logs);
        }

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
