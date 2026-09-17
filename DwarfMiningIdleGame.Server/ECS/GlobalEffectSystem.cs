public class GlobalEffectSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        if (world.GlobalEffects.Count == 0) return;

        foreach (var globalEffect in world.GlobalEffects)
        {
            // 1. Ёффекты, которые срабатывают ровно 1 раз в начале раунда
            if (globalEffect.Trigger == TriggerPhase.OnRoundStart && context.IsNewRound)
            {
                globalEffect.Apply(world, context);
            }

            // 2. Ёффекты, которые срабатывают перед ходом  ј∆ƒќ√ќ персонажа
            if (globalEffect.Trigger == TriggerPhase.OnTurnStart)
            {
                globalEffect.Apply(world, context);
            }
        }
    }
}