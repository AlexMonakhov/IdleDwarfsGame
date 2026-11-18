public interface IAchievementRepository
{
    /// <summary>
    /// How many tiers of each achievement the player has claimed (achievements without a row are absent).
    /// </summary>
    Task<Dictionary<string, int>> GetClaimedAsync(Guid playerId);

    /// <summary>
    /// Current values of every achievement metric: lifetime activity counters and account state.
    /// </summary>
    Task<AchievementStats> GetStatsAsync(Guid playerId);

    /// <summary>
    /// Moves the claimed tiers of <paramref name="key"/> from <paramref name="expectedClaimed"/> to <paramref name="claimedTiers"/>
    /// and grants the reward in one transaction. False — the tiers were changed by a parallel request, nothing granted.
    /// </summary>
    Task<bool> ClaimAsync(Guid playerId, string key, int expectedClaimed, int claimedTiers, PlayerReward reward,
        int rainbowCoins, int elderSeals, DateTime now);
}

public interface ILoginCalendarRepository
{
    Task<PlayerLoginCalendar?> GetAsync(Guid playerId);

    /// <summary>
    /// Saves <paramref name="next"/> (the row must still have <paramref name="expectedVersion"/>; null — must not exist yet)
    /// and grants the day's reward in one transaction. False — claimed by a parallel request, nothing granted.
    /// </summary>
    Task<bool> ClaimAsync(PlayerLoginCalendar next, int? expectedVersion, PlayerReward reward, int rainbowCoins, int elderSeals);
}
