// Полка магазина (ShopService): товары, чем платить и когда обновится
public class ShopOverview
{
    public ShopKind Shop { get; set; }
    public List<ShopOfferInfo> Items { get; set; } = new();
    public List<ShopOfferInfo> Heroes { get; set; } = new();
    public ShopWallet Wallet { get; set; } = new();
    // Те же ресурсы, что в шапке: фронт обновляет её после покупки и обновления за радужные монеты
    public PlayerResources Resources { get; set; } = new();
    // Бесплатное обновление сегодня ещё не потрачено; иначе обновление стоит RerollCost радужных монет
    public bool FreeReroll { get; set; }
    public int RerollCost { get; set; }
    // Когда полка обновится сама (00:00 UTC следующих суток) и время сервера — для таймера
    public DateTime RefreshAt { get; set; }
    public DateTime ServerTime { get; set; }
}

// Товар: Item — предмет (карточка как в инвентаре), Hero — дворф (карточка как у героя)
public class ShopOfferInfo
{
    public Guid Id { get; set; }
    public ShopOfferKind Kind { get; set; }
    public int Slot { get; set; }
    public Rarity Rarity { get; set; }
    public bool Locked { get; set; }
    public bool Sold { get; set; }
    public ShopPrice Price { get; set; } = new();
    public EquipmentInfo? Item { get; set; }
    public HeroInfo? Hero { get; set; }
}

public class ShopBuyResult
{
    public ShopOverview Shop { get; set; } = new();
    // Что досталось игроку: новый герой или предмет
    public HeroInfo? Hero { get; set; }
    public EquipmentInfo? Item { get; set; }
}

public class ShopLockRequest
{
    public bool Locked { get; set; }
}
