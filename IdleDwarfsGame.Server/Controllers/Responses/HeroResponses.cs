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
    // Сколько энергии даёт один удар: база, предметы и «Резонанс рун»
    public int EnergyPerAttack { get; set; }
    public List<string> Spells { get; set; } = new();
    // Те же заклинания с ценой в энергии и силой каста (порядок как у Spells)
    public List<SpellDetail> SpellDetails { get; set; } = new();
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
        int upgradeLevel = HeroUpgradeService.ClampLevel(hero.Level, hero.UpgradeLevel);
        int attack = HeroUpgradeService.ScaleStat(hero.Attack, upgradeLevel) + gear.Stats.Attack;
        var aura = EntityFactory.ResolveAura(hero, gear, attack);
        var passives = EntityFactory.CreatePassives(hero, gear).Active;
        int resonance = passives.GetValueOrDefault(HeroPassive.RuneResonance).Percent;
        int sorcerer = passives.GetValueOrDefault(HeroPassive.Sorcerer).Percent;

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
            Attack = attack,
            Speed = hero.Speed + gear.Stats.Speed,
            MaxEnergy = hero.MaxEnergy + gear.Stats.MaxEnergy,
            EnergyPerAttack = hero.EnergyPerAttack + gear.Stats.EnergyPerAttack + resonance,
            Spells = hero.SpellIds.Select(SpellRegistry.Describe).ToList(),
            SpellDetails = hero.SpellIds
                .Select(id => SpellRegistry.All.GetValueOrDefault(id))
                .OfType<ISpell>()
                .Select(spell => SpellDetail.Of(spell, attack, gear, sorcerer))
                .ToList(),
            Immunities = hero.Immunities.ToList(),
            Aura = aura == null ? null : $"{aura.EffectName} {aura.Power} → {aura.TargetType}",
            Passives = passives
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

// Заклинание героя для подробной карточки: цена каста и что он даёт при текущей атаке
public class SpellDetail
{
    // Как в HeroInfo.Spells: «Fireball 2»
    public string Spell { get; set; } = string.Empty;
    public int EnergyCost { get; set; }
    // Урон по одной цели вместе с эффектами (горение, яд…), с учётом предметов и «Чародея»
    public int Damage { get; set; }
    // Сколько врагов задевает каст, если их 5 (полный отряд)
    public int Targets { get; set; }
    public int Healing { get; set; }
    // Спеллы, которые предметы вызывают после этого каста (SpellProcEffect): берутся из снаряжения, без списка вручную
    public List<SpellProcDetail> Procs { get; set; } = new();

    private const int FullSquad = 5;

    public static SpellDetail Of(ISpell spell, int attack, GearComponent gear, int sorcererPercent)
    {
        var single = spell.Estimate(attack, 1);
        var full = spell.Estimate(attack, FullSquad);
        double perTarget = single.Damage;
        int targets = perTarget > 0 ? (int)Math.Round(full.Damage / perTarget) : 0;

        if (gear.Spells.TryGetValue(spell.Name, out var modifier))
        {
            perTarget = perTarget * (100 + modifier.DamagePercent) / 100;
            if (targets > 0)
                targets = Math.Min(FullSquad, targets + modifier.ExtraTargets);
        }
        perTarget = perTarget * (100 + sorcererPercent) / 100;

        // Как в BaseSpell.CastTriggered: урон срабатывания — процент от атаки, без «Чародея» и бонусов предметов
        var procs = (modifier?.Procs ?? [])
            .Where(p => SpellRegistry.All.ContainsKey(p.SpellId))
            .Select(p => new SpellProcDetail
            {
                Spell = SpellRegistry.Describe(p.SpellId),
                Damage = attack * p.DamagePercent / 100,
                Chance = p.Chance
            })
            .ToList();

        return new SpellDetail
        {
            Spell = SpellRegistry.Describe(spell.Id),
            EnergyCost = spell.EnergyCost,
            Damage = (int)Math.Round(perTarget),
            Targets = targets,
            Healing = (int)Math.Round(single.Healing),
            Procs = procs
        };
    }
}

// Срабатывание с предмета: какой спелл, сколько урона по каждой цели и с каким шансом на цель
public class SpellProcDetail
{
    // «Frost Bolt 1», как SpellDetail.Spell
    public string Spell { get; set; } = string.Empty;
    public int Damage { get; set; }
    // %, 100 — по каждой цели всегда
    public int Chance { get; set; }
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
