// Раскат грома: бьёт всех врагов и с шансом оглушает каждого
public class Thunderclap : BaseSpell
{
    public Thunderclap(int level = 1) : base(level + 27, "Thunderclap", level, 55) { }

    public override DamageType DamageType => DamageType.Lightning;

    private double DamageMultiplier => 0.3 + 0.1 * Level;

    // 15% / 25% / 35%
    private double StunChance => 0.05 + 0.1 * Level;

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, null);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (context.Random.NextDouble() < StunChance)
            new StunEffect(turns: 1).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: enemyCount * (int)(casterAttack * DamageMultiplier), StunnedTurns: enemyCount * StunChance);
}
