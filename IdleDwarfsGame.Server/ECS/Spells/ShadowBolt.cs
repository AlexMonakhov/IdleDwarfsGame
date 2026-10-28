// Стрела тьмы: сильный удар по одной цели, со 2 уровня проклинает её
public class ShadowBolt : BaseSpell
{
    private const int CurseTurns = 2;

    public ShadowBolt(int level = 1) : base(level + 36, "Shadow Bolt", level, 25) { }

    public override DamageType DamageType => DamageType.Dark;

    private double DamageMultiplier => 1.0 + 0.2 * Level;

    private bool Curses => Level >= 2;
    private int CurseDamage(int attack) => (int)(attack * (0.1 + 0.05 * Level));

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), 1, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (Curses)
            new CurseEffect(turns: CurseTurns, damagePerTurn: CurseDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: (int)(casterAttack * DamageMultiplier) + (Curses ? CurseTurns * CurseDamage(casterAttack) : 0));
}
