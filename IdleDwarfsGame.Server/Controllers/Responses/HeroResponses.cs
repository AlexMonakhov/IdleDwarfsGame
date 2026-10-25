// Герой в инвентаре: базовые статы без масштабирования уровнем арены
public class HeroInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public HeroElement Element { get; set; }
    public int Level { get; set; }
    // null — герой не в отряде
    public int? PlaceInSquad { get; set; }
    // Герой в экспедиции: виден в инвентаре, но не может участвовать в других активностях
    public bool OnExpedition { get; set; }
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Speed { get; set; }
    public int MaxEnergy { get; set; }
    public List<string> Spells { get; set; } = new();
    public List<string> Immunities { get; set; } = new();
    public string? Aura { get; set; }
    public CombatPowerBreakdown Power { get; set; } = new();

    public static HeroInfo From(Hero hero, CombatPowerBreakdown power, bool onExpedition = false) => new()
    {
        Id = hero.Id,
        Name = hero.Name,
        UnitType = hero.UnitType,
        Element = hero.Element,
        Level = hero.Level,
        PlaceInSquad = hero.PlaceInSquad,
        OnExpedition = onExpedition,
        MaxHp = hero.MaxHp,
        Attack = hero.Attack,
        Speed = hero.Speed,
        MaxEnergy = hero.MaxEnergy,
        Spells = hero.SpellIds.Select(SpellRegistry.Describe).ToList(),
        Immunities = hero.Immunities.ToList(),
        Aura = string.IsNullOrWhiteSpace(hero.AuraEffect) ? null : $"{hero.AuraEffect} {hero.AuraPower} → {hero.AuraTarget}",
        Power = power
    };
}
