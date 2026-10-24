public class StunEffect : StatusEffectBase
{
    public override string EffectName => "Stun";

    public StunEffect(int turns = 1) : base(turns, 0)
    {
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        RemainingTurns--;
        if (!world.IsAlive(targetId)) return;

        // 1. Блокируем действие
        if (world.TurnState.ContainsKey(targetId))
        {
            world.TurnState[targetId].CanAct = false;
        }

        // 2. Пишем в лог, что ход пропущен из-за стана
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Stunned,
            Phase = "TurnStart",
            AttackerId = targetId,
            AttackerTeam = world.Team[targetId].TeamId,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = "Stunned",
            TargetRemainingHp = world.Health[targetId].Current,
            ActiveEffects = { ToLogData() }
        });
    }
}
