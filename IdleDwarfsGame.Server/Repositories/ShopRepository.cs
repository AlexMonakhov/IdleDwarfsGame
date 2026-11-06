using Microsoft.EntityFrameworkCore;

public class ShopRepository(GameDbContext context) : IShopRepository
{
    // Камни магазинных цен — те же стопки, что у шахт и наград: 1-й уровень
    private const int GemLevel = 1;

    public async Task<ShopState?> GetStateAsync(Guid playerId, ShopKind shop)
    {
        return await context.ShopStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.Shop == shop);
    }

    public async Task<List<ShopOffer>> GetOffersAsync(Guid playerId, ShopKind shop)
    {
        return await context.ShopOffers
            .AsNoTracking()
            .Where(o => o.PlayerId == playerId && o.Shop == shop)
            .OrderBy(o => o.Kind)
            .ThenBy(o => o.Slot)
            .ToListAsync();
    }

    public async Task<ShopOffer?> GetOfferAsync(Guid playerId, Guid offerId)
    {
        return await context.ShopOffers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offerId && o.PlayerId == playerId);
    }

    public async Task<ShopWallet> GetWalletAsync(Guid playerId)
    {
        // Без трекинга: ресурсы меняются через ExecuteUpdate, отслеживаемые копии могли устареть
        var wallet = await context.Players
            .AsNoTracking()
            .Where(p => p.Id == playerId)
            .Select(p => new ShopWallet { Gold = p.Gold, RainbowCoins = p.RainbowCoins, BloodCoins = p.BloodCoins, HonorBadges = p.HonorBadges })
            .FirstOrDefaultAsync() ?? new ShopWallet();

        var stacks = await context.PlayerItems
            .AsNoTracking()
            .Where(i => i.PlayerId == playerId && i.ItemType == ItemType.Gem && i.Level == GemLevel && i.Quantity > 0)
            .Select(i => new { i.SubType, i.Rarity, i.Quantity })
            .ToListAsync();

        wallet.Gems = stacks
            .Where(s => Enum.TryParse<GemType>(s.SubType, out _))
            .Select(s => new ShopGemStack { Gem = Enum.Parse<GemType>(s.SubType), Rarity = s.Rarity, Quantity = s.Quantity })
            .ToList();
        return wallet;
    }

    public async Task<ShopSaveOutcome> RefreshAsync(ShopState state, int? expectedVersion, List<ShopOffer> fresh, long rainbowCost)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if (expectedVersion is { } version)
        {
            // Условный апдейт: из двух параллельных обновлений пройдёт только первое
            int updated = await context.ShopStates
                .Where(s => s.PlayerId == state.PlayerId && s.Shop == state.Shop && s.Version == version)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.RefreshedDay, state.RefreshedDay)
                    .SetProperty(x => x.FreeRerollDay, state.FreeRerollDay)
                    .SetProperty(x => x.Version, version + 1));
            if (updated == 0)
                return ShopSaveOutcome.Changed;
            state.Version = version + 1;
        }
        else
        {
            // Первый заход в магазин: ключ PlayerId + Shop не даст завести состояние дважды
            context.ShopStates.Add(state);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                context.ChangeTracker.Clear();
                return ShopSaveOutcome.Changed;
            }
            context.Entry(state).State = EntityState.Detached;
        }

        if (rainbowCost > 0)
        {
            int paid = await context.Players
                .Where(p => p.Id == state.PlayerId && p.RainbowCoins >= rainbowCost)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.RainbowCoins, p => p.RainbowCoins - rainbowCost));
            if (paid == 0)
                return ShopSaveOutcome.NotEnoughResources;
        }

        // Закреплённые товары остаются, остальные заменяются свежими
        await context.ShopOffers
            .Where(o => o.PlayerId == state.PlayerId && o.Shop == state.Shop && !o.Locked)
            .ExecuteDeleteAsync();
        context.ShopOffers.AddRange(fresh);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return ShopSaveOutcome.Saved;
    }

    public async Task<bool> SetLockedAsync(Guid playerId, Guid offerId, bool locked)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var offer = await context.ShopOffers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offerId && o.PlayerId == playerId && !o.Sold);
        if (offer == null)
            return false;

        // Закреплённым бывает только один товар каждого вида
        if (locked)
        {
            await context.ShopOffers
                .Where(o => o.PlayerId == playerId && o.Shop == offer.Shop && o.Kind == offer.Kind && o.Id != offerId && o.Locked)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Locked, false));
        }

        int updated = await context.ShopOffers
            .Where(o => o.Id == offerId && !o.Sold)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Locked, locked));
        if (updated == 0)
            return false;

        await transaction.CommitAsync();
        return true;
    }

    public async Task<ShopSaveOutcome> BuyAsync(ShopOffer offer, Hero? hero, PlayerEquipment? equipment)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Проданный товар снимается с закрепления: место освободится при следующем обновлении
        int sold = await context.ShopOffers
            .Where(o => o.Id == offer.Id && o.PlayerId == offer.PlayerId && !o.Sold)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Sold, true)
                .SetProperty(o => o.Locked, false));
        if (sold == 0)
            return ShopSaveOutcome.Changed;

        long gold = offer.PriceGold;
        long blood = offer.Shop == ShopKind.Pvp ? offer.PriceCurrency : 0;
        long badges = offer.Shop == ShopKind.Guild ? offer.PriceCurrency : 0;
        int paid = await context.Players
            .Where(p => p.Id == offer.PlayerId && p.Gold >= gold && p.BloodCoins >= blood && p.HonorBadges >= badges)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Gold, p => p.Gold - gold)
                .SetProperty(p => p.BloodCoins, p => p.BloodCoins - blood)
                .SetProperty(p => p.HonorBadges, p => p.HonorBadges - badges));
        if (paid == 0)
            return ShopSaveOutcome.NotEnoughResources;

        if (offer.PriceGem is { } gem && offer.PriceGems > 0)
        {
            string subType = gem.ToString();
            int gems = offer.PriceGems;
            int spent = await context.PlayerItems
                .Where(i => i.PlayerId == offer.PlayerId && i.ItemType == ItemType.Gem && i.SubType == subType
                    && i.Rarity == offer.PriceGemRarity && i.Level == GemLevel && i.Quantity >= gems)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity - gems));
            if (spent == 0)
                return ShopSaveOutcome.NotEnoughResources;
        }

        if (hero != null)
            context.Heroes.Add(hero);
        if (equipment != null)
            context.PlayerEquipment.Add(equipment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return ShopSaveOutcome.Saved;
    }
}
