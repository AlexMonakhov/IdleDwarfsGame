public class GlobalEffectSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        if (world.GlobalEffects.Count == 0 || context.CurrentActor == -1) return;

        foreach (var globalEffect in world.GlobalEffects)
        {
            // 1. Эффекты, которые срабатывают ровно 1 раз в начале раунда
            if (globalEffect.Trigger == TriggerPhase.OnRoundStart && context.IsNewRound)
            {
                globalEffect.Apply(world, context);
            }

            // 2. Эффекты, которые срабатывают перед ходом каждого персонажа
            if (globalEffect.Trigger == TriggerPhase.OnTurnStart)
            {
                globalEffect.Apply(world, context);
            }
        }
    }
}
