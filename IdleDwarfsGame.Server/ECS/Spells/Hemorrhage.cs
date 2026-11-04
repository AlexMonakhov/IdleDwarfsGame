// Кровопускание: режет случайных врагов и пускает им кровь (Bleed). Повторные наложения
// складываются по правилам кровотечения: стак и ход сверху, каждое следующее вдвое слабее
public class Hemorrhage : BaseSpell
{
    public Hemorrhage(int level = 1) : base(level + 57, "Hemorrhage", level, 30) { }

    public override DamageType DamageType => DamageType.Bleed;

    private double DamageMultiplier => 0.6 + 0.1 * Level;
    private int TargetCount => Level == 3 ? 3 : 2;
    private int BleedDamage(int attack) => (int)(attack * (0.15 + 0.05 * Level));

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), TargetCount, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new BleedEffect(damagePerTurn: BleedDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: Math.Min(TargetCount, enemyCount) * ((int)(casterAttack * DamageMultiplier) + BleedDamage(casterAttack)));
}
