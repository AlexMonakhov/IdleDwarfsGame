// Бой с мировым боссом. Как у босса гильдии: босс выходит с общим для сервера остатком HP,
// засчитывается урон отряда из статистики боя (CombatResult.DamageDealtBy). На фронте это тот же режим, что и босс гильдии
public class WorldBossScenario(IEntityFactory factory, Hero template, int level, int maxHp, int currentHp) : ICombatScenario
{
    public CombatMode Mode => CombatMode.GuildBoss;

    public int MaxRounds => WorldBossRules.FightRounds;

    public int Level => level;

    public Task PopulateEnemiesAsync(World world, int difficultyOrFloor)
    {
        SpawnBoss(world);
        return Task.CompletedTask;
    }

    // То же без боя — для превью босса в окне ивента
    public int SpawnBoss(World world)
    {
        int bossId = factory.CreateUnit(world, template, teamId: 2, position: 1, level: level);
        world.Info[bossId].IsBoss = true;
        world.Health[bossId].NominalMax = world.Health[bossId].Max;
        world.Health[bossId].Max = maxHp;
        world.Health[bossId].Current = Math.Clamp(currentHp, 0, maxHp);
        return bossId;
    }

    // Награда — валюта ивента за удар и за убийство (WorldBossService), не за бой
    public Task<RewardEntity?> CalculateRewardsAsync(int level, List<CombatLogEntry> logs) =>
        Task.FromResult<RewardEntity?>(null);
}
