using Microsoft.EntityFrameworkCore;

public class TowerRepository(GameDbContext context) : ITowerRepository
{
    public async Task<int> GetFloorAsync(Guid playerId, DateTime week)
    {
        return await context.PlayerTowerProgress
            .Where(p => p.PlayerId == playerId && p.WeekStart == week)
            .Select(p => p.Floor)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetBestBeforeAsync(Guid playerId, DateTime week)
    {
        return await context.PlayerTowerProgress
            .Where(p => p.PlayerId == playerId && p.WeekStart < week)
            .MaxAsync(p => (int?)p.Floor) ?? 0;
    }

    public async Task<bool> AdvanceAsync(Guid playerId, DateTime week, int fromFloor, int toFloor, PlayerReward reward, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт: из двух одновременных боёв за один этаж засчитается только первый
        int updated = await context.PlayerTowerProgress
            .Where(p => p.PlayerId == playerId && p.WeekStart == week && p.Floor == fromFloor)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Floor, toFloor)
                .SetProperty(p => p.UpdatedAt, now));

        if (updated == 0)
        {
            // Строки ещё нет только у первого этажа недели; параллельной второй вставки не даст ключ (PlayerId + WeekStart)
            if (fromFloor != 0 || await context.PlayerTowerProgress.AnyAsync(p => p.PlayerId == playerId && p.WeekStart == week))
                return false;
            context.PlayerTowerProgress.Add(new PlayerTowerProgress { PlayerId = playerId, WeekStart = week, Floor = toFloor, UpdatedAt = now });
        }

        context.PlayerRewards.Add(reward);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();
            return false;
        }

        await transaction.CommitAsync();
        return true;
    }
}
