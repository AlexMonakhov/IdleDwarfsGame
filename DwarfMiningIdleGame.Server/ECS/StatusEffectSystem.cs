using System.Linq;

public class StatusEffectSystem : ICombatSystem
{
    private readonly EffectPhase Phase;

    public StatusEffectSystem(EffectPhase phase)
    {
        Phase = phase;
    }

    public void Execute(World world, CombatContext context)
    {
        int actorId = context.CurrentActor;
        if (actorId == -1 || !world.StatusEffects.ContainsKey(actorId)) return;

        var effects = world.StatusEffects[actorId].ActiveEffects;

        foreach (var effect in effects.ToList())
        {
            if (Phase == EffectPhase.Start)
            {
                // Вызываем только логику начала хода (Stun, Burn и т.д.)
                effect.OnTurnStart(world, actorId, context);
            }
            else if (Phase == EffectPhase.End)
            {
                // Вызываем только логику конца хода (Poison, регенерация и т.д.)
                effect.OnTurnEnd(world, actorId, context);
            }
        }

        effects.RemoveAll(e => e.RemainingTurns <= 0);
    }
}

public enum EffectPhase
{
    Start, // Начало хода
    End    // Конец хода
}
