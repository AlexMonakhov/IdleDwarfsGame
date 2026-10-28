// Горение: урон огнём в начале хода носителя. Наложения — независимые слои (LayeredStatusEffect)
public class BurnEffect : DamageOverTimeEffect
{
    public override string EffectName => "Burn";
    protected override DamageType TickDamageType => DamageType.Fire;

    public BurnEffect(int turns = 3, int damagePerTurn = 5) : base(turns, damagePerTurn)
    {
    }
}
