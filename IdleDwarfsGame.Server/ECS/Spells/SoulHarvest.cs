// Жатва душ: бьёт тьмой в первую очередь проклятых врагов. Проклятие на цели срабатывает сразу —
// весь его оставшийся урон добавляется к удару, и проклятие снимается. На 3 уровне каждое снятое
// проклятие возвращает кастеру энергию
public class SoulHarvest : BaseSpell
{
    private const string CurseName = "Curse";
    private const int EnergyPerCurse = 10;

    public SoulHarvest(int level = 1) : base(level + 54, "Soul Harvest", level, 45) { }

    public override DamageType DamageType => DamageType.Dark;

    // 150% / 190% / 190% атаки (на 3 уровне целей три) плюс весь оставшийся урон проклятия
    private double DamageMultiplier => Level == 1 ? 1.5 : 1.9;
    private int TargetCount => Level == 3 ? 3 : 2;
    private bool RefundsEnergy => Level >= 3;

    // Сначала проклятые (в случайном порядке), остальные места — случайные враги
    protected override List<int> GetTargets(World world, int casterId, Random random) =>
        GetAliveEnemies(world, casterId)
            .OrderBy(e => FindCurse(world, e.Id) == null)
            .ThenBy(_ => random.Next())
            .Take(TargetCount)
            .Select(e => e.Id)
            .ToList();

    protected override int GetDamage(World world, int casterId, int targetId) =>
        ScaledDamage(world, casterId, DamageMultiplier) + (FindCurse(world, targetId) is { } curse ? curse.Power * curse.RemainingTurns : 0);

    protected override void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
        if (FindCurse(world, targetId) is not { } curse) return;

        RecordInCast(context, cast => cast.CurseConsumed += curse.Power * curse.RemainingTurns);
        world.StatusEffects[targetId].ActiveEffects.Remove(curse);
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.StatusExpired,
            AttackerId = casterId,
            AttackerTeam = world.Team[casterId].TeamId,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = curse.EffectName,
            SkillId = Id,
            SkillName = Name,
            TargetRemainingHp = world.Health[targetId].Current,
            ActiveEffects = { curse.ToLogData() }
        });

        if (RefundsEnergy && world.Energy.TryGetValue(casterId, out var energy))
            energy.Current = Math.Min(energy.Max, energy.Current + EnergyPerCurse);
    }

    // Добитая цель: проклятие уже ушло в удар целиком, хотя снять его не успели — оно снимется вместе со смертью
    protected override void AfterHit(World world, int casterId, int targetId, int damage, CombatContext context)
    {
        if (!world.IsAlive(targetId) && FindCurse(world, targetId) is { } curse)
            RecordInCast(context, cast => cast.CurseConsumed += curse.Power * curse.RemainingTurns);
    }

    private static CurseEffect? FindCurse(World world, int targetId) =>
        world.StatusEffects.TryGetValue(targetId, out var component)
            ? component.ActiveEffects.OfType<CurseEffect>().FirstOrDefault(e => e.EffectName == CurseName && e.RemainingTurns > 0)
            : null;

    // Проклятие на цели не угадать заранее — оцениваем только сам удар
    public override SpellEstimate Estimate(int casterAttack, int enemyCount) =>
        new(Damage: Math.Min(TargetCount, enemyCount) * (int)(casterAttack * DamageMultiplier));
}
