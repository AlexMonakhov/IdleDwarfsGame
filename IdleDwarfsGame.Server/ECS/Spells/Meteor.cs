// Метеор: дорогой удар по самому живучему врагу, со 2 уровня поджигает
public class Meteor : BaseSpell
{
    public Meteor(int level = 1) : base(level + 12, "Meteor", level, 60) { }

    public override DamageType DamageType => DamageType.Fire;

    private double DamageMultiplier => 1.6 + 0.4 * Level;

    private bool Burns => Level >= 2;
    private static int BurnDamage(int attack) => (int)(attack * 0.25);
    private const int BurnTurns = 2;

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetAliveEnemies(world, casterId)
            .OrderByDescending(e => world.Health[e.Id].Current)
            .Take(1)
            .Select(e => e.Id)
            .ToList();

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (Burns)
            new BurnEffect(turns: BurnTurns, damagePerTurn: BurnDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: (int)(casterAttack * DamageMultiplier) + (Burns ? BurnTurns * BurnDamage(casterAttack) : 0));
}
