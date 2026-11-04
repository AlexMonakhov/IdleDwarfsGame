// Сияющая вспышка: бьёт светом всех врагов, со 2 уровня тот же свет подлечивает союзников
public class RadiantNova : BaseSpell
{
    public RadiantNova(int level = 1) : base(level + 48, "Radiant Nova", level, 55) { }

    public override DamageType DamageType => DamageType.Radiant;

    // 80% / 100% / 120% атаки по каждому врагу
    private double DamageMultiplier => 0.6 + 0.2 * Level;

    // 0 / 30% / 50% атаки каждому союзнику
    private double HealMultiplier => Level switch
    {
        1 => 0,
        2 => 0.3,
        _ => 0.5
    };

    public override void Execute(World world, int casterId, CombatContext context)
    {
        base.Execute(world, casterId, context);
        if (HealMultiplier <= 0) return;

        int heal = ScaledDamage(world, casterId, HealMultiplier);
        foreach (var ally in world.GetAliveAllies(casterId))
            HealTarget(world, casterId, ally.Id, heal, context);
    }

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, null);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    // Союзников в среднем бою столько же, сколько врагов
    public override SpellEstimate Estimate(int casterAttack, int enemyCount) => new(
        Damage: enemyCount * (int)(casterAttack * DamageMultiplier),
        Healing: enemyCount * (int)(casterAttack * HealMultiplier));
}
