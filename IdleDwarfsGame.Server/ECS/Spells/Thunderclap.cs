// Раскат грома: бьёт всех врагов и с шансом оглушает каждого
public class Thunderclap : BaseSpell
{
    public Thunderclap(int level = 1) : base(level + 27, "Thunderclap", level, 55) { }

    public override DamageType DamageType => DamageType.Lightning;

    // 90% / 115% / 145% атаки по каждому врагу
    private double DamageMultiplier => Level switch
    {
        1 => 0.9,
        2 => 1.15,
        _ => 1.45
    };

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
