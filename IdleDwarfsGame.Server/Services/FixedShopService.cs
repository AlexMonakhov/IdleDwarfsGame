// Причина отказа — фронт показывает по коду переведённый текст (fixedShop.errors.<code>)
public enum FixedShopError
{
    UnknownShop,
    OfferNotFound,
    LimitReached,
    NotEnoughCurrency,
    // Лавка ивента закрыта (такого сейчас не бывает: неделя ивента всегда заканчивается началом следующего)
    ShopClosed,
    Changed
}

public class FixedShopException(FixedShopError code, string message) : InvalidOperationException(message)
{
    public FixedShopError Code { get; } = code;
}

// Лавки с постоянным ассортиментом (правила — FixedShops): лавка старейшин и лавка текущего ивента
public class FixedShopService(
    IFixedShopRepository shops,
    IPlayerRepository players,
    IEntityFactory entityFactory,
    TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<FixedShopOverview> GetAsync(Guid playerId, FixedShopKind shop)
    {
        EnsureKnown(shop);
        return await DescribeAsync(playerId, shop);
    }

    public async Task<FixedShopBuyResult> BuyAsync(Guid playerId, FixedShopKind shop, string offerKey)
    {
        EnsureKnown(shop);
        var now = Now;
        var window = WorldEvents.Of(now);
        if (shop == FixedShopKind.Event && !window.IsShopOpen(now))
            throw new FixedShopException(FixedShopError.ShopClosed, "The event shop is closed");

        var offer = FixedShops.CatalogOf(shop, window).FirstOrDefault(o => o.Key == offerKey)
            ?? throw new FixedShopException(FixedShopError.OfferNotFound, $"Offer '{offerKey}' not found");

        var progress = await players.GetProgressAsync(playerId);
        var goods = GoodsOf(offer, playerId, progress, now);

        switch (await shops.BuyAsync(playerId, shop, FixedShops.PeriodOf(now), offer, goods))
        {
            case FixedShopOutcome.LimitReached:
                throw new FixedShopException(FixedShopError.LimitReached, "This offer is sold out for now");
            case FixedShopOutcome.NotEnoughCurrency:
                throw new FixedShopException(FixedShopError.NotEnoughCurrency, $"Not enough currency: {offer.Price} needed");
            case FixedShopOutcome.Changed:
                throw new FixedShopException(FixedShopError.Changed, "The shop has just changed, try again");
        }

        return new FixedShopBuyResult
        {
            Shop = await DescribeAsync(playerId, shop),
            Hero = goods.Hero == null ? null : HeroInfo.From(entityFactory, goods.Hero),
            Item = goods.Equipment == null ? null : EquipmentInfo.From(goods.Equipment),
            Reward = goods.Reward == null ? null : RewardInfo.From(goods.Reward),
            RainbowCoins = goods.RainbowCoins
        };
    }

    // Адрес /fixed-shops/{shop} принимает и число — (FixedShopKind)7 не лавка
    private static void EnsureKnown(FixedShopKind shop)
    {
        if (!Enum.IsDefined(shop))
            throw new FixedShopException(FixedShopError.UnknownShop, $"Unknown shop '{shop}'");
    }

    private static FixedShopGoods GoodsOf(FixedOffer offer, Guid playerId, PlayerProgress progress, DateTime now)
    {
        switch (offer.Kind)
        {
            case FixedRewardKind.Hero:
                return new FixedShopGoods { Hero = FixedShops.CreateHero(offer.Hero!, playerId) };
            case FixedRewardKind.Item:
                return new FixedShopGoods
                {
                    Equipment = new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = playerId, ItemKey = offer.ItemKey! }
                };
            case FixedRewardKind.RainbowCoins:
                return new FixedShopGoods { RainbowCoins = offer.Amount };
            default:
                var rewardId = Guid.NewGuid();
                int chestLevel = DailyQuests.ChestLevelFor(progress);
                return new FixedShopGoods
                {
                    Reward = new PlayerReward
                    {
                        Id = rewardId,
                        PlayerId = playerId,
                        Gold = DailyQuests.GoldFor(offer.GoldStages, progress),
                        Chests = offer.Chest is { } chest
                            ? Enumerable.Range(0, offer.Amount)
                                .Select(_ => new PlayerRewardChest { Id = Guid.NewGuid(), RewardId = rewardId, ChestType = chest, ChestLevel = chestLevel })
                                .ToList()
                            : [],
                        CreatedAt = now
                    }
                };
        }
    }

    private async Task<FixedShopOverview> DescribeAsync(Guid playerId, FixedShopKind shop)
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        var period = FixedShops.PeriodOf(now);
        var bought = await shops.GetPurchasesAsync(playerId, shop, period);
        var progress = await players.GetProgressAsync(playerId);
        var resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources();
        var wallet = shop == FixedShopKind.Event ? await shops.GetEventWalletAsync(playerId, window.StartsAt) : null;
        int chestLevel = DailyQuests.ChestLevelFor(progress);

        return new FixedShopOverview
        {
            Shop = shop,
            Offers = FixedShops.CatalogOf(shop, window).Select(offer => new FixedOfferInfo
            {
                Key = offer.Key,
                Kind = offer.Kind,
                Rarity = FixedShops.RarityOf(offer),
                Price = offer.Price,
                Limit = offer.Limit,
                Bought = bought.GetValueOrDefault(offer.Key),
                // Превью героя — тот же герой, что выдаст покупка (seed постоянный)
                Hero = offer.Hero == null ? null : HeroInfo.From(entityFactory, FixedShops.CreateHero(offer.Hero, playerId)),
                Item = offer.ItemKey == null ? null : EquipmentInfo.From(new PlayerEquipment { Id = Guid.Empty, ItemKey = offer.ItemKey }),
                Chest = offer.Chest is { } chest ? new RewardChestInfo { Type = chest, Level = chestLevel } : null,
                Gold = DailyQuests.GoldFor(offer.GoldStages, progress),
                RainbowCoins = offer.Kind == FixedRewardKind.RainbowCoins ? offer.Amount : 0
            }).ToList(),
            Currency = shop == FixedShopKind.Elder ? resources.ElderSeals : wallet?.Currency ?? 0,
            ResetAt = window.ShopClosesAt,
            ServerTime = now,
            Event = shop == FixedShopKind.Event ? WorldEventInfo.From(window, now, wallet) : null,
            Resources = resources
        };
    }
}
