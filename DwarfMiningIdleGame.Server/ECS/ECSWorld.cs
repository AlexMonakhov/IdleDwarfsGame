public class World
{
    private int _nextId = 1;

    public List<Entity> Entities = new();
    public List<IGlobalEffect> GlobalEffects { get; set; } = new();

    public Dictionary<int, HealthComponent> Health = new();
    public Dictionary<int, AttackComponent> Attack = new();
    public Dictionary<int, SpeedComponent> Speed = new();
    public Dictionary<int, TeamComponent> Team = new();
    public Dictionary<int, SpellComponent> Spells = new();
    public Dictionary<int, StatusEffectComponent> StatusEffects = new();
    public Dictionary<int, ImmunityComponent> Immunities = new();
    public Dictionary<int, EnergyComponent> Energy = new();
    public Dictionary<int, TurnStateComponent> TurnState = new();
    public Dictionary<int, AuraComponent> Auras = new();

    public Entity CreateEntity()
    {
        var entity = new Entity(_nextId++);
        Entities.Add(entity);
        return entity;
    }

    public void AddGlobalEffect(IGlobalEffect effect)
    {
        GlobalEffects.Add(effect);
    }
}