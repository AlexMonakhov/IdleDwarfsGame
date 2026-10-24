public class BurnEffect : StatusEffectBase
{
    public override string EffectName => "Burn";
    protected override DamageType? EffectDamageType => DamageType.Fire;

    public BurnEffect(int turns = 3, int damagePerTurn = 5) : base(turns, damagePerTurn)
    {
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        RemainingTurns--;
        if (!world.IsAlive(targetId)) return;

        int hpLeft = world.ApplyDamage(targetId, Power);

        var log = TickLog(world, targetId);
        log.Damage = Power;
        log.DamageType = DamageType.Fire;
        log.TargetRemainingHp = hpLeft;
        context.AddLog(log);
    }
}
