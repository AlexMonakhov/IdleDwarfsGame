// Бой с боссом гильдии. Создаётся на один бой: босс выходит с общим для гильдии остатком HP,
// а после боя по BossHpLeft считается, сколько урона нанёс игрок
public class GuildBossScenario(IEntityFactory factory, Hero template, GuildBossDefinition definition, int currentHp) : ICombatScenario
{
    public CombatMode Mode => CombatMode.GuildBoss;

    public GuildBossDefinition Definition => definition;

    private World? _world;
    private int _bossId;

    public int BossHpLeft => _world?.Health[_bossId].Current ?? currentHp;

    public Task PopulateEnemiesAsync(World world, int level)
    {
        // Ячейка 1 — передняя линия: по ней бьёт весь отряд
        _bossId = SpawnBoss(world);
        _world = world;
        return Task.CompletedTask;
    }

    // То же без боя — для превью босса в окне гильдии
    public int SpawnBoss(World world)
    {
        int bossId = factory.CreateUnit(world, template, teamId: 2, position: 1, level: definition.Level);
        world.Info[bossId].IsBoss = true;
        world.Health[bossId].Max = definition.MaxHp;
        world.Health[bossId].Current = Math.Clamp(currentHp, 0, definition.MaxHp);
        return bossId;
    }

    // Награда — не за бой, а за убийство босса гильдией (GuildService.ClaimBossRewardAsync)
    public Task<RewardEntity?> CalculateRewardsAsync(int level, List<CombatLogEntry> logs) =>
        Task.FromResult<RewardEntity?>(null);
}
