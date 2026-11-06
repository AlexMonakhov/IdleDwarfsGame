// Магазины игрока. Хранятся строкой — значения не переименовывать без миграции:
//   Market — рынок в городе: золото, а за дворфов ещё и камни их стихии;
//   Pvp — подпольный рынок (вкладка рынка): кровавые монеты за бои на арене;
//   Guild — лавка гильдии: значки почёта за удары по боссам
public enum ShopKind { Market, Pvp, Guild }

public enum ShopOfferKind { Item, Hero }

// Состояние магазина игрока: когда он последний раз обновлялся сам и когда потрачено бесплатное обновление.
// Version — условные апдейты: из двух параллельных обновлений пройдёт только первое
public class ShopState
{
    public Guid PlayerId { get; set; }
    public ShopKind Shop { get; set; }
    // Сутки (00:00 UTC) последнего ежедневного обновления
    public DateTime RefreshedDay { get; set; }
    // Сутки, в которые потрачено бесплатное обновление; null — ещё ни разу
    public DateTime? FreeRerollDay { get; set; }
    public int Version { get; set; }
}

// Товар на полке: предмет из каталога (ItemKey) или уже сгенерированный дворф (HeroJson — снимок героя,
// при покупке он становится героем игрока). Цена фиксируется при генерации: способности дворфа на неё влияют
public class ShopOffer
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public ShopKind Shop { get; set; }
    public ShopOfferKind Kind { get; set; }
    // Место на полке 0..ShopRules.SlotCount-1
    public int Slot { get; set; }
    // Редкость предмета; у дворфа — по рангу (как в сундуках: ранг 1 — Common … ранг 4 — Mythic)
    public Rarity Rarity { get; set; }

    public string? ItemKey { get; set; }
    public string? HeroJson { get; set; }

    // Цена. Рынок: золото и, у дворфа со стихией, камни его стихии (GemRarity — ступень камня по рангу);
    // подпольный рынок и лавка гильдии — только своя валюта (Currency)
    public long PriceGold { get; set; }
    public long PriceCurrency { get; set; }
    public GemType? PriceGem { get; set; }
    public Rarity PriceGemRarity { get; set; }
    public int PriceGems { get; set; }

    // Закреплённый товар не меняется ни при ежедневном обновлении, ни при ручном. Не больше одного на вид товара
    public bool Locked { get; set; }
    public bool Sold { get; set; }
    public DateTime CreatedAt { get; set; }
}
