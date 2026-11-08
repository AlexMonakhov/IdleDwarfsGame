// Валюта игрока в одном ивенте. Ключ (PlayerId, EventStart): у каждого ивента своя, с концом недели она сгорает.
// Earned — сколько заработано за ивент всего (траты не уменьшают): для рейтинга и итогов
public class PlayerEventWallet
{
    public Guid PlayerId { get; set; }
    public DateTime EventStart { get; set; }
    public long Currency { get; set; }
    public long Earned { get; set; }
}

// Лавки с постоянным ассортиментом (FixedShops): товары и цены захардкожены, у товара лимит покупок на период.
// Хранится строкой — значения не переименовывать без миграции
//   Elder — лавка старейшин за печати ежедневных заданий, лимиты на неделю;
//   Event — лавка текущего ивента за его валюту, лимиты на ивент
public enum FixedShopKind { Elder, Event }

// Сколько раз игрок купил товар за период (неделя лавки старейшин или ивент). Ключ (PlayerId, Shop, Period, OfferKey)
public class FixedShopPurchase
{
    public Guid PlayerId { get; set; }
    public FixedShopKind Shop { get; set; }
    public DateTime Period { get; set; }
    public string OfferKey { get; set; } = string.Empty;
    public int Count { get; set; }
}
