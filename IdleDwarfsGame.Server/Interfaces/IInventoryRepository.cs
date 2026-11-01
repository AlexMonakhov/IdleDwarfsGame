using IdleDwarfsGame.Server.Entities.GameEntities;

public interface IInventoryRepository
{
    // Закрытые сундуки игрока вместе с таблицей дропа (тип и уровень сундука)
    Task<List<PlayerChest>> GetClosedChestsAsync(Guid playerId);
    Task<PlayerChest?> GetClosedChestAsync(Guid playerId, Guid chestId);
    Task<List<PlayerItem>> GetItemsAsync(Guid playerId);

    // Открывает сундук и кладёт выпавшее в инвентарь одной транзакцией.
    // false — сундук уже открыли параллельным запросом, ничего не выдано
    Task<bool> OpenChestAsync(PlayerChest chest, IReadOnlyList<LootDrop> items, IReadOnlyList<Hero> heroes, IReadOnlyList<PlayerEquipment>? equipment = null);
}
