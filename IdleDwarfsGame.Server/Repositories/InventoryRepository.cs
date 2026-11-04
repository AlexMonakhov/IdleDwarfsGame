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

    public async Task<bool> OpenChestAsync(PlayerChest chest, IReadOnlyList<LootDrop> items, IReadOnlyList<Hero> heroes, IReadOnlyList<PlayerEquipment>? equipment = null)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт: из двух одновременных открытий одного сундука пройдёт только первое
        int opened = await context.PlayerChests
            .Where(c => c.Id == chest.Id && c.PlayerId == chest.PlayerId && !c.IsOpened)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsOpened, true));
        if (opened == 0)
            return false;

        foreach (var item in items)
            await PlayerItemStacks.AddAsync(context, chest.PlayerId, item);

        context.Heroes.AddRange(heroes);
        if (equipment != null)
            context.PlayerEquipment.AddRange(equipment);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
        return true;
    }
}
