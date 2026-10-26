using IdleDwarfsGame.Server.Entities.GameEntities;
using Microsoft.EntityFrameworkCore;

public class InventoryRepository(GameDbContext context) : IInventoryRepository
{
    private IQueryable<PlayerChest> ClosedChests(Guid playerId) => context.PlayerChests
        .AsNoTracking()
        .Include(c => c.ChestDropTable)
        .Where(c => c.PlayerId == playerId && !c.IsOpened);

    public async Task<List<PlayerChest>> GetClosedChestsAsync(Guid playerId)
    {
        var chests = await ClosedChests(playerId).ToListAsync();
        return chests
            .OrderBy(c => c.ChestDropTable.ChestType)
            .ThenByDescending(c => c.ChestDropTable.ChestLevel)
            .ToList();
    }

    public async Task<PlayerChest?> GetClosedChestAsync(Guid playerId, Guid chestId)
    {
        return await ClosedChests(playerId).FirstOrDefaultAsync(c => c.Id == chestId);
    }

    public async Task<List<PlayerItem>> GetItemsAsync(Guid playerId)
    {
        return await context.PlayerItems
            .AsNoTracking()
            .Where(i => i.PlayerId == playerId && i.Quantity > 0)
            .ToListAsync();
    }

    public async Task<bool> OpenChestAsync(PlayerChest chest, IReadOnlyList<LootDrop> items, IReadOnlyList<Hero> heroes)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт: из двух одновременных открытий одного сундука пройдёт только первое
        int opened = await context.PlayerChests
            .Where(c => c.Id == chest.Id && c.PlayerId == chest.PlayerId && !c.IsOpened)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsOpened, true));
        if (opened == 0)
            return false;

        foreach (var item in items)
        {
            // Такие же предметы уже есть — прибавляем к стопке, иначе заводим новую
            int stacked = await context.PlayerItems
                .Where(i => i.PlayerId == chest.PlayerId && i.ItemType == item.ItemType && i.SubType == item.SubType
                    && i.Rarity == item.Rarity && i.Level == item.Level)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity + item.Amount));
            if (stacked == 0)
            {
                context.PlayerItems.Add(new PlayerItem
                {
                    Id = Guid.NewGuid(),
                    PlayerId = chest.PlayerId,
                    ItemType = item.ItemType,
                    SubType = item.SubType,
                    Rarity = item.Rarity,
                    Level = item.Level,
                    Quantity = item.Amount
                });
            }
        }

        context.Heroes.AddRange(heroes);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
        return true;
    }
}
