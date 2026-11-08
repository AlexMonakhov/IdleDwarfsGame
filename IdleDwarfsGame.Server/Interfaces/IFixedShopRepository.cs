public interface IFixedShopRepository
{
    // Сколько раз куплен каждый товар лавки за период
    Task<Dictionary<string, int>> GetPurchasesAsync(Guid playerId, FixedShopKind shop, DateTime period);

    Task<PlayerEventWallet?> GetEventWalletAsync(Guid playerId, DateTime eventStart);

    /// <summary>
    /// Counts the purchase against the offer limit, charges the price (elder seals, or the currency of the event
    /// that started at <paramref name="period"/>) and grants the goods, all in one transaction.
    /// </summary>
    Task<FixedShopOutcome> BuyAsync(Guid playerId, FixedShopKind shop, DateTime period, FixedOffer offer, FixedShopGoods goods);
}

// Что получит игрок: ровно одно из полей, по виду товара
public class FixedShopGoods
{
    public Hero? Hero { get; set; }
    public PlayerEquipment? Equipment { get; set; }
    // Золото и сундуки
    public PlayerReward? Reward { get; set; }
    public long RainbowCoins { get; set; }
}

public enum FixedShopOutcome
{
    Saved,
    LimitReached,
    NotEnoughCurrency,
    // Параллельная первая покупка товара — повторить
    Changed
}
