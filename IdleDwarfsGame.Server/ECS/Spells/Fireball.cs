public class Fireball : BaseSpell
{
    public Fireball(int level = 1) : base(level, "Fireball", level, 20) { }

    public override DamageType DamageType => DamageType.Fire;

    // 1 — одна цель, 2 — группа случайных врагов, 3 — по площади всех врагов (null)
    private int? MaxTargets => Level switch
    {
        1 => 1,
        2 => 3,
        _ => null
    };

    private double DamageMultiplier => Level switch
    {
        1 => 1.2,
        2 => 0.8,
        3 => 0.6,
        _ => 1
    };

    // 1 — поджигает на 1 ход фиксированным уроном, дальше на 2 хода уроном 30% от атаки кастера
    private int BurnTurns => Level == 1 ? 1 : 2;
    private int BurnDamage(int attack) => Level == 1 ? 10 : (int)(attack * 0.3);

    protected override List<int> GetTargets(World world, int casterId, Random random)
    {
        var enemies = GetAliveEnemies(world, casterId);
        return MaxTargets is { } count
            ? GetRandomTargets(enemies, count, random)
            : enemies.Select(e => e.Id).ToList();
    }

    protected override int GetDamage(World world, int casterId, int targetId) =>
        (int)(world.Attack[casterId].Damage * DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        int burnDamage = BurnDamage(world.Attack[casterId].Damage);
        new BurnEffect(turns: BurnTurns, damagePerTurn: burnDamage).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(MaxTargets ?? enemyCount, enemyCount);
        int perTarget = (int)(casterAttack * DamageMultiplier) + BurnTurns * BurnDamage(casterAttack);
        return new SpellEstimate(Damage: targets * perTarget);
    }
}
