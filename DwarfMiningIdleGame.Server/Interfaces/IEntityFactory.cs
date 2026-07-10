public interface IEntityFactory
{
    void CreatePlayer(World world, List<Hero> heroes);
    void CreateMonster(World world, List<MonsterStats> stats);
    void CreateBoss(World world, MonsterStats stats);
}