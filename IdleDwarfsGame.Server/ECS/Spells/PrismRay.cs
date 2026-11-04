// Призматический луч: точный удар светом, на 3 уровне луч раздваивается
public class PrismRay : BaseSpell
{
    public PrismRay(int level = 1) : base(level + 45, "Prism Ray", level, 30) { }

    public override DamageType DamageType => DamageType.Radiant;

    private int TargetCount => Level == 3 ? 2 : 1;
    // 190% / 235% / 195% атаки (на 3 уровне целей две)
    private double DamageMultiplier => Level switch
    {
        1 => 1.9,
        2 => 2.35,
        _ => 1.95
    };

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), TargetCount, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: Math.Min(TargetCount, enemyCount) * (int)(casterAttack * DamageMultiplier));
}
