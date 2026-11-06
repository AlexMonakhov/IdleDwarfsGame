using Microsoft.EntityFrameworkCore;

public class ForgeRepository(GameDbContext context) : IForgeRepository
{
    // Камни — те же стопки, что у шахт, наград и магазина: 1-й уровень
    private const int GemLevel = 1;

    public async Task<List<PlayerEquipment>> GetFreeEquipmentAsync(Guid playerId)
    {
        return await context.PlayerEquipment
            .AsNoTracking()
            .Where(e => e.PlayerId == playerId && e.HeroId == null)
            .ToListAsync();
    }

    public async Task<List<ShopGemStack>> GetGemsAsync(Guid playerId)
    {
        var stacks = await context.PlayerItems
            .AsNoTracking()
            .Where(i => i.PlayerId == playerId && i.ItemType == ItemType.Gem && i.Level == GemLevel && i.Quantity > 0)
            .Select(i => new { i.SubType, i.Rarity, i.Quantity })
            .ToListAsync();

        // Стопка одного камня и ступени одна (уникальный индекс PlayerItems)
        return stacks
            .Where(s => Enum.TryParse<GemType>(s.SubType, out _))
            .Select(s => new ShopGemStack { Gem = Enum.Parse<GemType>(s.SubType), Rarity = s.Rarity, Quantity = s.Quantity })
            .ToList();
    }

    public async Task<bool> FuseItemsAsync(Guid playerId, IReadOnlyList<Guid> equipmentIds, PlayerEquipment result)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Удаляем только снятые предметы игрока: если хоть один успели надеть или уже сплавили — откатываем всё
        var ids = equipmentIds.ToList();
        int removed = await context.PlayerEquipment
            .Where(e => e.PlayerId == playerId && e.HeroId == null && ids.Contains(e.Id))
            .ExecuteDeleteAsync();
        if (removed != ids.Count)
            return false;

        context.PlayerEquipment.Add(result);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> FuseGemsAsync(Guid playerId, GemType gem, Rarity from, int cost, Rarity to, int created)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        string subType = gem.ToString();

        // Условное списание: двойной клик не уведёт стопку в минус
        int spent = await context.PlayerItems
            .Where(i => i.PlayerId == playerId && i.ItemType == ItemType.Gem && i.SubType == subType
                && i.Rarity == from && i.Level == GemLevel && i.Quantity >= cost)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity - cost));
        if (spent == 0)
            return false;

        int stacked = await context.PlayerItems
            .Where(i => i.PlayerId == playerId && i.ItemType == ItemType.Gem && i.SubType == subType
                && i.Rarity == to && i.Level == GemLevel)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity + created));
        if (stacked == 0)
        {
            // Камней этой ступени у игрока ещё не было — заводим стопку
            context.PlayerItems.Add(new PlayerItem
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                ItemType = ItemType.Gem,
                SubType = subType,
                Rarity = to,
                Level = GemLevel,
                Quantity = created
            });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        await transaction.CommitAsync();
        return true;
    }
}
