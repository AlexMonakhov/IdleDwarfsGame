public interface IPlayerRepository
{
    Task<List<Hero>> GetHeroes(Guid playerId);
    /// <summary>
    /// Account-wide resources (gold, experience), read straight from the database. Null if the player doesn't exist.
    /// </summary>
    Task<PlayerResources?> GetResourcesAsync(Guid playerId);
    /// <summary>
    /// Credits shop currencies earned outside the shop: blood coins (arena) and honor badges (guild bosses).
    /// </summary>
    Task AddCurrenciesAsync(Guid playerId, long bloodCoins, long honorBadges);
    /// <summary>
    /// How far the player got: cleared campaign stages, the current stage (rewards scale with it) and guild membership.
    /// Daily quests and fixed shops depend on it.
    /// </summary>
    Task<PlayerProgress> GetProgressAsync(Guid playerId);
}
