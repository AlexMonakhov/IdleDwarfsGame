public interface IEntityFactory
{
    // Создаёт ECS-сущность из героя (игрока или врага). level > 1 масштабирует HP/атаку.
    int CreateUnit(World world, Hero hero, int teamId, int position, int level = 1);

    // Создаёт весь отряд; позиции берутся из PlaceInSquad или по порядку
    List<int> CreateSquad(World world, IEnumerable<Hero> heroes, int teamId, int level = 1);
}
