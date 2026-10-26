// Герой в инвентаре: статы с бонусом прокачки, но без масштабирования уровнем арены
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
    public HeroUpgradeInfo Upgrade { get; set; } = new();

    public static HeroInfo From(IEntityFactory factory, Hero hero, bool onExpedition = false) => new()
    {
        Id = hero.Id,
        Name = hero.Name,
        UnitType = hero.UnitType,
        Element = hero.Element,
        Level = hero.Level,
        PlaceInSquad = hero.PlaceInSquad,
        OnExpedition = onExpedition,
        MaxHp = HeroUpgradeService.ScaleStat(hero.MaxHp, hero.UpgradeLevel),
        Attack = HeroUpgradeService.ScaleStat(hero.Attack, hero.UpgradeLevel),
        Speed = hero.Speed,
        MaxEnergy = hero.MaxEnergy,
        Spells = hero.SpellIds.Select(SpellRegistry.Describe).ToList(),
        Immunities = hero.Immunities.ToList(),
        Aura = string.IsNullOrWhiteSpace(hero.AuraEffect) ? null : $"{hero.AuraEffect} {hero.AuraPower} → {hero.AuraTarget}",
        Power = CombatPower.OfHero(factory, hero),
        Upgrade = HeroUpgradeService.Describe(factory, hero)
    };
}

// Прокачка героя (HeroUpgradeService): текущий уровень, цена следующего и что он даст
public class HeroUpgradeInfo
{
    public int Level { get; set; }
    public int MaxLevel { get; set; }
    // null — уровень максимальный
    public HeroUpgradeCost? Cost { get; set; }
    public HeroUpgradePreview? Next { get; set; }
}

public class HeroUpgradeCost
{
    public int Gold { get; set; }
    public int Experience { get; set; }
}

// Статы и мощь героя после следующего уровня прокачки
public class HeroUpgradePreview
{
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Power { get; set; }
}

// Ответ на прокачку: обновлённый герой и остаток ресурсов, чтобы шапка обновилась без лишнего запроса
public class HeroUpgradeResult
{
    public HeroInfo Hero { get; set; } = new();
    public PlayerResources Resources { get; set; } = new();
}
