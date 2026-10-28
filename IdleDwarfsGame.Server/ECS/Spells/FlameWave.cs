// Волна огня: катится по передней линии врагов
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

    private double DamageMultiplier => Level == 2 ? 0.75 : 0.7;

    // На 3 уровне поджигает на ход уроном 20% от атаки
    private bool Burns => Level == 3;
    private static int BurnDamage(int attack) => (int)(attack * 0.2);

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, MaxTargets);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (Burns)
            new BurnEffect(turns: 1, damagePerTurn: BurnDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(MaxTargets ?? enemyCount, enemyCount);
        int perTarget = (int)(casterAttack * DamageMultiplier) + (Burns ? BurnDamage(casterAttack) : 0);
        return new SpellEstimate(Damage: targets * perTarget);
    }
}
