public class ItemFactory : IItemFactory
{
    public ILootItem Create(ItemType type, Rarity rarity, int level, string subType = "")
    {
        return new LootItem
        {
            Name = $"{rarity} {subType} {type} Lvl {level}",
            ItemType = type,
            SubType = subType,
            Rarity = rarity,
            Level = level
        };
    }
}