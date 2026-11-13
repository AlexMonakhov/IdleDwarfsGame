// Кровопускание: режет случайных врагов и пускает им кровь (Bleed). Повторные наложения
// складываются по правилам кровотечения (BleedEffect, docs/effect-rules.md): один стак за наложение, прибавка тем меньше, чем больше стаков
public class Hemorrhage : BaseSpell
{
    public Hemorrhage(int level = 1) : base(level + 57, "Hemorrhage", level, 30) { }

    public override DamageType DamageType => DamageType.Bleed;

    // 100% / 120% / 110% атаки (на 3 уровне целей три) и кровотечение 20% / 30% / 35% атаки
    private double DamageMultiplier => Level switch
    {
        1 => 1.0,
        2 => 1.2,
        _ => 1.1
    };
    private int TargetCount => Level == 3 ? 3 : 2;
    private int BleedDamage(int attack) => (int)(attack * Level switch { 1 => 0.2, 2 => 0.3, _ => 0.35 });

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), TargetCount, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new BleedEffect(damagePerTurn: BleedDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: Math.Min(TargetCount, enemyCount) * ((int)(casterAttack * DamageMultiplier) + BleedDamage(casterAttack)));
}
