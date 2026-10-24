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
                // Эффекты начала хода (Stun, Burn и т.д.)
                effect.OnTurnStart(world, actorId, context);
            }
            else if (Phase == EffectPhase.End)
            {
                // Эффекты конца хода (Poison, кровотечение и т.д.)
                effect.OnTurnEnd(world, actorId, context);
            }
        }

        var expired = effects.Where(e => e.RemainingTurns <= 0).ToList();
        if (expired.Count == 0) return;

        effects.RemoveAll(e => e.RemainingTurns <= 0);

        // Мёртвым не пишем — их статусы фронт и так снимет при смерти
        if (!world.IsAlive(actorId)) return;

        foreach (var effect in expired)
        {
            context.AddLog(new CombatLogEntry
            {
                EventType = CombatEventType.StatusExpired,
                Phase = Phase == EffectPhase.Start ? "TurnStart" : "TurnEnd",
                TargetId = actorId,
                TargetTeam = world.Team[actorId].TeamId,
                ActionType = effect.EffectName,
                TargetRemainingHp = world.Health[actorId].Current,
                ActiveEffects = { effect.ToLogData() }
            });
        }
    }
}

public enum EffectPhase
{
    Start, // Начало хода
    End    // Конец хода
}
