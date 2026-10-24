public interface IItemFactory
{
    ILootItem Create(ItemType type, Rarity rarity, int level, string subType = "");
}