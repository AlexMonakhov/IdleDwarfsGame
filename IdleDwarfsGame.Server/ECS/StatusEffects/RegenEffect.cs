// Регенерация: лечит в начале хода носителя
public class RegenEffect : StatusEffectBase
{
    public override string EffectName => "Regen";

    public RegenEffect(int turns = 1, int healPerTurn = 5) : base(turns, healPerTurn)
    {
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        RemainingTurns--;
        if (!world.IsAlive(targetId)) return;

        int healed = world.ApplyHeal(targetId, Power);
        if (healed <= 0) return;

        var log = TickLog(world, targetId);
        log.EventType = CombatEventType.Heal;
        log.Heal = healed;
        log.TargetRemainingHp = world.Health[targetId].Current;
        context.AddLog(log);
    }
}
