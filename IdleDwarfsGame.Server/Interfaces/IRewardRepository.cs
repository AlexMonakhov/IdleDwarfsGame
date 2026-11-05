public interface IRewardRepository
{
    // Все незабранные награды игрока из источника, от старых к новым
    Task<List<PlayerReward>> GetUnclaimedAsync(Guid playerId, RewardSource source);

    // Забрать награду: золото и опыт — на аккаунт, сундуки — в инвентарь закрытыми.
    // null — награды нет, она чужая или её уже забрали
    Task<PlayerReward?> ClaimAsync(Guid playerId, Guid rewardId, DateTime now);

    // Забрать разом все незабранные награды источника — одной транзакцией.
    // Возвращает то, что забрано именно сейчас; пустой список — забирать нечего
    Task<List<PlayerReward>> ClaimAllAsync(Guid playerId, RewardSource source, DateTime now);
}
