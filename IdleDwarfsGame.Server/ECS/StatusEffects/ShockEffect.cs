// Электрошок: урон молнией в начале хода носителя, наложения — независимые слои
public class ShockEffect : DamageOverTimeEffect
{
    public override string EffectName => "Shock";
    protected override DamageType TickDamageType => DamageType.Lightning;

    public ShockEffect(int turns = 1, int damagePerTurn = 5) : base(turns, damagePerTurn)
    {
    }
}
