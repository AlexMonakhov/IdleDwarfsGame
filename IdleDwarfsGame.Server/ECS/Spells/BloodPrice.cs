// Кровавая цена: кастер платит частью своего здоровья (от каста не умирает — остаётся хотя бы 1 HP)
// и обрушивает кровь на всех врагов. На 3 уровне враги ещё и кровоточат
public class BloodPrice : BaseSpell
{
    // Доля максимального HP кастера, которой он платит за каст
    private const double PriceShare = 0.1;

    public BloodPrice(int level = 1) : base(level + 60, "Blood Price", level, 50) { }

    public override DamageType DamageType => DamageType.Bleed;

    // 95% / 120% / 150% атаки по каждому врагу
    private double DamageMultiplier => Level switch
    {
        1 => 0.95,
        2 => 1.2,
        _ => 1.5
    };
    private bool Bleeds => Level >= 3;
    private static int BleedDamage(int attack) => (int)(attack * 0.25);

    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetFrontTargets(world, casterId, null);

    // Плата кровью — сразу после лога каста: фронт показывает её на кастере до попаданий
    protected override void OnCast(World world, int casterId, CombatContext context)
    {
        var hp = world.Health[casterId];
        int price = Math.Min((int)(hp.Max * PriceShare), hp.Current - 1);
        if (price <= 0) return;

        int hpLeft = world.ApplyDamage(casterId, price);
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Damage,
            AttackerId = casterId,
            TargetId = casterId,
            AttackerTeam = world.Team[casterId].TeamId,
            TargetTeam = world.Team[casterId].TeamId,
            ActionType = $"{Name} (Lvl {Level})",
            SkillId = Id,
            SkillName = Name,
            DamageType = DamageType,
            Damage = price,
            TargetRemainingHp = hpLeft
        });
    }

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (Bleeds)
            new BleedEffect(damagePerTurn: BleedDamage(world.Attack[casterId].Damage)).ApplyEffect(world, targetId, context, casterId);
    }

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: enemyCount * ((int)(casterAttack * DamageMultiplier) + (Bleeds ? BleedDamage(casterAttack) : 0)));
}
