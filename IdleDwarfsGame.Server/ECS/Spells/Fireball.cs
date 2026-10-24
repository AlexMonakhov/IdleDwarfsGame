public class Fireball : BaseSpell
{
    public Fireball(int level = 1) : base(level, "Fireball", level, 20) { }

    public override DamageType DamageType => DamageType.Fire;

    protected override List<int> GetTargets(World world, int casterId)
    {
        var enemies = GetAliveEnemies(world, casterId);
        return Level switch
        {
            1 => GetRandomTargets(enemies, 1),
            2 => GetRandomTargets(enemies, 3), // По группе случайных врагов
            3 => enemies.Select(e => e.Id).ToList(), // По площади всех врагов
            _ => new List<int>()
        };
    }

    protected override int GetDamage(World world, int casterId, int targetId)
    {
        int baseDmg = world.Attack[casterId].Damage;
        return Level switch
        {
            1 => (int)(baseDmg * 1.2),
            2 => (int)(baseDmg * 0.8),
            3 => (int)(baseDmg * 0.6),
            _ => baseDmg
        };
    }

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        int baseDmg = world.Attack[casterId].Damage;

        if (Level == 1)
        {
            // Поджигает на 1 ход, фиксированный урон (например 10)
            new BurnEffect(turns: 1, damagePerTurn: 10).ApplyEffect(world, targetId, context, casterId);
        }
        else if (Level >= 2)
        {
            // Поджигает на 2 хода, урон зависит от силы атаки кастера (например 30% от атаки)
            int burnTickDamage = (int)(baseDmg * 0.3);
            new BurnEffect(turns: 2, damagePerTurn: burnTickDamage).ApplyEffect(world, targetId, context, casterId);
        }
    }
}
