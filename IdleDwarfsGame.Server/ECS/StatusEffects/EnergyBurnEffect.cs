// Выжигание энергии (аура тьмы): сразу отнимает у цели Amount энергии — проклятый реже кастует.
// Статусом на цели не висит: копить и отсчитывать нечего, каждое срабатывание ауры жжёт заново.
// В логе — StatusApplied с длительностью 0: Power — сколько энергии сгорело на деле, TargetEnergy — сколько осталось
public class EnergyBurnEffect(int amount) : IStatusEffect
{
    public const string Name = "EnergyBurn";

    public string EffectName => Name;
    public int RemainingTurns { get; set; }
    public int Amount { get; } = Math.Max(0, amount);

    private int _burned;

    public void ApplyEffect(World world, int targetId, CombatContext context, int sourceId = -1, int? stackLimit = null)
    {
        if (!world.IsAlive(targetId)) return;

        var log = new CombatLogEntry
        {
            EventType = CombatEventType.StatusApplied,
            Phase = "Status",
            AttackerId = sourceId,
            AttackerTeam = sourceId > 0 ? world.Team[sourceId].TeamId : 0,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = Name,
            TargetRemainingHp = world.Health[targetId].Current
        };

        if (world.Immunities.TryGetValue(targetId, out var immunity) && immunity.ImmuneTo.Contains(Name))
        {
            log.EventType = CombatEventType.Immune;
            log.ActionType = $"Immune ({Name})";
            log.AppliedEffects.Add(ToLogData());
            context.AddLog(log);
            return;
        }

        if (world.Energy.TryGetValue(targetId, out var energy))
        {
            _burned = Math.Min(Amount, energy.Current);
            energy.Current -= _burned;
            log.TargetEnergy = energy.Current;
        }
        log.AppliedEffects.Add(ToLogData());
        context.AddLog(log);
    }

    public void OnTurnStart(World world, int targetId, CombatContext context)
    {
    }

    public void OnTurnEnd(World world, int targetId, CombatContext context)
    {
    }

    public StatusEffectLogData ToLogData() => new()
    {
        EffectId = "energyburn",
        Name = Name,
        Duration = 0,
        Stacks = 1,
        Power = _burned
    };
}
