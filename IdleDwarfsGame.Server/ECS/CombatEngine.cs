public class CombatEngine : ICombatEngine
{
    private List<ICombatSystem> _systems;

    public CombatEngine()
    {
        var spellRegistry = new Dictionary<int, ISpell>();
        spellRegistry[1] = new Fireball(1);
        spellRegistry[2] = new Fireball(2);
        spellRegistry[4] = new Lightning(1);

        _systems = new List<ICombatSystem>
        {
            new TurnOrderSystem(),
            new RoundStartSystem(),
            new AuraSystem(),
            new StatusEffectSystem(EffectPhase.Start),
            new SpellSystem(spellRegistry),
            new DamageSystem(),
            new StatusEffectSystem(EffectPhase.End),
            new HealthSystem()
        };
    }

    public CombatContext Run(World world)
    {
        var context = new CombatContext();

        while (!context.CombatEnded)
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