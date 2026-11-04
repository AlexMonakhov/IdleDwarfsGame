// Похищение жизни: урон кровью по одной цели, часть урона лечит кастера
public class LifeDrain : BaseSpell
{
    public LifeDrain(int level = 1) : base(level + 39, "Life Drain", level, 40) { }

    public override DamageType DamageType => DamageType.Bleed;

    // 200% / 250% / 310% атаки
    private double DamageMultiplier => Level switch
    {
        1 => 2.0,
        2 => 2.5,
        _ => 3.1
    };

    // Половина нанесённого урона
    private const double DrainShare = 0.5;

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), 1, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void AfterHit(World world, int casterId, int targetId, int damage, CombatContext context) =>
        HealTarget(world, casterId, casterId, (int)(damage * DrainShare), context);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int damage = (int)(casterAttack * DamageMultiplier);
        return new SpellEstimate(Damage: damage, Healing: damage * DrainShare);
    }
}
