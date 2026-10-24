public interface IStatusEffect
{
    string EffectName { get; }
    int RemainingTurns { get; set; }
    void ApplyEffect(World world, int targetId, CombatContext context, int sourceId = -1);
    void OnTurnStart(World world, int targetId, CombatContext context);
    void OnTurnEnd(World world, int targetId, CombatContext context);
    StatusEffectLogData ToLogData();
}

public abstract class StatusEffectBase : IStatusEffect
{
    public abstract string EffectName { get; }
    public int RemainingTurns { get; set; }
    protected int Power { get; }
    protected virtual DamageType? EffectDamageType => null;

    protected StatusEffectBase(int turns, int power)
    {
        RemainingTurns = turns;
        Power = power;
    }

    public void ApplyEffect(World world, int targetId, CombatContext context, int sourceId = -1)
    {
        if (!world.IsAlive(targetId)) return;

        // 1. Проверяем иммунитет
        if (world.Immunities.TryGetValue(targetId, out var immunity) && immunity.ImmuneTo.Contains(EffectName))
        {
            context.AddLog(new CombatLogEntry
            {
                EventType = CombatEventType.Immune,
                Phase = "Status",
                AttackerId = sourceId,
                AttackerTeam = sourceId > 0 ? world.Team[sourceId].TeamId : 0,
                TargetId = targetId,
                TargetTeam = world.Team[targetId].TeamId,
                ActionType = $"Immune ({EffectName})",
                TargetRemainingHp = world.Health[targetId].Current,
                AppliedEffects = { ToLogData() }
            });
            return;
        }

        // 2. Вешаем эффект (компонент создаётся, если его ещё нет)
        if (!world.StatusEffects.ContainsKey(targetId))
        {
            world.StatusEffects[targetId] = new StatusEffectComponent();
        }
        world.StatusEffects[targetId].ActiveEffects.Add(this);

        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.StatusApplied,
            Phase = "Status",
            AttackerId = sourceId,
            AttackerTeam = sourceId > 0 ? world.Team[sourceId].TeamId : 0,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = EffectName,
            TargetRemainingHp = world.Health[targetId].Current,
            AppliedEffects = { ToLogData() }
        });
    }

    public abstract void OnTurnStart(World world, int targetId, CombatContext context);

    public virtual void OnTurnEnd(World world, int targetId, CombatContext context)
    {
    }

    public StatusEffectLogData ToLogData() => new()
    {
        EffectId = EffectName.ToLowerInvariant(),
        Name = EffectName,
        Duration = RemainingTurns,
        Stacks = 1,
        TickDamage = EffectDamageType != null ? Power : null,
        DamageType = EffectDamageType
    };

    protected CombatLogEntry TickLog(World world, int targetId) => new()
    {
        EventType = CombatEventType.StatusTick,
        Phase = "TurnStart",
        TargetId = targetId,
        TargetTeam = world.Team[targetId].TeamId,
        ActionType = EffectName,
        SkillName = EffectName,
        ActiveEffects = { ToLogData() }
    };
}

public static class StatusEffectFactory
{
    public static IStatusEffect Create(string name, int power, int duration) => name switch
    {
        "Burn" => new BurnEffect(duration, power),
        "Frost" => new FrostEffect(duration, power),
        "Stun" => new StunEffect(duration),
        "Regen" => new RegenEffect(duration, power),
        _ => throw new ArgumentException($"Unknown status effect '{name}'")
    };
}
