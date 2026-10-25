// Руна лечения: лечит самого раненого союзника и вешает на него регенерацию
public class HealingRune : BaseSpell
{
    private const int RegenTurns = 2;

    public HealingRune(int level = 1) : base(level + 6, "Healing Rune", level, 40) { }

    public override DamageType DamageType => DamageType.Magical;

    private int HealAmount(int attack) => (int)(attack * (1.5 + 0.5 * Level));
    private int RegenPerTurn => 4 * Level;

    public override void Execute(World world, int casterId, CombatContext context)
    {
        var target = world.GetAliveAllies(casterId)
            .OrderBy(e => (double)world.Health[e.Id].Current / world.Health[e.Id].Max)
            .First();

        LogCast(world, casterId, context, new List<int> { target.Id });

        int healed = world.ApplyHeal(target.Id, HealAmount(world.Attack[casterId].Damage));

        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Heal,
            AttackerId = casterId,
            TargetId = target.Id,
            AttackerTeam = world.Team[casterId].TeamId,
            TargetTeam = world.Team[target.Id].TeamId,
            ActionType = $"{Name} (Lvl {Level})",
            SkillId = Id,
            SkillName = Name,
            Heal = healed,
            TargetRemainingHp = world.Health[target.Id].Current
        });

        new RegenEffect(turns: RegenTurns, healPerTurn: RegenPerTurn).ApplyEffect(world, target.Id, context, casterId);
    }

    protected override List<int> GetTargets(World world, int casterId, Random random) => new();

    protected override int GetDamage(World world, int casterId, int targetId) => 0;

    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Healing: HealAmount(casterAttack) + RegenTurns * RegenPerTurn);
}
