using Microsoft.EntityFrameworkCore;

public class ActivityRepository(GameDbContext context) : IActivityRepository
{
    // Два первых действия дня параллельно: оба не нашли счётчик, второй упрётся в ключ и прибавит к уже созданному
    private const int Attempts = 3;

    public async Task AddAsync(Guid playerId, DateTime day, ActivityKind kind, int amount)
    {
        for (int attempt = 0; attempt < Attempts; attempt++)
        {
            int updated = await context.PlayerDailyActivities
                .Where(a => a.PlayerId == playerId && a.Day == day && a.Kind == kind)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.Count, a => a.Count + amount));
            if (updated > 0)
                return;

            var counter = new PlayerDailyActivity { PlayerId = playerId, Day = day, Kind = kind, Count = amount };
            context.PlayerDailyActivities.Add(counter);
            try
            {
                await context.SaveChangesAsync();
                return;
            }
            catch (DbUpdateException) when (attempt < Attempts - 1)
            {
                // Счётчик успел создать параллельный запрос — прибавим к нему
            }
            finally
            {
                context.Entry(counter).State = EntityState.Detached;
            }
        }
    }

    public async Task<Dictionary<ActivityKind, int>> GetDayAsync(Guid playerId, DateTime day)
    {
        return await context.PlayerDailyActivities
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId && a.Day == day)
            .ToDictionaryAsync(a => a.Kind, a => a.Count);
    }

    public async Task<int> CountActivePlayersAsync(DateTime sinceDay)
    {
        return await context.PlayerDailyActivities
            .Where(a => a.Day >= sinceDay)
            .Select(a => a.PlayerId)
            .Distinct()
            .CountAsync();
    }
}
