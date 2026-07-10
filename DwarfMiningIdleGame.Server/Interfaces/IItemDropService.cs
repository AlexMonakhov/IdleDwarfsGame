using DwarfMiningIdleGame.Server.Enums;

public interface IItemDropService
{
    Task<IReadOnlyCollection<ILootItem>> OpenChestAsync(int chestLevel, ChestType chestType);
}