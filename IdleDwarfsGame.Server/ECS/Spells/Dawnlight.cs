// Рассвет: лечит весь отряд и вешает на каждого короткую регенерацию
public class Dawnlight : BaseSpell
{
    private const int RegenTurns = 2;

    public Dawnlight(int level = 1) : base(level + 51, "Dawnlight", level, 45) { }

    public override DamageType DamageType => DamageType.Radiant;

    private double HealMultiplier => 0.4 + 0.2 * Level;
    private int RegenPerTurn => 3 * Level;

    public override void Execute(World world, int casterId, CombatContext context)
    {
        var allies = world.GetAliveAllies(casterId);
        LogCast(world, casterId, context, allies.Select(a => a.Id).ToList());

        int heal = ScaledDamage(world, casterId, HealMultiplier);
        foreach (var ally in allies)
        {
            HealTarget(world, casterId, ally.Id, heal, context);
            new RegenEffect(turns: RegenTurns, healPerTurn: RegenPerTurn).ApplyEffect(world, ally.Id, context, casterId);
        }
    }

    protected override List<int> GetTargets(World world, int casterId, Random random) => new();

    protected override int GetDamage(World world, int casterId, int targetId) => 0;

    // Союзников в среднем бою столько же, сколько врагов
    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Healing: enemyCount * ((int)(casterAttack * HealMultiplier) + RegenTurns * RegenPerTurn));
}
