public interface IRewardRepository
{
    // Последняя незабранная награда игрока из источника
    Task<PlayerReward?> GetUnclaimedAsync(Guid playerId, RewardSource source);

    // Забрать награду: золото и опыт — на аккаунт, сундуки — в инвентарь закрытыми.
    // null — награды нет, она чужая или её уже забрали
    Task<PlayerReward?> ClaimAsync(Guid playerId, Guid rewardId, DateTime now);
}
