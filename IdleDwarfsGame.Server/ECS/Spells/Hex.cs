// Порча: слабый удар по всем врагам и проклятие, которое тикает тьмой и вытягивает энергию
public class Hex : BaseSpell
{
    private const double DamageMultiplier = 0.3;

    public Hex(int level = 1) : base(level + 42, "Hex", level, 50) { }

    public override DamageType DamageType => DamageType.Dark;

    private int CurseTurns => Level == 3 ? 3 : 2;
    private int CurseDamage(int attack) => (int)(attack * (0.15 + 0.05 * Level));

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, null);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new CurseEffect(turns: CurseTurns, damagePerTurn: CurseDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: enemyCount * ((int)(casterAttack * DamageMultiplier) + CurseTurns * CurseDamage(casterAttack)));
}
