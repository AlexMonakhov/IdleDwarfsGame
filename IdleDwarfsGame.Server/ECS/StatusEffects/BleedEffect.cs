// Кровотечение: у наложения нет длительности — каждое добавляет стак и один ход сверху.
// Первое бьёт в полную силу, каждое следующее, пока кровотечение висит, — вдвое слабее (с округлением вверх),
// урон стаков складывается. Пример: 15, 10, 5 → 15 + 5 + 3 = 23 за ход, три хода.
// Через два хода остаётся один стак; новое наложение на 15 даёт ещё стак и +8 урона
public class BleedEffect : StatusEffectBase
{
    public override string EffectName => "Bleed";
    protected override DamageType? EffectDamageType => DamageType.Bleed;

    public BleedEffect(int damagePerTurn = 5) : base(1, damagePerTurn)
    {
    }

    protected override void Absorb(StatusEffectBase added)
    {
        Power += (added.Power + 1) / 2;
        RemainingTurns++;
        Stacks = RemainingTurns;
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        int damage = Power;
        RemainingTurns--;
        Stacks = RemainingTurns;
        if (!world.IsAlive(targetId)) return;
        DealTickDamage(world, targetId, context, damage, DamageType.Bleed);
    }
}
