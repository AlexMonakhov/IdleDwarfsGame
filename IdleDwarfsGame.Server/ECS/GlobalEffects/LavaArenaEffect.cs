public class LavaArenaEffect : IGlobalEffect
{
    public string Name => "Lava Arena";
    public TriggerPhase Trigger => TriggerPhase.OnRoundStart;
    private readonly int _burnDamage;

    public LavaArenaEffect(int burnDamage = 5)
    {
        _burnDamage = burnDamage;
    }

    public void Apply(World world, CombatContext context)
    {
        if (!context.IsNewRound) return;

        // Находим всех живых в самом начале раунда
        var aliveEntities = world.Entities
            .Where(e => world.IsAlive(e.Id))
            .ToList();

        foreach (var entity in aliveEntities)
        {
            new BurnEffect(turns: 1, damagePerTurn: _burnDamage).ApplyEffect(world, entity.Id, context);
        }
    }
}
