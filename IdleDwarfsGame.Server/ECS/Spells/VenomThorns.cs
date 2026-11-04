// Ядовитые шипы: отравляют случайных врагов, яд тикает несколько ходов
public class VenomThorns : BaseSpell
{
    public VenomThorns(int level = 1) : base(level + 30, "Venom Thorns", level, 30) { }

    public override DamageType DamageType => DamageType.Poison;

    // 1 — одна цель, 2 — две, 3 — три
    private int TargetCount => Level;
    // 110% / 100% / 95% атаки по каждой цели
    private double DamageMultiplier => Level switch
    {
        1 => 1.1,
        2 => 1.0,
        _ => 0.95
    };

    // Яд 25% атаки на 2 хода, со 2 уровня — 20% на 3 хода
    private int PoisonTurns => Level == 1 ? 2 : 3;
    private int PoisonDamage(int attack) => (int)(attack * (Level == 1 ? 0.25 : 0.2));

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetRandomTargets(GetAliveEnemies(world, casterId), TargetCount, random);

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context) =>
        new PoisonEffect(turns: PoisonTurns, damagePerTurn: PoisonDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);

    public override SpellEstimate Estimate(int casterAttack, int enemyCount)
    {
        int targets = Math.Min(TargetCount, enemyCount);
        int perTarget = (int)(casterAttack * DamageMultiplier) + PoisonTurns * PoisonDamage(casterAttack);
        return new SpellEstimate(Damage: targets * perTarget);
    }
}
