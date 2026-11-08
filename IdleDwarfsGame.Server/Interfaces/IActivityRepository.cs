public interface IActivityRepository
{
    /// <summary>
    /// Adds <paramref name="amount"/> to the player's counter of <paramref name="kind"/> for <paramref name="day"/>,
    /// creating the counter on the first action of the day.
    /// </summary>
    Task AddAsync(Guid playerId, DateTime day, ActivityKind kind, int amount);

    /// <summary>
    /// Every counter of the player for the day.
    /// </summary>
    Task<Dictionary<ActivityKind, int>> GetDayAsync(Guid playerId, DateTime day);

    /// <summary>
    /// How many players did anything since <paramref name="sinceDay"/> — world events scale their bosses and deposits by it.
    /// </summary>
    Task<int> CountActivePlayersAsync(DateTime sinceDay);
}
