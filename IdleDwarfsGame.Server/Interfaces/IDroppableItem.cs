using IdleDwarfsGame.Server.Enums;

public interface IChestDropTable
{
    int ChestLevel { get; }
    ChestType ChestType { get; }
    int Rolls { get; }
    IReadOnlyCollection<DropEntry> Entries { get; }
}

// Что выпало из сундука: Amount одинаковых штук (гемы складываются в стопку, героев создаётся Amount)
public record LootDrop(ItemType ItemType, string SubType, Rarity Rarity, int Level, int Amount);
