// Цепная молния: перескакивает между случайными врагами, на 3 уровне оставляет электрошок
public class ChainLightning : BaseSpell
{
    public ChainLightning(int level = 1) : base(level + 24, "Chain Lightning", level, 35) { }

    public override DamageType DamageType => DamageType.Lightning;

    // 1 — две цели, 2 — три, 3 — четыре
    private int TargetCount => Level + 1;
    private const double DamageMultiplier = 0.7;

    private bool Shocks => Level == 3;
    private static int ShockDamage(int attack) => (int)(attack * 0.2);

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), TargetCount, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (Shocks)
            new ShockEffect(turns: 1, damagePerTurn: ShockDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(TargetCount, enemyCount);
        int perTarget = (int)(casterAttack * DamageMultiplier) + (Shocks ? ShockDamage(casterAttack) : 0);
        return new SpellEstimate(Damage: targets * perTarget);
    }
}
