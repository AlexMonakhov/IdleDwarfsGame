// Яд: урон природой в начале хода носителя. Складывается как горение — независимыми слоями
public class PoisonEffect : DamageOverTimeEffect
{
    public override string EffectName => "Poison";
    protected override DamageType TickDamageType => DamageType.Poison;

    public PoisonEffect(int turns = 2, int damagePerTurn = 5) : base(turns, damagePerTurn)
    {
    }
}
