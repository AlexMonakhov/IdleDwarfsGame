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

    // Кнопка «Забрать всё» в кампании и башне: все незабранные награды источника одной суммой
    public async Task<RewardClaimAllResult> ClaimAllAsync(Guid playerId, RewardSource source)
    {
        var claimed = await rewards.ClaimAllAsync(playerId, source, time.GetUtcNow().UtcDateTime);
        if (claimed.Count == 0)
            throw new InvalidOperationException("Nothing to claim");

        return new RewardClaimAllResult
        {
            RewardIds = claimed.Select(r => r.Id).ToList(),
            Reward = RewardInfo.Sum(claimed),
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }
}
