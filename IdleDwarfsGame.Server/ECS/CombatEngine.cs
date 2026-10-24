public class CombatEngine : ICombatEngine
{
    // Страховка от зацикливания: ходов не может быть больше, чем раундов * участников (+ запас)
    private const int MaxTurnsPerUnit = 100;

    private readonly List<ICombatSystem> _systems;

    public CombatEngine() : this(new Random())
    {
    }

    public CombatEngine(Random random)
    {
        _systems = new List<ICombatSystem>
        {
            new TurnOrderSystem(),
            new RoundStartSystem(),
            new GlobalEffectSystem(),
            new AuraSystem(),
            new StatusEffectSystem(EffectPhase.Start),
            new SpellSystem(SpellRegistry.All, random),
            new DamageSystem(),
            new StatusEffectSystem(EffectPhase.End),
            new HealthSystem()
        };
    }

    public CombatContext Run(World world)
    {
        var context = new CombatContext();
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.CombatStart,
            Phase = "Start",
            ActionType = "CombatStart"
        });

        int turnsLeft = Math.Max(1, world.Entities.Count) * MaxTurnsPerUnit;

        while (!context.CombatEnded && turnsLeft-- > 0)
        {
            foreach (var system in _systems)
            {
                system.Execute(world, context);

                if (context.CombatEnded)
                    break;
            }
        }

        return context;
    }
}

public static class CombatSnapshot
{
    // Снимок участников на старте боя: фронт рисует сцену по нему и дальше живёт только логами
    public static List<CombatUnitInfo> Units(World world) => world.Entities
        .Select(e =>
        {
            var info = world.Info.GetValueOrDefault(e.Id) ?? new UnitInfoComponent();
            var energy = world.Energy.GetValueOrDefault(e.Id);
            var aura = world.Auras.GetValueOrDefault(e.Id);

            return new CombatUnitInfo
            {
                EntityId = e.Id,
                HeroId = info.HeroId,
                Name = info.Name,
                UnitType = info.UnitType,
                Color = info.Color,
                Level = info.Level,
                Team = world.Team[e.Id].TeamId,
                Position = info.Position,
                MaxHp = world.Health[e.Id].Max,
                Hp = world.Health[e.Id].Current,
                Attack = world.Attack[e.Id].Damage,
                Speed = world.Speed[e.Id].Value,
                MaxEnergy = energy?.Max ?? 0,
                Energy = energy?.Current ?? 0,
                Spells = world.Spells.GetValueOrDefault(e.Id)?.SpellIds.Select(SpellRegistry.Describe).ToList() ?? new(),
                Immunities = world.Immunities.GetValueOrDefault(e.Id)?.ImmuneTo.ToList() ?? new(),
                Aura = aura == null ? null : $"{aura.EffectName} {aura.Power} → {aura.TargetType}"
            };
        })
        .OrderBy(u => u.Team)
        .ThenBy(u => u.Position)
        .ToList();
}
