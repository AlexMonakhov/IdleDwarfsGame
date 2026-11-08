using Microsoft.EntityFrameworkCore;

public class FixedShopRepository(GameDbContext context) : IFixedShopRepository
{
    public async Task<Dictionary<string, int>> GetPurchasesAsync(Guid playerId, FixedShopKind shop, DateTime period)
    {
        return await context.FixedShopPurchases
            .AsNoTracking()
            .Where(p => p.PlayerId == playerId && p.Shop == shop && p.Period == period)
            .ToDictionaryAsync(p => p.OfferKey, p => p.Count);
    }

    public async Task<PlayerEventWallet?> GetEventWalletAsync(Guid playerId, DateTime eventStart)
    {
        return await context.PlayerEventWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.PlayerId == playerId && w.EventStart == eventStart);
    }

    public async Task<FixedShopOutcome> BuyAsync(Guid playerId, FixedShopKind shop, DateTime period, FixedOffer offer, FixedShopGoods goods)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Лимит: условный апдейт счётчика, первая покупка — новая строка (ключ не даст завести её дважды)
        int counted = await context.FixedShopPurchases
            .Where(p => p.PlayerId == playerId && p.Shop == shop && p.Period == period && p.OfferKey == offer.Key && p.Count < offer.Limit)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Count, p => p.Count + 1));
        if (counted == 0)
        {
            bool exists = await context.FixedShopPurchases
                .AnyAsync(p => p.PlayerId == playerId && p.Shop == shop && p.Period == period && p.OfferKey == offer.Key);
            if (exists)
                return FixedShopOutcome.LimitReached;

            context.FixedShopPurchases.Add(new FixedShopPurchase { PlayerId = playerId, Shop = shop, Period = period, OfferKey = offer.Key, Count = 1 });
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                context.ChangeTracker.Clear();
                return FixedShopOutcome.Changed;
            }
            context.ChangeTracker.Clear();
        }

        long price = offer.Price;
        int paid = shop == FixedShopKind.Elder
            ? await context.Players
                .Where(p => p.Id == playerId && p.ElderSeals >= price)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ElderSeals, p => p.ElderSeals - price))
            : await context.PlayerEventWallets
                .Where(w => w.PlayerId == playerId && w.EventStart == period && w.Currency >= price)
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.Currency, w => w.Currency - price));
        if (paid == 0)
            return FixedShopOutcome.NotEnoughCurrency;

        if (goods.Reward != null)
            await RewardGrant.ApplyAsync(context, playerId, goods.Reward);
        if (goods.RainbowCoins > 0)
        {
            long coins = goods.RainbowCoins;
            await context.Players
                .Where(p => p.Id == playerId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.RainbowCoins, p => p.RainbowCoins + coins));
        }
        if (goods.Hero != null)
            context.Heroes.Add(goods.Hero);
        if (goods.Equipment != null)
            context.PlayerEquipment.Add(goods.Equipment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return FixedShopOutcome.Saved;
    }
}
