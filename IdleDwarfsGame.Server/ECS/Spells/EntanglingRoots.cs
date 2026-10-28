// Цепкие корни: опутывают переднюю линию — враги пропускают ход. На 3 уровне корни ещё и ядовитые
public class EntanglingRoots : BaseSpell
{
    private const int PoisonTurns = 2;

    public EntanglingRoots(int level = 1) : base(level + 33, "Entangling Roots", level, 50) { }

    public override DamageType DamageType => DamageType.Poison;

    private int TargetCount => Level == 1 ? 1 : 2;
    private double DamageMultiplier => 0.5 + 0.1 * Level;

    private bool Poisons => Level == 3;
    private static int PoisonDamage(int attack) => (int)(attack * 0.2);

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, TargetCount);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        new StunEffect(turns: 1).ApplyEffect(world, targetId, context, casterId);
        if (Poisons)
            new PoisonEffect(turns: PoisonTurns, damagePerTurn: PoisonDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(TargetCount, enemyCount);
        int perTarget = (int)(casterAttack * DamageMultiplier) + (Poisons ? PoisonTurns * PoisonDamage(casterAttack) : 0);
        return new SpellEstimate(Damage: targets * perTarget, StunnedTurns: targets);
    }
}
