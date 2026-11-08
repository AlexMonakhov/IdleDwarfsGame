using System.Text.Json;
using System.Text.Json.Serialization;

// Причина отказа — фронт показывает по коду переведённый текст (shop.errors.<code>)
public enum ShopError
{
    // Такого магазина нет (например, число вместо имени в адресе)
    UnknownShop,
    OfferNotFound,
    AlreadySold,
    NotEnoughGold,
    NotEnoughGems,
    NotEnoughCurrency,
    NotEnoughRainbowCoins,
    // Магазин обновили параллельным запросом (двойной клик) — фронт перечитает полку
    Changed
}

public class ShopException(ShopError code, string message) : InvalidOperationException(message)
{
    public ShopError Code { get; } = code;
}

// Магазины игрока (правила и цены — ShopRules). Полка создаётся при первом заходе и обновляется сама,
// когда игрок заходит в новые сутки (00:00 UTC): все незакреплённые товары, в том числе проданные, заменяются свежими.
// Ручное обновление — то же самое по кнопке: раз в сутки бесплатно, дальше за радужные монеты
public class ShopService(
    IShopRepository shops,
    IPlayerRepository players,
    IEntityFactory entityFactory,
    Random random,
    TimeProvider clock,
    IActivityTracker activity)
{
    private static readonly JsonSerializerOptions HeroJson = new() { Converters = { new JsonStringEnumConverter() } };

    // Полку могли обновить параллельно (две вкладки) — тогда перечитываем и смотрим ещё раз
    private const int RefreshAttempts = 3;

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public static DateTime DayOf(DateTime now) => now.Date;

    public async Task<ShopOverview> GetAsync(Guid playerId, ShopKind shop)
    {
        EnsureKnown(shop);
        await EnsureFreshAsync(playerId, shop);
        return await DescribeAsync(playerId, shop);
    }

    public async Task<ShopOverview> RerollAsync(Guid playerId, ShopKind shop)
    {
        EnsureKnown(shop);
        await EnsureFreshAsync(playerId, shop);
        var today = DayOf(Now);
        var state = await shops.GetStateAsync(playerId, shop)
            ?? throw new ShopException(ShopError.Changed, "The shop has just changed, try again");

        bool free = state.FreeRerollDay != today;
        var next = new ShopState
        {
            PlayerId = playerId,
            Shop = shop,
            RefreshedDay = state.RefreshedDay,
            FreeRerollDay = free ? today : state.FreeRerollDay
        };
        var fresh = Generate(playerId, shop, await shops.GetOffersAsync(playerId, shop));

        switch (await shops.RefreshAsync(next, state.Version, fresh, free ? 0 : ShopRules.RerollCost))
        {
            case ShopSaveOutcome.Changed:
                throw new ShopException(ShopError.Changed, "The shop has just changed, try again");
            case ShopSaveOutcome.NotEnoughResources:
                throw new ShopException(ShopError.NotEnoughRainbowCoins, $"A reroll costs {ShopRules.RerollCost} rainbow coins");
        }
        return await DescribeAsync(playerId, shop);
    }

    public async Task<ShopOverview> SetLockedAsync(Guid playerId, Guid offerId, bool locked)
    {
        var offer = await shops.GetOfferAsync(playerId, offerId)
            ?? throw new ShopException(ShopError.OfferNotFound, "Offer not found");
        if (offer.Sold || !await shops.SetLockedAsync(playerId, offerId, locked))
            throw new ShopException(ShopError.AlreadySold, "The offer is already sold");

        return await DescribeAsync(playerId, offer.Shop);
    }

    public async Task<ShopBuyResult> BuyAsync(Guid playerId, Guid offerId)
    {
        var offer = await shops.GetOfferAsync(playerId, offerId)
            ?? throw new ShopException(ShopError.OfferNotFound, "Offer not found");
        if (offer.Sold)
            throw new ShopException(ShopError.AlreadySold, "The offer is already sold");

        EnsureAffordable(await shops.GetWalletAsync(playerId), offer);

        Hero? hero = null;
        PlayerEquipment? equipment = null;
        if (offer.Kind == ShopOfferKind.Hero)
        {
            hero = HeroOf(offer) ?? throw new ShopException(ShopError.OfferNotFound, "Offer not found");
            // Купленный дворф — новый герой игрока, в инвентаре, вне отряда
            hero.Id = Guid.NewGuid();
            hero.PlayerId = playerId;
            hero.PlaceInSquad = null;
            hero.Equipment = new();
        }
        else
        {
            equipment = new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = playerId, ItemKey = offer.ItemKey ?? string.Empty };
        }

        switch (await shops.BuyAsync(offer, hero, equipment))
        {
            case ShopSaveOutcome.Changed:
                throw new ShopException(ShopError.AlreadySold, "The offer is already sold");
            case ShopSaveOutcome.NotEnoughResources:
                // Ресурсы потратили параллельным запросом — перечитываем, чтобы назвать, чего не хватило
                EnsureAffordable(await shops.GetWalletAsync(playerId), offer);
                throw new ShopException(ShopError.NotEnoughGold, "Not enough resources");
        }
        await activity.RecordAsync(playerId, ActivityKind.ShopBuy);

        return new ShopBuyResult
        {
            Shop = await DescribeAsync(playerId, offer.Shop),
            Hero = hero == null ? null : HeroInfo.From(entityFactory, hero),
            Item = equipment == null ? null : EquipmentInfo.From(equipment)
        };
    }

    // Адрес /shop/{shop} принимает и число: (ShopKind)5 — не магазин. Без проверки у такой полки не было бы валюты,
    // и товары с неё покупались бы даром
    private static void EnsureKnown(ShopKind shop)
    {
        if (!Enum.IsDefined(shop))
            throw new ShopException(ShopError.UnknownShop, $"Unknown shop '{shop}'");
    }

    /* ===== Полка ===== */

    // Магазин без состояния или со вчерашней полкой — генерируем свежую (закреплённые товары остаются)
    private async Task EnsureFreshAsync(Guid playerId, ShopKind shop)
    {
        for (int attempt = 0; attempt < RefreshAttempts; attempt++)
        {
            var today = DayOf(Now);
            var state = await shops.GetStateAsync(playerId, shop);
            if (state != null && state.RefreshedDay >= today)
                return;

            var next = new ShopState
            {
                PlayerId = playerId,
                Shop = shop,
                RefreshedDay = today,
                FreeRerollDay = state?.FreeRerollDay
            };
            var fresh = Generate(playerId, shop, await shops.GetOffersAsync(playerId, shop));
            if (await shops.RefreshAsync(next, state?.Version, fresh, 0) == ShopSaveOutcome.Saved)
                return;
        }
    }

    // Новые товары на все места, кроме занятых закреплёнными
    private List<ShopOffer> Generate(Guid playerId, ShopKind shop, IEnumerable<ShopOffer> current)
    {
        var locked = current.Where(o => o.Locked && !o.Sold).ToList();
        var now = Now;
        var fresh = new List<ShopOffer>();

        foreach (var kind in new[] { ShopOfferKind.Item, ShopOfferKind.Hero })
        {
            var taken = locked.Where(o => o.Kind == kind).Select(o => o.Slot).ToHashSet();
            for (int slot = 0; slot < ShopRules.SlotCount; slot++)
            {
                if (taken.Contains(slot))
                    continue;
                fresh.Add(kind == ShopOfferKind.Item ? ItemOffer(playerId, shop, slot, now) : HeroOffer(playerId, shop, slot, now));
            }
        }
        return fresh;
    }

    private ShopOffer ItemOffer(Guid playerId, ShopKind shop, int slot, DateTime now)
    {
        var item = ShopRules.RollItem(random);
        return WithPrice(new ShopOffer
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Shop = shop,
            Kind = ShopOfferKind.Item,
            Slot = slot,
            Rarity = item.Rarity,
            ItemKey = item.Key,
            CreatedAt = now
        }, ShopRules.PriceOf(shop, item));
    }

    private ShopOffer HeroOffer(Guid playerId, ShopKind shop, int slot, DateTime now)
    {
        var hero = ShopRules.RollHero(shop, playerId, random);
        return WithPrice(new ShopOffer
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Shop = shop,
            Kind = ShopOfferKind.Hero,
            Slot = slot,
            Rarity = ShopRules.RarityOf(hero.Level),
            HeroJson = JsonSerializer.Serialize(hero, HeroJson),
            CreatedAt = now
        }, ShopRules.PriceOf(shop, hero));
    }

    private static ShopOffer WithPrice(ShopOffer offer, ShopPrice price)
    {
        offer.PriceGold = price.Gold;
        offer.PriceCurrency = price.Currency;
        offer.PriceGem = price.Gem;
        offer.PriceGemRarity = price.GemRarity;
        offer.PriceGems = price.Gems;
        return offer;
    }

    private static Hero? HeroOf(ShopOffer offer) =>
        offer.HeroJson == null ? null : JsonSerializer.Deserialize<Hero>(offer.HeroJson, HeroJson);

    /* ===== Ответ ===== */

    private async Task<ShopOverview> DescribeAsync(Guid playerId, ShopKind shop)
    {
        var now = Now;
        var today = DayOf(now);
        var state = await shops.GetStateAsync(playerId, shop);
        var offers = await shops.GetOffersAsync(playerId, shop);

        return new ShopOverview
        {
            Shop = shop,
            Items = offers.Where(o => o.Kind == ShopOfferKind.Item).Select(Describe).ToList(),
            Heroes = offers.Where(o => o.Kind == ShopOfferKind.Hero).Select(Describe).ToList(),
            Wallet = await shops.GetWalletAsync(playerId),
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources(),
            FreeReroll = state?.FreeRerollDay != today,
            RerollCost = ShopRules.RerollCost,
            RefreshAt = today.AddDays(1),
            ServerTime = now
        };
    }

    private ShopOfferInfo Describe(ShopOffer offer)
    {
        var hero = offer.Kind == ShopOfferKind.Hero ? HeroOf(offer) : null;
        return new ShopOfferInfo
        {
            Id = offer.Id,
            Kind = offer.Kind,
            Slot = offer.Slot,
            Rarity = offer.Rarity,
            Locked = offer.Locked,
            Sold = offer.Sold,
            Price = new ShopPrice
            {
                Gold = offer.PriceGold,
                Currency = offer.PriceCurrency,
                Gem = offer.PriceGem,
                GemRarity = offer.PriceGemRarity,
                Gems = offer.PriceGems
            },
            // У товара на полке своего id у предмета нет — карточка ключуется id товара
            Item = offer.ItemKey == null ? null : EquipmentInfo.From(new PlayerEquipment { Id = offer.Id, ItemKey = offer.ItemKey }),
            Hero = hero == null ? null : HeroInfo.From(entityFactory, hero)
        };
    }

    private static void EnsureAffordable(ShopWallet wallet, ShopOffer offer)
    {
        if (wallet.Gold < offer.PriceGold)
            throw new ShopException(ShopError.NotEnoughGold, $"Not enough gold: {wallet.Gold} of {offer.PriceGold}");

        long currency = offer.Shop switch
        {
            ShopKind.Pvp => wallet.BloodCoins,
            ShopKind.Guild => wallet.HonorBadges,
            ShopKind.Market => long.MaxValue,
            _ => throw new ShopException(ShopError.UnknownShop, $"Unknown shop '{offer.Shop}'")
        };
        if (currency < offer.PriceCurrency)
            throw new ShopException(ShopError.NotEnoughCurrency, $"Not enough currency: {currency} of {offer.PriceCurrency}");

        if (offer.PriceGem is { } gem && offer.PriceGems > 0)
        {
            int gems = wallet.Gems.Where(g => g.Gem == gem && g.Rarity == offer.PriceGemRarity).Sum(g => g.Quantity);
            if (gems < offer.PriceGems)
                throw new ShopException(ShopError.NotEnoughGems, $"Not enough {offer.PriceGemRarity} {gem}: {gems} of {offer.PriceGems}");
        }
    }
}
