public class FixedOfferInfo
{
    public string Key { get; set; } = string.Empty;
    public FixedRewardKind Kind { get; set; }
    public Rarity Rarity { get; set; }
    public long Price { get; set; }
    public int Limit { get; set; }
    // Куплено за период; Limit - Bought — сколько ещё можно
    public int Bought { get; set; }

    // Что внутри — по виду товара: герой, предмет, сундук (уровня текущей главы), золото или радужные монеты
    public HeroInfo? Hero { get; set; }
    public EquipmentInfo? Item { get; set; }
    public RewardChestInfo? Chest { get; set; }
    public int Gold { get; set; }
    public long RainbowCoins { get; set; }
}

public class WorldEventInfo
{
    public WorldEventKind Kind { get; set; }
    public DateTime StartsAt { get; set; }
    // Конец активной части: дальше валюта не копится, работает только лавка
    public DateTime EndsAt { get; set; }
    // Конец недели: лавка закрывается, остаток валюты сгорает, начинается следующий ивент
    public DateTime ShopClosesAt { get; set; }
    public bool Active { get; set; }
    public WorldEventKind NextKind { get; set; }

    // Валюта игрока в этом ивенте: остаток и сколько заработано всего
    public long Currency { get; set; }
    public long Earned { get; set; }

    public static WorldEventInfo From(WorldEventWindow window, DateTime now, PlayerEventWallet? wallet) => new()
    {
        Kind = window.Kind,
        StartsAt = window.StartsAt,
        EndsAt = window.EndsAt,
        ShopClosesAt = window.ShopClosesAt,
        Active = window.IsActive(now),
        NextKind = WorldEvents.Next(now).Kind,
        Currency = wallet?.Currency ?? 0,
        Earned = wallet?.Earned ?? 0
    };
}

public class FixedShopOverview
{
    public FixedShopKind Shop { get; set; }
    public List<FixedOfferInfo> Offers { get; set; } = new();
    // Валюта этой лавки: печати старейшин или валюта ивента
    public long Currency { get; set; }
    // Когда сбросятся лимиты (и у лавки ивента — сгорит валюта)
    public DateTime ResetAt { get; set; }
    public DateTime ServerTime { get; set; }
    // Ивент недели — только у лавки ивента
    public WorldEventInfo? Event { get; set; }
    public PlayerResources Resources { get; set; } = new();
}

public class FixedShopBuyResult
{
    public FixedShopOverview Shop { get; set; } = new();
    public HeroInfo? Hero { get; set; }
    public EquipmentInfo? Item { get; set; }
    public RewardInfo? Reward { get; set; }
    public long RainbowCoins { get; set; }
}
