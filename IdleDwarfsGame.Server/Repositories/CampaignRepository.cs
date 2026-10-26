using Microsoft.EntityFrameworkCore;

public class CampaignRepository(GameDbContext context) : ICampaignRepository
{
    private IQueryable<CampaignStage> StagesWithDetails => context.CampaignStages
        .AsNoTracking()
        .Include(s => s.Arena)
        .Include(s => s.Enemies.OrderBy(e => e.Position))
            .ThenInclude(e => e.Hero);

    public async Task<List<int>> GetStageIdsAsync()
    {
        return await context.CampaignStages
            .OrderBy(s => s.Id)
            .Select(s => s.Id)
            .ToListAsync();
    }

    public async Task<List<CampaignStage>> GetStagesAsync(IReadOnlyCollection<int> stageIds)
    {
        return await StagesWithDetails
            .Where(s => stageIds.Contains(s.Id))
            .OrderBy(s => s.Id)
            .ToListAsync();
    }

    public async Task<CampaignStage?> GetStageAsync(int stageId)
    {
        return await StagesWithDetails.FirstOrDefaultAsync(s => s.Id == stageId);
    }

    public async Task<HashSet<int>> GetClearedStageIdsAsync(Guid playerId)
    {
        var ids = await context.PlayerCampaignProgress
            .Where(p => p.PlayerId == playerId)
            .Select(p => p.StageId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task<bool> MarkClearedAsync(Guid playerId, int stageId, PlayerReward? reward = null)
    {
        var alreadyCleared = await context.PlayerCampaignProgress
            .AnyAsync(p => p.PlayerId == playerId && p.StageId == stageId);
        if (alreadyCleared)
            return false;

        await context.PlayerCampaignProgress.AddAsync(new PlayerCampaignProgress
        {
            PlayerId = playerId,
            StageId = stageId,
            ClearedAt = DateTime.UtcNow
        });
        // Одним SaveChanges с прогрессом: награда не потеряется и не выдастся дважды
        if (reward != null)
            await context.PlayerRewards.AddAsync(reward);

        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Уровень только что засчитал параллельный бой (ключ PlayerId + StageId)
            context.ChangeTracker.Clear();
            return false;
        }
    }
}
