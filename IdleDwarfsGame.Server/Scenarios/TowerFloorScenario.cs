// Бой за этаж бесконечной башни. Создаётся на один бой: этаж (враги, стихия, арена) собирает InfinityTower
public class TowerFloorScenario(IEntityFactory factory, TowerFloor floor) : ICombatScenario
{
    public CombatMode Mode => CombatMode.Tower;

    public TowerFloor Floor => floor;

    public Task PopulateEnemiesAsync(World world, int level)
    {
        SpawnEnemies(world);
        return Task.CompletedTask;
    }

    // Используется и в бою, и для превью этажа в окне башни — враги в превью совпадают с боем.
    // HP и атаку растит уровень врагов этажа (как в кампании), аура — доля уже выросшей атаки (AuraBook)
    public void SpawnEnemies(World world)
    {
        foreach (var enemy in floor.Enemies)
        {
            int id = factory.CreateUnit(world, enemy.Hero, teamId: 2, position: enemy.Position, level: floor.EnemyLevel);
            world.Info[id].IsBoss = enemy.IsBoss;
            if (!enemy.IsBoss) continue;

            var health = world.Health[id];
            health.Max = Math.Max(1, (int)Math.Round(health.Max * InfinityTower.BossHpMultiplier));
            health.Current = health.Max;
        }
    }

    // Награду за этаж выдаёт TowerService (TowerRewards), игрок забирает её в окне башни
    public Task<RewardEntity?> CalculateRewardsAsync(int level, List<CombatLogEntry> logs) =>
        Task.FromResult<RewardEntity?>(null);
}
