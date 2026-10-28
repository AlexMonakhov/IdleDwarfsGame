public class World
{
    private int _nextId = 1;

    public List<Entity> Entities = new();
    public List<IGlobalEffect> GlobalEffects { get; set; } = new();

    // Прибавка ко всему урону в бою, в процентах (арена PvP наращивает её по ходу боя)
    public int DamageBonusPercent { get; set; }

    public Dictionary<int, HealthComponent> Health = new();
    public Dictionary<int, AttackComponent> Attack = new();
    public Dictionary<int, SpeedComponent> Speed = new();
    public Dictionary<int, TeamComponent> Team = new();
    public Dictionary<int, UnitInfoComponent> Info = new();
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

    public bool IsAlive(int entityId) =>
        Health.TryGetValue(entityId, out var hp) && hp.Current > 0;

    public int GetPosition(int entityId) =>
        Info.TryGetValue(entityId, out var info) ? info.Position : entityId;

    public int? GetEnergy(int entityId) =>
        Energy.TryGetValue(entityId, out var energy) ? energy.Current : null;

    // Живые враги, отсортированные по позиции в отряде (первая позиция — передняя линия)
    public List<Entity> GetAliveEnemies(int entityId)
    {
        int team = Team[entityId].TeamId;
        return Entities
            .Where(e => Team[e.Id].TeamId != team && IsAlive(e.Id))
            .OrderBy(e => GetPosition(e.Id))
            .ToList();
    }

    public List<Entity> GetAliveAllies(int entityId)
    {
        int team = Team[entityId].TeamId;
        return Entities
            .Where(e => Team[e.Id].TeamId == team && IsAlive(e.Id))
            .OrderBy(e => GetPosition(e.Id))
            .ToList();
    }

    // Урон с учётом DamageBonusPercent. Считается до ApplyDamage, чтобы в лог шла та же цифра, что сняла HP
    public int ScaleDamage(int amount) =>
        DamageBonusPercent == 0 ? amount : (int)Math.Round(amount * (100 + DamageBonusPercent) / 100.0, MidpointRounding.AwayFromZero);

    // Наносит урон и возвращает фактический остаток HP (не ниже 0)
    public int ApplyDamage(int targetId, int amount)
    {
        var hp = Health[targetId];
        hp.Current = Math.Max(0, hp.Current - Math.Max(0, amount));
        return hp.Current;
    }

    // Лечит не выше максимума и возвращает фактически восстановленное HP
    public int ApplyHeal(int targetId, int amount)
    {
        var hp = Health[targetId];
        int before = hp.Current;
        hp.Current = Math.Min(hp.Max, hp.Current + Math.Max(0, amount));
        return hp.Current - before;
    }
}
