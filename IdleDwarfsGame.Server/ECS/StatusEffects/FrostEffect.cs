// Мороз: небольшой урон холодом в начале хода и сжигание части энергии
public class FrostEffect : StatusEffectBase
{
    public override string EffectName => "Frost";
    protected override DamageType? EffectDamageType => DamageType.Frost;

    public FrostEffect(int turns = 2, int damagePerTurn = 0) : base(turns, damagePerTurn)
    {
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        RemainingTurns--;
        if (!world.IsAlive(targetId)) return;

        int hpLeft = world.ApplyDamage(targetId, Power);
        if (world.Energy.TryGetValue(targetId, out var energy))
        {
            energy.Current = Math.Max(0, energy.Current - 10);
        }

        var log = TickLog(world, targetId);
        log.Damage = Power;
        log.DamageType = DamageType.Frost;
        log.TargetRemainingHp = hpLeft;
        context.AddLog(log);
    }
}
