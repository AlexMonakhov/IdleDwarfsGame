public interface IPlayerRepository
{
    Task<List<Hero>> GetHeroes(Guid playerId);
    /// <summary>
    /// Account-wide resources (gold, experience), read straight from the database. Null if the player doesn't exist.
    /// </summary>
    Task<PlayerResources?> GetResourcesAsync(Guid playerId);
}
