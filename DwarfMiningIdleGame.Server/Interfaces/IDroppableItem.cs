using DwarfMiningIdleGame.Server.Enums;

public interface ILootItem
{
    Guid Id { get; }
    string Name { get; }
    ItemType ItemType { get; }
    string SubType { get; }
    Rarity Rarity { get; }
    int Level { get; }
}

public interface IChestDropTable
{
    int ChestLevel { get; }
    ChestType ChestType { get; }
    IReadOnlyCollection<DropEntry> Entries { get; }
}




