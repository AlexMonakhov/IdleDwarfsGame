// Проклятие: тёмный урон в начале хода и потеря части энергии — проклятый реже кастует.
// Длительность задаёт первое наложение: следующие её не продлевают, но их урон прибавляется целиком
// и действует до конца уже висящего проклятия
public class CurseEffect : StatusEffectBase
{
    private const int EnergyDrain = 5;

    public override string EffectName => "Curse";
    protected override DamageType? EffectDamageType => DamageType.Dark;

    public CurseEffect(int turns = 2, int damagePerTurn = 5) : base(turns, damagePerTurn)
    {
    }

    protected override void Absorb(StatusEffectBase added)
    {
        Power += added.Power;
        Stacks++;
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        RemainingTurns--;
        if (!world.IsAlive(targetId)) return;

        if (world.Energy.TryGetValue(targetId, out var energy))
        {
            energy.Current = Math.Max(0, energy.Current - EnergyDrain);
        }
        DealTickDamage(world, targetId, context, Power, DamageType.Dark);
    }
}
