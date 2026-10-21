public class LootItem : ILootItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public ItemType ItemType { get; init; }
    public string SubType { get; init; } = string.Empty;
    public Rarity Rarity { get; init; }
    public int Level { get; init; }
}