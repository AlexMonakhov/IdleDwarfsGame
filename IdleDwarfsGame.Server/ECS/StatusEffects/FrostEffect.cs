// Мороз: урона не наносит — на Power % замедляет накопление энергии за удар, пока висит.
// Проценты складываются слоями, как урон горения (не больше 100%).
// Ход отсчитывается в конце хода носителя, иначе мороз на 1 ход снимался бы до его удара
public class FrostEffect : LayeredStatusEffect
{
    public override string EffectName => "Frost";

    public FrostEffect(int turns = 2, int slowPercent = 20) : base(turns, slowPercent)
    {
    }

    public int EnergySlowPercent => Math.Clamp(Power, 0, 100);

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
    }

    public override void OnTurnEnd(World world, int targetId, CombatContext context)
    {
        PassTurn();
        if (!world.IsAlive(targetId)) return;

        var log = TickLog(world, targetId);
        log.Phase = "TurnEnd";
        context.AddLog(log);
    }
}
