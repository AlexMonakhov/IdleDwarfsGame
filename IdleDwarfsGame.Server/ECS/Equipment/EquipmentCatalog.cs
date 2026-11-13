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

// Все предметы игры. Атака и HP предметов — плоские прибавки под статы дворфов 1–2 ранга (атака ~100, HP ~800).
// Ключ хранится в БД (PlayerEquipment.ItemKey, DropEntry.SubType) — не переименовывать без миграции.
// Название предмета — на фронте, equipment.items.<Key>.
// Новый предмет в конце каталога меняет таблицы дропа: нужна миграция (LootSeedData)
public static class EquipmentCatalog
{
    // Id спеллов 1 уровня (SpellRegistry), которые артефакты вызывают или берут как визуал срабатывания
    private const int FireballLevel1 = 1;
    private const int LightningLevel1 = 4;
    private const int HealingRuneLevel1 = 7;
    private const int FlameWaveLevel1 = 10;
    private const int MeteorLevel1 = 13;
    private const int FrostBoltLevel1 = 16;
    private const int VenomThornsLevel1 = 31;
    private const int ShadowBoltLevel1 = 37;
    private const int PrismRayLevel1 = 46;
    private const int RadiantNovaLevel1 = 49;

    private static readonly HeroElement[] AnyElement = [];

    public static readonly IReadOnlyList<EquipmentDefinition> All =
    [
        // ===== Оружие: атака, сильные предметы меняют спеллы своей стихии =====
        new("RustyAxe", EquipmentType.Weapon, Rarity.Common, new(Attack: 20), AnyElement, []),
        new("DwarvenHammer", EquipmentType.Weapon, Rarity.Uncommon, new(Attack: 40, MaxHp: 80), AnyElement, []),
        new("EmberMace", EquipmentType.Weapon, Rarity.Rare, new(Attack: 60), [HeroElement.Fire],
            [new SpellDamageEffect("Fireball", 20)]),
        new("StormSpear", EquipmentType.Weapon, Rarity.Rare, new(Attack: 60, Speed: 1), [HeroElement.Lightning],
            [new SpellTargetsEffect("Lightning", 1)]),
        new("BlackIceSword", EquipmentType.Weapon, Rarity.Mythic, new(Attack: 90), [HeroElement.Frost, HeroElement.Dark],
            [new SpellDamageEffect("Frost Bolt", 25), new SpellDamageEffect("Shadow Bolt", 25)]),
        new("ThornStaff", EquipmentType.Weapon, Rarity.Mythic, new(Attack: 80, MaxHp: 160), [HeroElement.Nature],
            [new SpellTargetsEffect("Venom Thorns", 1)]),
        new("DawnBlade", EquipmentType.Weapon, Rarity.Legendary, new(Attack: 120, MaxHp: 240), [HeroElement.Radiance],
            [new SpellTargetsEffect("Prism Ray", 1), new SpellDamageEffect("Prism Ray", 25)]),
        new("ArmageddonBlade", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 160), [HeroElement.Fire],
            [new SpellTargetsEffect("Flame Wave", 1), new SpellProcEffect("Flame Wave", MeteorLevel1, 50)]),

        // ===== Шлемы: живучесть и ауры. Герою без ауры шлем с AuraGrant её даёт, остальные усиливают имеющуюся =====
        new("IronHelm", EquipmentType.Helmet, Rarity.Common, new(MaxHp: 120), AnyElement, []),
        new("MinerHelm", EquipmentType.Helmet, Rarity.Uncommon, new(MaxHp: 200), AnyElement,
            [new AuraPowerEffect(15)]),
        new("EmberCrown", EquipmentType.Helmet, Rarity.Rare, new(MaxHp: 240), [HeroElement.Fire],
            [new AuraGrantEffect("Burn", AuraTarget.Enemies, Power: 8), new AuraPowerEffect(20)]),
        new("FrostCrown", EquipmentType.Helmet, Rarity.Rare, new(MaxHp: 240), [HeroElement.Frost],
            [new AuraGrantEffect("Frost", AuraTarget.Enemies, Power: 5), new AuraPowerEffect(20)]),
        new("GroveCirclet", EquipmentType.Helmet, Rarity.Mythic, new(MaxHp: 360), [HeroElement.Nature, HeroElement.Radiance],
            [new AuraGrantEffect("Regen", AuraTarget.Allies, Power: 10), new AuraDurationEffect(1)]),
        new("WarlordHelm", EquipmentType.Helmet, Rarity.Legendary, new(MaxHp: 480, Attack: 30), AnyElement,
            [new AuraPowerEffect(40), new AuraIntervalEffect(1)]),

        // ===== Амулеты: энергия и скорость =====
        new("BoneNecklace", EquipmentType.Neck, Rarity.Common, new(Speed: 1), AnyElement, []),
        new("MoonstonePendant", EquipmentType.Neck, Rarity.Uncommon, new(StartEnergy: 10), AnyElement, []),
        new("RunicAmulet", EquipmentType.Neck, Rarity.Rare, new(EnergyPerAttack: 4), AnyElement, []),
        new("StormAmulet", EquipmentType.Neck, Rarity.Mythic, new(Speed: 2, EnergyPerAttack: 4), [HeroElement.Lightning, HeroElement.Frost], []),
        new("HeartOfTheMountain", EquipmentType.Neck, Rarity.Legendary, new(MaxHp: 320, StartEnergy: 20, EnergyPerAttack: 5), AnyElement, []),

        // ===== Кольца: усиливают пассивки героя =====
        new("CopperRing", EquipmentType.Ring, Rarity.Common, new(MaxHp: 40, Attack: 10), AnyElement, []),
        new("RingOfTheUnderdog", EquipmentType.Ring, Rarity.Uncommon, new(MaxHp: 80), AnyElement,
            [new PassivePercentEffect(HeroPassive.SurvivalOfTheWeakest, 3)]),
        new("SpitefulRing", EquipmentType.Ring, Rarity.Uncommon, new(Attack: 10), AnyElement,
            [new PassivePercentEffect(HeroPassive.SharpTongue, 5)]),
        new("TwinBand", EquipmentType.Ring, Rarity.Uncommon, new(Attack: 10), AnyElement,
            [new PassivePercentEffect(HeroPassive.DoubleStrike, 10)]),
        new("RingOfLingeringWords", EquipmentType.Ring, Rarity.Rare, new(Attack: 20), AnyElement,
            [new PassiveTurnsEffect(HeroPassive.SharpTongue, 1)]),
        new("RingOfTheStoneSkin", EquipmentType.Ring, Rarity.Rare, new(MaxHp: 160), AnyElement,
            [new PassivePercentEffect(HeroPassive.SurvivalOfTheWeakest, 5)]),
        new("RingOfCleaving", EquipmentType.Ring, Rarity.Mythic, new(Attack: 30), AnyElement,
            [new PassiveTargetsEffect(HeroPassive.DoubleStrike, 1)]),
        new("RingOfWildfire", EquipmentType.Ring, Rarity.Legendary, new(Attack: 40), AnyElement,
            [new PassiveTargetsEffect(HeroPassive.SharpTongue, 1), new PassivePercentEffect(HeroPassive.SharpTongue, 10)]),

        // ===== Добавлено позже: дописываем в конец, чтобы не сдвигать таблицы дропа =====
        new("GlacialRelic", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 160), [HeroElement.Frost],
            [new SpellProcEffect("Blizzard", FrostBoltLevel1, 100, Chance: 50)]),

        // ===== Артефакты: по одному на каждый спелл стихии, у каждого своя механика (SpellArtifactEffects) =====
        // Огонь
        new("PhoenixStaff", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 160), [HeroElement.Fire],
            [new SpellEchoEffect("Fireball", 70, EchoTrigger.OnKill, Value: 2)]),
        new("VolcanoHeart", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 150, MaxHp: 200), [HeroElement.Fire],
            [new DetonateStatusEffect("Meteor", "Burn", 150, FlameWaveLevel1)]),
        // Лёд
        new("WinterShard", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 150, Speed: 1), [HeroElement.Frost],
            [new UpgradeStatusEffect("Frost Bolt", From: "Frost", To: "Freeze", Turns: 1)]),
        new("GlacierShackles", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 140, MaxHp: 280), [HeroElement.Frost],
            [new ShatterEffect("Glacial Prison", "Freeze", 80, FrostBoltLevel1)]),
        // Молния
        new("ThunderHammer", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 170), [HeroElement.Lightning],
            [new SpellEchoEffect("Lightning", 100, EchoTrigger.EveryNthCast, Value: 3)]),
        new("StormCoil", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 140, Speed: 2), [HeroElement.Lightning],
            [new EnergyRefundEffect("Chain Lightning", PerHit: 4, PerKill: 12)]),
        new("ThunderHorn", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 150, MaxHp: 200), [HeroElement.Lightning],
            [new SpellProcEffect("Thunderclap", LightningLevel1, 120, Condition: ProcCondition.StatusApplied, Status: "Stun")]),
        // Природа
        new("LifeTreeSeed", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 110, MaxHp: 400), [HeroElement.Nature],
            [new OverhealProcEffect("Healing Rune", VenomThornsLevel1, 100)]),
        new("WyvernSting", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 160), [HeroElement.Nature],
            [new SpreadOnDeathEffect("Venom Thorns", "Poison", Targets: 2, VenomThornsLevel1)]),
        new("ElderGroveStaff", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 120, MaxHp: 360), [HeroElement.Nature],
            [new HealAlliesPerStatusEffect("Entangling Roots", "Stun", 120, HealingRuneLevel1)]),
        // Тьма
        new("AbyssalSigil", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 160), [HeroElement.Dark],
            [new MarkTargetsEffect("Shadow Bolt", 30, Turns: 2)]),
        new("CurseDoll", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 150, MaxHp: 200), [HeroElement.Dark],
            [new StatusTickProcEffect("Hex", "Curse", Chance: 25, ShadowBoltLevel1, 60)]),
        new("ReaperScythe", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 150, MaxHp: 240), [HeroElement.Dark],
            [new ConsumedCurseHealEffect("Soul Harvest", 50)]),
        // Свет
        new("PrismLens", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 160), [HeroElement.Radiance],
            [new SpellProcPoolEffect("Prism Ray", [FireballLevel1, FrostBoltLevel1, LightningLevel1, ShadowBoltLevel1, VenomThornsLevel1], 70)]),
        new("SunDisc", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 120, MaxHp: 360), [HeroElement.Radiance],
            [new CleanseAlliesEffect("Radiant Nova", Count: 1, 50, PrismRayLevel1)]),
        new("MorningStar", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 110, MaxHp: 400), [HeroElement.Radiance],
            [new SpellProcEffect("Dawnlight", RadiantNovaLevel1, 40, Condition: ProcCondition.AllyBelowHp, Threshold: 30)]),
        // Кровь
        new("BloodMoonFang", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 160), [HeroElement.Blood],
            [new SpellEchoEffect("Life Drain", 60, EchoTrigger.TargetBelowHp, Value: 50)]),
        new("ThousandCutsDagger", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 150, Speed: 1), [HeroElement.Blood],
            [new StackBurstEffect("Hemorrhage", "Bleed", Stacks: 5, Ticks: 3)]),
        new("SacrificialAltar", EquipmentType.Weapon, Rarity.Artifact, new(Attack: 120, MaxHp: 480), [HeroElement.Blood],
            [new PaidHpHealEffect("Blood Price", 100)])
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
