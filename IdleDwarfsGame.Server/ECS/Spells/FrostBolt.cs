// Ледяная стрела: одна цель, морозит на 2 хода — замедляет накопление энергии на 20% / 25% / 30%
public class FrostBolt : BaseSpell
{
    private const int FrostTurns = 2;

    public FrostBolt(int level = 1) : base(level + 15, "Frost Bolt", level, 25) { }

    public override DamageType DamageType => DamageType.Frost;

    // 160% / 190% / 230% атаки
    private double DamageMultiplier => Level switch
    {
        1 => 1.6,
        2 => 1.9,
        _ => 2.3
    };
    private int FrostSlow => 15 + 5 * Level;

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), 1, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new FrostEffect(turns: FrostTurns, slowPercent: FrostSlow).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: (int)(casterAttack * DamageMultiplier));
}
