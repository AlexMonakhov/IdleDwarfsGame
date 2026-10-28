// Ледяная темница: вмораживает врага — он пропускает ход (Freeze). На 3 уровне — двух врагов
public class GlacialPrison : BaseSpell
{
    public GlacialPrison(int level = 1) : base(level + 21, "Glacial Prison", level, 60) { }

    public override DamageType DamageType => DamageType.Frost;

    private int TargetCount => Level == 3 ? 2 : 1;
    private double DamageMultiplier => Level == 1 ? 0.8 : 1.0;

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), TargetCount, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new FreezeEffect(turns: 1).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(TargetCount, enemyCount);
        return new SpellEstimate(Damage: targets * (int)(casterAttack * DamageMultiplier), StunnedTurns: targets);
    }
}
