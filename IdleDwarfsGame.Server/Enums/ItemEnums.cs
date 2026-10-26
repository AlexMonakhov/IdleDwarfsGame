// В DropEntry хранится числом — значения не переставлять
public enum ItemType
{
    Equipment = 0,
    Material = 1,
    Gem = 2,
    // Герой из сундука: ItemLevel — ранг (Hero.Level), SubType — стихия или пусто (случайная)
    Hero = 3
}

public enum EquipmentSubType
{
    Armor,
    Weapon,
    Accessory
}

public enum MaterialSubType
{
    Ore,
    Gem,
    Herb,
    Dust
}

// Камни стихий (HeroElement): рубин — огонь, сапфир — мороз, топаз — молния, изумруд — природа
public enum GemType
{
    Ruby,
    Emerald,
    Sapphire,
    Diamond,
    Amethyst,
    Topaz
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
