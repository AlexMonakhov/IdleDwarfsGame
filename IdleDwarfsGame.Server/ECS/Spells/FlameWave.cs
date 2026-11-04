// Волна огня: катится по передней линии врагов и поджигает их
public class FlameWave : BaseSpell
{
    public FlameWave(int level = 1) : base(level + 9, "Flame Wave", level, 30) { }

    public override DamageType DamageType => DamageType.Fire;

    // 1 — двое впереди, 2 — трое, 3 — все враги (null)
    private int? MaxTargets => Level switch
    {
        1 => 2,
        2 => 3,
        _ => null
    };

    // 110% / 105% / 90% атаки по каждой цели
    private double DamageMultiplier => Level switch
    {
        1 => 1.1,
        2 => 1.05,
        _ => 0.9
    };

    // Поджигает на 2 хода уроном 10% от атаки
    private const int BurnTurns = 2;
    private static int BurnDamage(int attack) => (int)(attack * 0.1);

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, MaxTargets);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new BurnEffect(turns: BurnTurns, damagePerTurn: BurnDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(MaxTargets ?? enemyCount, enemyCount);
        int perTarget = (int)(casterAttack * DamageMultiplier) + BurnTurns * BurnDamage(casterAttack);
        return new SpellEstimate(Damage: targets * perTarget);
    }
}
