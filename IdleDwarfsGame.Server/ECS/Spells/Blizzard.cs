// Буран: слабый урон холодом по всем врагам и мороз на ход — накопление энергии медленнее на 10% / 15% / 20%
public class Blizzard : BaseSpell
{
    public Blizzard(int level = 1) : base(level + 18, "Blizzard", level, 50) { }

    public override DamageType DamageType => DamageType.Frost;

    private double DamageMultiplier => 0.35 + 0.1 * Level;
    private int FrostSlow => 5 + 5 * Level;

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, null);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new FrostEffect(turns: 1, slowPercent: FrostSlow).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: enemyCount * (int)(casterAttack * DamageMultiplier));
}
