using System;
using System.Threading.Tasks;

public interface IRewardService
{
    Task<RewardEntity> GenerateAndSaveRewards(Guid playerId, ICombatScenario scenario, int level);
}