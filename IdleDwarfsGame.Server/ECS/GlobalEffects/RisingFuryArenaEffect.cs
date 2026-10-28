// Арена PvP «Нарастающая ярость»: после каждых IntervalRounds полных раундов весь урон в бою
// (удары, заклинания, тики статусов — у обеих команд) растёт ещё на BonusPercent. Прибавки складываются:
// с 6-го раунда +10%, с 11-го +20% и т.д. — затяжной бой рано или поздно кончается.
// Эффект без состояния: прибавка считается по номеру раунда, поэтому один экземпляр годится и для прогноза
public class RisingFuryArenaEffect(int intervalRounds = PvpArena.IntervalRounds, int bonusPercent = PvpArena.BonusPercent) : IGlobalEffect
{
    public string Name => "RisingFury";
    public TriggerPhase Trigger => TriggerPhase.OnRoundStart;

    public int IntervalRounds { get; } = Math.Max(1, intervalRounds);
    public int BonusPercent { get; } = bonusPercent;

    public static int BonusOnRound(int round, int intervalRounds, int bonusPercent) =>
        Math.Max(0, round - 1) / Math.Max(1, intervalRounds) * bonusPercent;

    public void Apply(World world, CombatContext context)
    {
        if (!context.IsNewRound) return;

        int bonus = BonusOnRound(context.CurrentRound, IntervalRounds, BonusPercent);
        if (bonus == world.DamageBonusPercent) return;

        world.DamageBonusPercent = bonus;
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.ArenaBuff,
            Phase = "RoundStart",
            ActionType = Name,
            DamageBonusPercent = bonus,
            Text = $"All damage +{bonus}%"
        });
    }
}
