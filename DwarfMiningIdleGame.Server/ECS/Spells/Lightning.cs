public class Lightning : BaseSpell
{
    public Lightning(int level = 1) : base(level + 3, "Lightning", level, 40) { }

    protected override List<int> GetTargets(World world, int casterId)
    {
        var enemies = GetAliveEnemies(world, casterId);

        return Level switch
        {
            1 => GetRandomTargets(enemies, 1), // 1 цель
            2 => GetRandomTargets(enemies, 2), // 2 цели
            3 => GetRandomTargets(enemies, 2), // 2 цели
            _ => new List<int>()
        };
    }

    protected override int GetDamage(World world, int casterId, int targetId)
    {
        int baseDamage = world.Attack[casterId].Damage;
        return Level switch
        {
            1 => (int)(baseDamage * 1.3),
            2 => (int)(baseDamage * 0.9),
            3 => (int)(baseDamage * 0.9), // ”рон как на 2 уровне, но будет стан
            _ => baseDamage
        };
    }

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        // —тан накладываетс€ только на 3-м уровне
        if (Level == 3)
        {
            var stun = new StunEffect(turns: 1);
            stun.ApplyEffect(world, targetId, context);
        }
    }
}
