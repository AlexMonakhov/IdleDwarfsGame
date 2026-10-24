public interface IPlayerRepository
{
    Task<List<Hero>> GetHeroes(Guid playerId);
}