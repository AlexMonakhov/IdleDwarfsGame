public class LavaArenaEffect : IGlobalEffect
{
    public string Name => "Lava Arena";
    public TriggerPhase Trigger => TriggerPhase.OnRoundStart;
    private readonly int _burnDamage = 10;

    public void Apply(World world, CombatContext context)
    {
        if (!context.IsNewRound) return;

        // Находим всех живых юнитов в обоих командах
        var aliveEntities = world.Entities
            .Where(e => world.Health[e.Id].Current > 0)
            .ToList();

        foreach (var entity in aliveEntities)
        {
            var lavaBurn = new BurnEffect(turns: 1, damagePerTurn: 10);
            lavaBurn.ApplyEffect(world, entity.Id, context);
        }
    }
}