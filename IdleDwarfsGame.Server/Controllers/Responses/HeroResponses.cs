// Герой в инвентаре: статы с бонусом прокачки, но без масштабирования уровнем арены
public class HeroInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public HeroElement Element { get; set; }
    public HeroProfession? Profession { get; set; }
    public int Level { get; set; }
    // null — герой не в отряде
    public int? PlaceInSquad { get; set; }
    // Герой в экспедиции: виден в инвентаре, но не может участвовать в других активностях
    public bool OnExpedition { get; set; }
    // Дворф работает в шахте — так же занят, как в экспедиции
    public bool InMine { get; set; }
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Speed { get; set; }
    public int MaxEnergy { get; set; }
    public List<string> Spells { get; set; } = new();
    public List<string> Immunities { get; set; } = new();
    public string? Aura { get; set; }
    public List<PassiveInfo> Passives { get; set; } = new();
    // Способности, которые можно извлечь, распылив дворфа (AbilityService)
    public List<AbilityInfo> Abilities { get; set; } = new();
    public List<EquipmentInfo> Equipment { get; set; } = new();
    public CombatPowerBreakdown Power { get; set; } = new();
    public HeroUpgradeInfo Upgrade { get; set; } = new();

    public static HeroInfo From(IEntityFactory factory, Hero hero, bool onExpedition = false, bool inMine = false)
    {
        var gear = EquipmentCatalog.CollectGear(hero);
        var aura = EntityFactory.ResolveAura(hero, gear);
        int upgradeLevel = HeroUpgradeService.ClampLevel(hero.Level, hero.UpgradeLevel);

        return new HeroInfo
        {
            Id = hero.Id,
            Name = hero.Name,
            UnitType = hero.UnitType,
            Element = hero.Element,
            Profession = hero.Profession,
            Level = hero.Level,
            PlaceInSquad = hero.PlaceInSquad,
            OnExpedition = onExpedition,
            InMine = inMine,
            MaxHp = HeroUpgradeService.ScaleStat(hero.MaxHp, upgradeLevel) + gear.Stats.MaxHp,
            Attack = HeroUpgradeService.ScaleStat(hero.Attack, upgradeLevel) + gear.Stats.Attack,
            Speed = hero.Speed + gear.Stats.Speed,
            MaxEnergy = hero.MaxEnergy + gear.Stats.MaxEnergy,
            Spells = hero.SpellIds.Select(SpellRegistry.Describe).ToList(),
            Immunities = hero.Immunities.ToList(),
            Aura = aura == null ? null : $"{aura.EffectName} {aura.Power} → {aura.TargetType}",
            Passives = EntityFactory.CreatePassives(hero, gear).Active
                .Select(p => new PassiveInfo
                {
                    Passive = p.Key,
                    Level = Math.Clamp(hero.PassiveLevel, PassiveBook.MinLevel, PassiveBook.MaxLevel),
                    Percent = p.Value.Percent,
                    Turns = p.Value.Turns,
                    Targets = p.Value.Targets
                })
                .ToList(),
            Abilities = AbilityInfo.Of(hero),
            Equipment = hero.Equipment.Select(EquipmentInfo.From).OfType<EquipmentInfo>().ToList(),
            Power = CombatPower.OfHero(factory, hero),
            Upgrade = HeroUpgradeService.Describe(factory, hero)
        };
    }
}

// Прокачка героя (HeroUpgradeService): текущий уровень, цена следующего и что он даст
public class HeroUpgradeInfo
{
    public int Level { get; set; }
    public int MaxLevel { get; set; }
    // null — уровень максимальный
    public HeroUpgradeCost? Cost { get; set; }
    public HeroUpgradePreview? Next { get; set; }
    // Сколько вложено в прокачку: при слиянии вернётся HeroUpgradeService.FusionRefund от этого
    public HeroUpgradeCost Invested { get; set; } = new();
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
