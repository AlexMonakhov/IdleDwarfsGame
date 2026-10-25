public class Lightning : BaseSpell
{
    public Lightning(int level = 1) : base(level + 3, "Lightning", level, 40) { }

    public override DamageType DamageType => DamageType.Lightning;

    // 1 — одна цель, 2 и 3 — две
    private int TargetCount => Level == 1 ? 1 : 2;

    // На 3 уровне урон как на 2-м, но со станом
    private double DamageMultiplier => Level == 1 ? 1.3 : 0.9;

    private bool Stuns => Level == 3;

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), TargetCount, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        (int)(world.Attack[casterId].Damage * DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (Stuns)
        {
            new StunEffect(turns: 1).ApplyEffect(world, targetId, context, casterId);
        }
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(TargetCount, enemyCount);
        return new SpellEstimate(
            Damage: targets * (int)(casterAttack * DamageMultiplier),
            StunnedTurns: Stuns ? targets : 0);
    }
}
