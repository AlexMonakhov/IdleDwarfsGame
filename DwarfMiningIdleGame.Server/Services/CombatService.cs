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

    private async Task<CombatResult> ProcessCombat(Guid playerId, int level, ICombatScenario scenario)
    {
        // 1. ������� ��� (������ WorldFactory ���������� ����� �� ��)
        var world = await worldFactory.CreateWorldAsync(playerId, scenario, level);

        // 2. �������� ���
        var context = combatEngine.Run(world);

        // 3. ��������� ������, ���� ������
        RewardEntity rewards = null;
        if (context.Win)
        {
            // ������� ������� �� ���� �������� � ������
            rewards = await rewardService.GenerateAndSaveRewards(playerId, scenario, level);
        }

        return new CombatResult
        {
            Logs = context.Logs,
            WinningTeam = context.Win ? 1 : 2
        };
    }
}