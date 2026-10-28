// Обледенение: как оглушение (ход пропускается), но отдельный статус — иммунитет к стану от него не спасает,
// а фронт рисует ледяную глыбу вместо звёздочек
public class FreezeEffect : StunEffect
{
    public override string EffectName => "Freeze";

    public FreezeEffect(int turns = 1) : base(turns)
    {
    }
}
