using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayerRepository : IPlayerRepository
{
    private readonly GameDbContext _context;

    public PlayerRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        return await _context.Players.FindAsync(id);
    }

    public async Task<List<Player>> GetAllAsync()
    {
        return await _context.Players.ToListAsync();
    }

    public async Task AddAsync(Player player)
    {
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Player player)
    {
        _context.Players.Update(player);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
            return;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Ссылки на аккаунт с «другой стороны» каскадом не удаляются (два каскадных пути SQL Server не допускает) — чистим сами.
        // Остальное (сам аккаунт, его заявки, дружба, бои и т.д.) удаляет каскад от игрока
        var userIds = _context.Users.Where(u => u.PlayerId == id).Select(u => u.Id);
        await _context.FriendRequests.Where(r => userIds.Contains(r.ToUserId)).ExecuteDeleteAsync();
        await _context.Friendships.Where(f => userIds.Contains(f.FriendId)).ExecuteDeleteAsync();
        await _context.PvpBattles.Where(b => userIds.Contains(b.DefenderUserId)).ExecuteDeleteAsync();

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<List<Hero>> GetHeroes(Guid playerId)
    {
        return await _context.Heroes
            .Where(h => h.PlayerId == playerId)
            .ToListAsync();
    }

    public async Task<PlayerResources?> GetResourcesAsync(Guid playerId)
    {
        // Без трекинга: ресурсы меняются через ExecuteUpdate, отслеживаемая копия игрока могла устареть
        return await _context.Players
            .AsNoTracking()
            .Where(p => p.Id == playerId)
            .Select(p => new PlayerResources
            {
                Gold = p.Gold,
                Experience = p.Experience,
                RainbowCoins = p.RainbowCoins,
                BloodCoins = p.BloodCoins,
                HonorBadges = p.HonorBadges,
                ElderSeals = p.ElderSeals
            })
            .FirstOrDefaultAsync();
    }

    public async Task AddCurrenciesAsync(Guid playerId, long bloodCoins, long honorBadges)
    {
        if (bloodCoins == 0 && honorBadges == 0)
            return;

        await _context.Players
            .Where(p => p.Id == playerId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.BloodCoins, p => p.BloodCoins + bloodCoins)
                .SetProperty(p => p.HonorBadges, p => p.HonorBadges + honorBadges));
    }

    public async Task<PlayerProgress> GetProgressAsync(Guid playerId)
    {
        int cleared = await _context.PlayerCampaignProgress.CountAsync(p => p.PlayerId == playerId);

        // Текущий уровень — следующий после пройденных; в конце кампании — последний
        int lastStage = await _context.CampaignStages.MaxAsync(s => (int?)s.Id) ?? 0;
        int currentId = Math.Min(cleared + 1, lastStage);
        var stage = currentId > 0
            ? await _context.CampaignStages.AsNoTracking().FirstOrDefaultAsync(s => s.Id == currentId)
            : null;

        bool inGuild = await _context.Users
            .Where(u => u.PlayerId == playerId)
            .AnyAsync(u => _context.GuildMembers.Any(m => m.UserId == u.Id));

        return new PlayerProgress(cleared, stage, inGuild);
    }
}