public class CampaignScenario : ICombatScenario
{
    public CombatMode Mode => CombatMode.Campaign;

    private readonly ICampaignRepository _campaign;
    private readonly IEntityFactory _factory;

    public CampaignScenario(ICampaignRepository campaign, IEntityFactory factory)
    {
        _campaign = campaign;
        _factory = factory;
    }

    public async Task PopulateEnemiesAsync(World world, int stageId)
    {
        var stage = await _campaign.GetStageAsync(stageId)
            ?? throw new InvalidOperationException($"Campaign stage {stageId} not found");

        SpawnEnemies(world, stage);
    }

    // Используется и в бою, и для превью уровня в списке кампании — враги в превью совпадают с боем
    public void SpawnEnemies(World world, CampaignStage stage)
    {
        foreach (var enemy in stage.Enemies.OrderBy(e => e.Position))
        {
            int id = _factory.CreateUnit(world, enemy.Hero, teamId: 2, position: enemy.Position, level: stage.EnemyLevel);
            world.Info[id].IsBoss = enemy.IsBoss;

            var health = world.Health[id];
            health.Max = Math.Max(1, (int)Math.Round(health.Max * enemy.HpMultiplier));
            health.Current = health.Max;
        }
    }

    public Task<RewardEntity?> CalculateRewardsAsync(int stageId, List<CombatLogEntry> logs)
    {
        // Награду за уровень выдаёт CampaignService (CampaignRewards), в ответе боя её нет — игрок забирает её в кампании
        return Task.FromResult<RewardEntity?>(null);
    }
}
