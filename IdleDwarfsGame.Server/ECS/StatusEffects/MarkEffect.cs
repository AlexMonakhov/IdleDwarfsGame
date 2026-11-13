// Метка: урон любых заклинаний по цели больше на Power %. Урона сама не наносит.
// Повторное наложение не копится: берётся большая сила и большая длительность.
// Ход отсчитывается в конце хода носителя, как у мороза: метка на 2 хода переживёт два его хода
public class MarkEffect : StatusEffectBase
{
    public const string Name = "Mark";

    public override string EffectName => Name;

    public MarkEffect(int turns = 2, int percent = 30) : base(turns, percent)
    {
    }

    protected override void Absorb(StatusEffectBase added)
    {
        Power = Math.Max(Power, added.Power);
        RemainingTurns = Math.Max(RemainingTurns, added.RemainingTurns);
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
    }

    public override void OnTurnEnd(World world, int targetId, CombatContext context)
    {
        RemainingTurns--;
        if (!world.IsAlive(targetId)) return;

        var log = TickLog(world, targetId);
        log.Phase = "TurnEnd";
        context.AddLog(log);
    }

    public static MarkEffect? Find(World world, int targetId) =>
        world.StatusEffects.TryGetValue(targetId, out var component)
            ? component.ActiveEffects.OfType<MarkEffect>().FirstOrDefault(e => e.RemainingTurns > 0)
            : null;
}
