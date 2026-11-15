using IdleDwarfsGame.Server.Entities.GameEntities;

public interface IInventoryRepository
{
    // Закрытые сундуки игрока вместе с таблицей дропа (тип и уровень сундука)
    Task<List<PlayerChest>> GetClosedChestsAsync(Guid playerId);
    Task<PlayerChest?> GetClosedChestAsync(Guid playerId, Guid chestId);
    Task<List<PlayerItem>> GetItemsAsync(Guid playerId);

    // Открывает сундук и кладёт выпавшее в инвентарь одной транзакцией вместе с доп. наградами (золото, опыт, валюта).
    // false — сундук уже открыли параллельным запросом, ничего не выдано
    Task<bool> OpenChestAsync(PlayerChest chest, IReadOnlyList<LootDrop> items, IReadOnlyList<Hero> heroes,
        IReadOnlyList<PlayerEquipment>? equipment = null, ChestCurrencyGrant? currency = null);
}

// Валюты из доп. наград сундука. EventStart — ключ кошелька текущего ивента (null — ивента нет, валюту не начислять)
public record ChestCurrencyGrant(long Gold, long Experience, long RainbowCoins, long EventCurrency, DateTime? EventStart);
