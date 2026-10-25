// Эффект арены: в начале каждого раунда вешает статус на всех живых участников боя.
// Статус создаётся по имени (StatusEffectFactory), поэтому арены можно описывать данными в БД.
public class ArenaStatusEffect : IGlobalEffect
{
    public string Name { get; }
    public TriggerPhase Trigger => TriggerPhase.OnRoundStart;

    // Открыты для прогноза боя (CombatPower.Forecast)
    public string EffectName { get; }
    public int Power { get; }
    public int Duration { get; }

    public ArenaStatusEffect(string name, string effectName, int power, int duration = 1)
    {
        Name = name;
        EffectName = effectName;
        Power = power;
        Duration = Math.Max(1, duration);

        // Кривое имя эффекта в БД должно падать сразу, а не посреди боя
        StatusEffectFactory.Create(EffectName, Power, Duration);
    }

    public static ArenaStatusEffect? FromArena(Arena arena) =>
        string.IsNullOrWhiteSpace(arena.EffectName)
            ? null
            : new ArenaStatusEffect(arena.Name, arena.EffectName, arena.EffectPower, arena.EffectDuration);

    public void Apply(World world, CombatContext context)
    {
        if (!context.IsNewRound) return;

        // Находим всех живых в самом начале раунда
        var aliveEntities = world.Entities
            .Where(e => world.IsAlive(e.Id))
            .ToList();

        // Новый экземпляр на каждую цель, иначе все делили бы один RemainingTurns
        foreach (var entity in aliveEntities)
        {
            StatusEffectFactory.Create(EffectName, Power, Duration).ApplyEffect(world, entity.Id, context);
        }
    }
}
