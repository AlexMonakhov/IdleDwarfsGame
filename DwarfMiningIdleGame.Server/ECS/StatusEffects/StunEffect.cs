public class StunEffect : IStatusEffect
{
    public string EffectName => "Stun";
    public int RemainingTurns { get; set; }

    public StunEffect(int turns = 1)
    {
        RemainingTurns = turns;
    }

    public void ApplyEffect(World world, int targetId, CombatContext context)
    {
        if (!world.StatusEffects.ContainsKey(targetId))
        {
            world.StatusEffects[targetId] = new StatusEffectComponent();
        }
        world.StatusEffects[targetId].ActiveEffects.Add(this);
    }

    public void OnTurnStart(World world, int targetId, CombatContext context)
    {
        // 1. Запрещаем ходить
        if (world.TurnState.ContainsKey(targetId))
        {
            world.TurnState[targetId].CanAct = false;
        }

        // 2. Сразу пишем в лог, что ход пропущен из-за стана
        context.Logs.Add(new CombatLogEntry
        {
            AttackerId = targetId,
            TargetId = -1,
            ActionType = "Stunned",
            Damage = 0
        });

        RemainingTurns--;
    }

    public void OnTurnEnd(World world, int targetId, CombatContext context)
    {
    }
}
