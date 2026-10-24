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
        return await ProcessCombat(playerId, floor, scenario);
    }

    public async Task<CombatResult> StartBossFight(Guid playerId, int difficulty)
    {
        var scenario = scenarios.OfType<BossScenario>().First();
        return await ProcessCombat(playerId, difficulty, scenario);
    }

    private async Task<CombatResult> ProcessCombat(Guid playerId, int level, ICombatScenario scenario)
    {
        // 1. Собираем мир (WorldFactory достаёт героев из БД)
        var world = await worldFactory.CreateWorldAsync(playerId, scenario, level, new List<IGlobalEffect> { new LavaArenaEffect() });

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
