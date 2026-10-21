public interface IBossRepository
{
    Task<Hero> GetBoss(int playerId);
}