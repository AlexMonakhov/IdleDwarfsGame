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
    public Dictionary<int, PassiveComponent> Passives = new();
    public Dictionary<int, GearComponent> Gear = new();

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

    public PassiveValues? GetPassive(int entityId, HeroPassive passive) =>
        Passives.TryGetValue(entityId, out var component) && component.Active.TryGetValue(passive, out var values) ? values : null;

    // Урон удара или спелла по цели: прибавка арены, добивание атакующего, затем защита цели.
    // Тики статусов сюда не идут — у них нет атакующего
    public int ResolveHit(int attackerId, int targetId, int amount)
    {
        int damage = ScaleDamage(amount);
        if (GetPassive(attackerId, HeroPassive.Executioner) is { } executioner
            && Health[targetId].Current * 100 < Health[targetId].Max * PassiveBook.ExecuteThreshold)
        {
            damage = Percent(damage, 100 + executioner.Percent);
        }
        if (GetPassive(targetId, HeroPassive.SurvivalOfTheWeakest) is { } survival
            && Attack[attackerId].Damage > Attack[targetId].Damage)
        {
            damage = Percent(damage, Math.Max(0, 100 - survival.Percent));
        }
        if (GetPassive(targetId, HeroPassive.StoneSkin) is { } stoneSkin)
            damage = Percent(damage, Math.Max(0, 100 - stoneSkin.Percent));
        return damage;
    }

    private static int Percent(int amount, int percent) =>
        (int)Math.Round(amount * percent / 100.0, MidpointRounding.AwayFromZero);

    // Герои, которых только что спас LastStand: лечение и лог — в HealthSystem в конце хода
    public List<int> PendingLastStands { get; } = new();

    // Наносит урон и возвращает фактический остаток HP (не ниже 0).
    // Смертельный урон по герою с неиспользованным LastStand оставляет ему 1 HP
    public int ApplyDamage(int targetId, int amount)
    {
        var hp = Health[targetId];
        hp.Current = Math.Max(0, hp.Current - Math.Max(0, amount));
        if (hp.Current == 0 && Passives.TryGetValue(targetId, out var passives) && !passives.LastStandUsed
            && passives.Active.ContainsKey(HeroPassive.LastStand))
        {
            passives.LastStandUsed = true;
            hp.Current = 1;
            PendingLastStands.Add(targetId);
        }
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
