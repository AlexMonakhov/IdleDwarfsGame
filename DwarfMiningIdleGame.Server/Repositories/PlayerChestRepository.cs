using DwarfMiningIdleGame.Server.Entities.GameEntities;
using DwarfMiningIdleGame.Server.Enums;
using Microsoft.EntityFrameworkCore;

public class PlayerChestRepository : IPlayerChestRepository
{
    private readonly GameDbContext _context;

    public PlayerChestRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task<PlayerChest?> GetPlayerChestAsync(Guid playerId, Guid chestId)
    {
        return await _context.PlayerChests
            .Include(pc => pc.ChestDropTable)
            .FirstOrDefaultAsync(pc => pc.PlayerId == playerId && pc.Id == chestId && !pc.IsOpened);
    }

    public async Task<bool> PlayerHasChestAsync(Guid playerId, Guid chestId)
    {
        return await _context.PlayerChests.AnyAsync(pc => pc.PlayerId == playerId && pc.Id == chestId && !pc.IsOpened);
    }

    public async Task MarkChestAsOpenedAsync(PlayerChest chest)
    {
        // Only update the IsOpened property without tracking navigation properties
        _context.PlayerChests.Update(chest);
        _context.Entry(chest).Property(c => c.IsOpened).IsModified = true;
        await _context.SaveChangesAsync();
    }

    public async Task<IChestDropTable?> GetChestDropTableByLevelAndTypeAsync(int chestLevel, ChestType chestType)
    {
        return await _context.ChestDropTables
            .Include(t => t.Entries)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ChestLevel == chestLevel && t.ChestType == chestType);
    }

    public async Task AddChestToPlayerAsync(PlayerChest chest)
    {
        // Check if player exists by fetching the player to ensure it's in the database
        var player = _context.Players.FirstOrDefault(p => p.Id == chest.PlayerId);
        
        var chestDropTable = _context.ChestDropTables.FirstOrDefault(t => t.Id == chest.ChestDropTableId);
        var a = Convert.ToHexString(player.Id.ToByteArray());
        var b = Convert.ToHexString(chest.PlayerId.ToByteArray());
        if (player == null)
        {
            var allPlayers = await _context.Players.AsNoTracking().Select(p => p.Id).ToListAsync();
            throw new InvalidOperationException(
                $"Player with ID {chest.PlayerId} not found. Available players: {string.Join(", ", allPlayers)}");
        }
        
        if (chestDropTable == null)
        {
            var allTables = await _context.ChestDropTables.AsNoTracking().Select(t => t.Id).ToListAsync();
            throw new InvalidOperationException(
                $"ChestDropTable with ID {chest.ChestDropTableId} not found. Available tables: {string.Join(", ", allTables)}");
        }
        
        _context.PlayerChests.Add(chest);
        _context.SaveChanges();
    }
}