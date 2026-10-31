// В DropEntry хранится числом — значения не переставлять
public enum ItemType
{
    Equipment = 0,
    Material = 1,
    Gem = 2,
    // Герой из сундука: ItemLevel — ранг (Hero.Level), SubType — стихия или пусто (случайная)
    Hero = 3
}

// Вид предмета: определяет, в какой слот он надевается (EquipmentCatalog.SlotsFor)
public enum EquipmentType
{
    Weapon,
    Helmet,
    Neck,
    Ring
}

// Слот на герое. Хранится строкой
public enum EquipmentSlot
{
    Weapon,
    Helmet,
    Neck,
    Ring1,
    Ring2
}

public enum MaterialSubType
{
    Ore,
    Gem,
    Herb,
    Dust
}

// Камни стихий (HeroElement): рубин — огонь, сапфир — мороз, топаз — молния, изумруд — природа,
// аметист — тьма, алмаз (бриллиант) — сияние
public enum GemType
{
    Ruby,
    Emerald,
    Sapphire,
    Diamond,
    Amethyst,
    Topaz
}

// В DropEntry хранится числом, в PlayerItems — строкой: значения не переставлять и не переименовывать без миграции
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Mythic,
    Legendary,
    Artifact
}
