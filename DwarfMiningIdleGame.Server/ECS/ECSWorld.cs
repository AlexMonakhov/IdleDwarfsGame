public class World
{
    private int _nextId = 1;

    public List<Entity> Entities = new();

    public Dictionary<int, HealthComponent> Health = new();
    public Dictionary<int, AttackComponent> Attack = new();
    public Dictionary<int, SpeedComponent> Speed = new();
    public Dictionary<int, TeamComponent> Team = new();

    public Entity CreateEntity()
    {
        var entity = new Entity(_nextId++);
        Entities.Add(entity);
        return entity;
    }
}