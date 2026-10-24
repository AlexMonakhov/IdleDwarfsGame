public class TowerFloorScenario : ICombatScenario
{
    public CombatMode Mode => CombatMode.Tower;

    private readonly IHeroesRepository _heroes;
    private readonly IEntityFactory _factory;

    public TowerFloorScenario(IHeroesRepository heroes, IEntityFactory factory)
    {
        _heroes = heroes;
        _factory = factory;
    }

    public async Task PopulateEnemiesAsync(World world, int floor)
    {
        // Шаблоны врагов лежат в таблице Heroes у служебного игрока, статы масштабируются этажом
        var enemies = await _heroes.GetSquadAsync(HeroSeedData.EnemyPlayerId);
        _factory.CreateSquad(world, enemies, teamId: 2, level: floor);
    }

    public Task<RewardEntity> CalculateRewardsAsync(int floor, List<CombatLogEntry> logs)
    {
        // Специфичная логика наград для башни
        return Task.FromResult(new RewardEntity { Gold = floor * 100, Experience = floor * 25 });
    }
}
