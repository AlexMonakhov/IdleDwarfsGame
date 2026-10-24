public class BossScenario : ICombatScenario
{
    public CombatMode Mode => CombatMode.Boss;

    private const int BossHpMultiplier = 4;

    private readonly IHeroesRepository _heroes;
    private readonly IEntityFactory _factory;

    public BossScenario(IHeroesRepository heroes, IEntityFactory factory)
    {
        _heroes = heroes;
        _factory = factory;
    }

    public async Task PopulateEnemiesAsync(World world, int difficulty)
    {
        // Босс — самый живучий из шаблонов врагов с умноженным HP
        var template = (await _heroes.GetByPlayerAsync(HeroSeedData.EnemyPlayerId))
            .OrderByDescending(h => h.MaxHp)
            .First();

        int bossId = _factory.CreateUnit(world, template, teamId: 2, position: 1, level: difficulty);
        world.Info[bossId].Name = $"{template.Name} (Boss)";
        world.Health[bossId].Max *= BossHpMultiplier;
        world.Health[bossId].Current = world.Health[bossId].Max;
    }

    public Task<RewardEntity> CalculateRewardsAsync(int difficulty, List<CombatLogEntry> logs)
    {
        return Task.FromResult(new RewardEntity { Gold = difficulty * 300, Experience = difficulty * 75 });
    }
}
