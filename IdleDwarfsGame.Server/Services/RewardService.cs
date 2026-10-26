// Получение наград, выданных за бои (PlayerRewards): до этого момента их нет ни на аккаунте, ни в UI
public class RewardService(IRewardRepository rewards, IPlayerRepository players, TimeProvider time)
{
    public async Task<RewardClaimResult> ClaimAsync(Guid playerId, Guid rewardId)
    {
        var reward = await rewards.ClaimAsync(playerId, rewardId, time.GetUtcNow().UtcDateTime)
            ?? throw new InvalidOperationException("Reward not found or already claimed");

        return new RewardClaimResult
        {
            Reward = RewardInfo.From(reward),
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }
}
