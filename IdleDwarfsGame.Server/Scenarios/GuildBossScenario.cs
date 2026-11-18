// Бой с боссом гильдии. Создаётся на один бой: босс выходит с общим для гильдии остатком HP.
// Игроку засчитывается урон отряда из статистики боя (CombatResult.DamageDealtBy), а не потеря HP босса
public class GuildBossScenario(IEntityFactory factory, Hero template, GuildBossDefinition definition, int currentHp) : ICombatScenario
{
    public CombatMode Mode => CombatMode.GuildBoss;

    // Бой с боссом гильдии короче обычного: на этот лимит рассчитан его запас HP (GuildBosses)
    public int MaxRounds => GuildBosses.FightRounds;

    public GuildBossDefinition Definition => definition;

    public Task PopulateEnemiesAsync(World world, int level)
    {
        // Ячейка 1 — передняя линия: по ней бьёт весь отряд
        SpawnBoss(world);
        return Task.CompletedTask;
    }

    // То же без боя — для превью босса в окне гильдии
    public int SpawnBoss(World world)
    {
        int bossId = factory.CreateUnit(world, template, teamId: 2, position: 1, level: definition.Level);
        world.Info[bossId].IsBoss = true;
        world.Health[bossId].NominalMax = world.Health[bossId].Max;
        world.Health[bossId].Max = definition.MaxHp;
        world.Health[bossId].Current = Math.Clamp(currentHp, 0, definition.MaxHp);
        return bossId;
    }

    // Награда — не за бой, а за убийство босса гильдией (GuildService.ClaimBossRewardAsync)
    public Task<RewardEntity?> CalculateRewardsAsync(int level, List<CombatLogEntry> logs) =>
        Task.FromResult<RewardEntity?>(null);
}
