public interface IWorldFactory
{
    Task<World> CreateWorldAsync(Guid playerId, ICombatScenario scenario, int level);
}