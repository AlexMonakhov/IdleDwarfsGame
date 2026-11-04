// Метеор: дорогой удар по самому живучему врагу, со 2 уровня поджигает
public class Meteor : BaseSpell
{
    public Meteor(int level = 1) : base(level + 12, "Meteor", level, 60) { }

    public override DamageType DamageType => DamageType.Fire;

    // 260% / 320% / 400% атаки: каст копит три удара, поэтому бьёт как четыре с лишним
    private double DamageMultiplier => Level switch
    {
        1 => 2.6,
        2 => 3.2,
        _ => 4.0
    };

    // Горение 15% атаки: 2 хода на 2 уровне, 3 — на 3-м
    private bool Burns => Level >= 2;
    private static int BurnDamage(int attack) => (int)(attack * 0.15);
    private int BurnTurns => Level >= 3 ? 3 : 2;

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
