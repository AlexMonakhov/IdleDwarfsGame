public class WorldFactory : IWorldFactory
{
    //private readonly ISquadRepository _squads;
    private readonly IEntityFactory _entityFactory;

    public WorldFactory(/*ISquadRepository squads, */IEntityFactory entityFactory)
    {
        //_squads = squads;
        _entityFactory = entityFactory;
    }

    public async Task<World> CreateWorldAsync(Guid playerId, ICombatScenario scenario, int level)
    {
        // 1. Берем сквад игрока
        //var playerSquad = await _squads.GetActiveSquadAsync(playerId);

        // 2. Спавним игрока в мир (твоя реализация в ECS)
        var world = new World();

        var heroes = new List<Hero>
        {
            new Hero{ Id = Guid.NewGuid(), Name = "Dwarf1", MaxHp = 100 , Attack = 10, Speed = 5 },
            new Hero{ Id = Guid.NewGuid(), Name = "Dwarf2", MaxHp = 120 , Attack = 8 , Speed = 4 }
        };

        var monsters = new List<MonsterStats>
        {
            new MonsterStats{  MaxHp = BalanceCalculator.ScaleStat(100, level), Attack = BalanceCalculator.ScaleStat(6, level), Speed = 6 },
            new MonsterStats{   MaxHp = BalanceCalculator.ScaleStat(150, level), Attack = BalanceCalculator.ScaleStat(12, level), Speed = 3 }
        };
        // TODO change to create squad
        _entityFactory.CreatePlayer(world, heroes);
        _entityFactory.CreateMonster(world, monsters);

        // 3. Сценарий спавнит врагов
        await scenario.PopulateEnemiesAsync(world, level);

        return world;
    }
}

public static class BalanceCalculator
{
    private const double LevelMultiplier = 1.2;

    public static int ScaleStat(int baseValue, int level)
    {
        if (level <= 1) return baseValue;

        // Формула: base * (multiplier ^ (level - 1))
        double scaledValue = baseValue * Math.Pow(LevelMultiplier, level - 1);

        // Округляем до ближайшего целого
        return (int)Math.Round(scaledValue);
    }
}