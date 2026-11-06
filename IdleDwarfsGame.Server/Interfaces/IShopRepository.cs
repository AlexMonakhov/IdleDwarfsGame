public interface IShopRepository
{
    Task<ShopState?> GetStateAsync(Guid playerId, ShopKind shop);
    Task<List<ShopOffer>> GetOffersAsync(Guid playerId, ShopKind shop);
    Task<ShopOffer?> GetOfferAsync(Guid playerId, Guid offerId);
    Task<ShopWallet> GetWalletAsync(Guid playerId);

    /// <summary>
    /// Replaces every unlocked offer of the shop with <paramref name="fresh"/> and saves the new state.
    /// The state row must still have <paramref name="expectedVersion"/> (null — the row must not exist yet);
    /// <paramref name="rainbowCost"/> rainbow coins are charged in the same transaction.
    /// </summary>
    Task<ShopSaveOutcome> RefreshAsync(ShopState state, int? expectedVersion, List<ShopOffer> fresh, long rainbowCost);

    /// <summary>
    /// Locks or unlocks an unsold offer. Locking unlocks the other offer of the same kind in the shop.
    /// False — the offer is gone or already sold.
    /// </summary>
    Task<bool> SetLockedAsync(Guid playerId, Guid offerId, bool locked);

    /// <summary>
    /// Marks the offer sold, charges its price and gives the player the hero or the item, all in one transaction.
    /// </summary>
    Task<ShopSaveOutcome> BuyAsync(ShopOffer offer, Hero? hero, PlayerEquipment? equipment);
}

public enum ShopSaveOutcome
{
    Saved,
    // Магазин или товар успели изменить параллельным запросом (двойной клик)
    Changed,
    NotEnoughResources
}

// Чем игрок может платить в магазинах
public class ShopWallet
{
    public long Gold { get; set; }
    public long RainbowCoins { get; set; }
    public long BloodCoins { get; set; }
    public long HonorBadges { get; set; }
    // Камни 1-го уровня по виду и ступени (редкости)
    public List<ShopGemStack> Gems { get; set; } = new();
}

public class ShopGemStack
{
    public GemType Gem { get; set; }
    public Rarity Rarity { get; set; }
    public int Quantity { get; set; }
}
