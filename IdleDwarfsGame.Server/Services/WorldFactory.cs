public class WorldFactory : IWorldFactory
{
    private readonly IHeroesRepository _heroes;
    private readonly IEntityFactory _entityFactory;

    public WorldFactory(IHeroesRepository heroes, IEntityFactory entityFactory)
    {
        _heroes = heroes;
        _entityFactory = entityFactory;
    }

    public async Task<World> CreateWorldAsync(Guid playerId, ICombatScenario scenario, int level, List<IGlobalEffect> globalEffects)
    {
        var world = new World();

        // 1. Берём отряд игрока из БД
        var squad = await _heroes.GetSquadAsync(playerId);
        if (squad.Count == 0)
            throw new InvalidOperationException($"Player {playerId} has no heroes");

        // 2. Превращаем героев в ECS-сущности (команда 1)
        _entityFactory.CreateSquad(world, squad, teamId: 1);

        // 3. Сценарий спавнит врагов (команда 2)
        await scenario.PopulateEnemiesAsync(world, level);

        // 4. Добавляем глобальные эффекты
        foreach (var globalEffect in globalEffects)
        {
            world.AddGlobalEffect(globalEffect);
        }

        return world;
    }
}

public static class BalanceCalculator
{
    private const double LevelMultiplier = 1.2;

    public static int ScaleStat(int baseValue, int level)
    {
        if (level <= 1) return baseValue;

        // Округляем до ближайшего целого
        return (int)Math.Round(baseValue * Factor(level));
    }

    // Формула: multiplier ^ (level - 1)
    public static double Factor(int level) => level <= 1 ? 1 : Math.Pow(LevelMultiplier, level - 1);
}
