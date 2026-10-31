// Описание предмета: Elements пустой — носить может любой герой, иначе только герой одной из этих стихий
public record EquipmentDefinition(
    string Key,
    EquipmentType Type,
    Rarity Rarity,
    ItemStats Stats,
    HeroElement[] Elements,
    ItemEffect[] Effects)
{
    public bool Fits(HeroElement element) => Elements.Length == 0 || Elements.Contains(element);
}

// Все предметы игры. Ключ хранится в БД (PlayerEquipment.ItemKey, DropEntry.SubType) — не переименовывать без миграции.
// Название предмета — на фронте, equipment.items.<Key>.
// Новый предмет в конце каталога меняет таблицы дропа: нужна миграция (LootSeedData)
public static class EquipmentCatalog
{
    private const int MeteorLevel1 = 13;

    private static readonly HeroElement[] AnyElement = [];

    public static readonly IReadOnlyList<EquipmentDefinition> All =
    [
        // ===== Оружие: атака, сильные предметы меняют спеллы своей стихии =====
        new("RustyAxe", EquipmentType.Weapon, Rarity.Common, new(Attack: 2), AnyElement, []),
        new("DwarvenHammer", EquipmentType.Weapon, Rarity.Uncommon, new(Attack: 4, MaxHp: 10), AnyElement, []),
        new("EmberMace", EquipmentType.Weapon, Rarity.Rare, new(Attack: 6), [HeroElement.Fire],
            [new SpellDamageEffect("Fireball", 20)]),
        new("StormSpear", EquipmentType.Weapon, Rarity.Rare, new(Attack: 6, Speed: 1), [HeroElement.Lightning],
            [new SpellTargetsEffect("Lightning", 1)]),
        new("BlackIceSword", EquipmentType.Weapon, Rarity.Mythic, new(Attack: 9), [HeroElement.Frost, HeroElement.Dark],
            [new SpellDamageEffect("Frost Bolt", 25), new SpellDamageEffect("Shadow Bolt", 25)]),
        new("ThornStaff", EquipmentType.Weapon, Rarity.Mythic, new(Attack: 8, MaxHp: 20), [HeroElement.Nature],
            [new SpellTargetsEffect("Venom Thorns", 1)]),
        new("DawnBlade", EquipmentType.Weapon, Rarity.Legendary, new(Attack: 12, MaxHp: 30), [HeroElement.Radiance],
            [new SpellTargetsEffect("Prism Ray", 1), new SpellDamageEffect("Prism Ray", 25)]),
        new("ArmageddonBlade", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 16), [HeroElement.Fire],
            [new SpellTargetsEffect("Flame Wave", 1), new SpellProcEffect("Flame Wave", MeteorLevel1, 50)]),

        // ===== Шлемы: живучесть и ауры. Герою без ауры шлем с AuraGrant её даёт, остальные усиливают имеющуюся =====
        new("IronHelm", EquipmentType.Helmet, Rarity.Common, new(MaxHp: 15), AnyElement, []),
        new("MinerHelm", EquipmentType.Helmet, Rarity.Uncommon, new(MaxHp: 25), AnyElement,
            [new AuraPowerEffect(15)]),
        new("EmberCrown", EquipmentType.Helmet, Rarity.Rare, new(MaxHp: 30), [HeroElement.Fire],
            [new AuraGrantEffect("Burn", AuraTarget.Enemies, Power: 3), new AuraPowerEffect(20)]),
        new("FrostCrown", EquipmentType.Helmet, Rarity.Rare, new(MaxHp: 30), [HeroElement.Frost],
            [new AuraGrantEffect("Frost", AuraTarget.Enemies, Power: 5), new AuraPowerEffect(20)]),
        new("GroveCirclet", EquipmentType.Helmet, Rarity.Mythic, new(MaxHp: 45), [HeroElement.Nature, HeroElement.Radiance],
            [new AuraGrantEffect("Regen", AuraTarget.Allies, Power: 3), new AuraDurationEffect(1)]),
        new("WarlordHelm", EquipmentType.Helmet, Rarity.Legendary, new(MaxHp: 60, Attack: 3), AnyElement,
            [new AuraPowerEffect(40), new AuraIntervalEffect(1)]),

        // ===== Амулеты: энергия и скорость =====
        new("BoneNecklace", EquipmentType.Neck, Rarity.Common, new(Speed: 1), AnyElement, []),
        new("MoonstonePendant", EquipmentType.Neck, Rarity.Uncommon, new(StartEnergy: 10), AnyElement, []),
        new("RunicAmulet", EquipmentType.Neck, Rarity.Rare, new(EnergyPerAttack: 4), AnyElement, []),
        new("StormAmulet", EquipmentType.Neck, Rarity.Mythic, new(Speed: 2, EnergyPerAttack: 4), [HeroElement.Lightning, HeroElement.Frost], []),
        new("HeartOfTheMountain", EquipmentType.Neck, Rarity.Legendary, new(MaxHp: 40, StartEnergy: 20, EnergyPerAttack: 5), AnyElement, []),

        // ===== Кольца: усиливают пассивки героя =====
        new("CopperRing", EquipmentType.Ring, Rarity.Common, new(MaxHp: 5, Attack: 1), AnyElement, []),
        new("RingOfTheUnderdog", EquipmentType.Ring, Rarity.Uncommon, new(MaxHp: 10), AnyElement,
            [new PassivePercentEffect(HeroPassive.SurvivalOfTheWeakest, 3)]),
        new("SpitefulRing", EquipmentType.Ring, Rarity.Uncommon, new(Attack: 1), AnyElement,
            [new PassivePercentEffect(HeroPassive.SharpTongue, 5)]),
        new("TwinBand", EquipmentType.Ring, Rarity.Uncommon, new(Attack: 1), AnyElement,
            [new PassivePercentEffect(HeroPassive.DoubleStrike, 10)]),
        new("RingOfLingeringWords", EquipmentType.Ring, Rarity.Rare, new(Attack: 2), AnyElement,
            [new PassiveTurnsEffect(HeroPassive.SharpTongue, 1)]),
        new("RingOfTheStoneSkin", EquipmentType.Ring, Rarity.Rare, new(MaxHp: 20), AnyElement,
            [new PassivePercentEffect(HeroPassive.SurvivalOfTheWeakest, 5)]),
        new("RingOfCleaving", EquipmentType.Ring, Rarity.Mythic, new(Attack: 3), AnyElement,
            [new PassiveTargetsEffect(HeroPassive.DoubleStrike, 1)]),
        new("RingOfWildfire", EquipmentType.Ring, Rarity.Legendary, new(Attack: 4), AnyElement,
            [new PassiveTargetsEffect(HeroPassive.SharpTongue, 1), new PassivePercentEffect(HeroPassive.SharpTongue, 10)])
    ];

    private static readonly Dictionary<string, EquipmentDefinition> ByKey = All.ToDictionary(i => i.Key);

    public static EquipmentDefinition? Find(string key) => ByKey.GetValueOrDefault(key);

    public static EquipmentSlot[] SlotsFor(EquipmentType type) => type switch
    {
        EquipmentType.Weapon => [EquipmentSlot.Weapon],
        EquipmentType.Helmet => [EquipmentSlot.Helmet],
        EquipmentType.Neck => [EquipmentSlot.Neck],
        EquipmentType.Ring => [EquipmentSlot.Ring1, EquipmentSlot.Ring2],
        _ => []
    };

    // Эффекты всех надетых предметов; неизвестные ключи отбрасываем, чтобы кривые данные в БД не ломали бой
    public static GearComponent CollectGear(Hero hero)
    {
        var gear = new GearComponent();
        foreach (var definition in hero.Equipment.Select(e => Find(e.ItemKey)).OfType<EquipmentDefinition>())
        {
            gear.Stats += definition.Stats;
            foreach (var effect in definition.Effects)
                effect.Apply(gear);
        }
        return gear;
    }
}
