using Microsoft.EntityFrameworkCore;

public class TutorialRepository(GameDbContext context) : ITutorialRepository
{
    // Два первых изменения одного ключа параллельно: оба не нашли строку, второй упрётся в ключ и обновит созданную
    private const int Attempts = 3;

    public Task<List<PlayerTutorial>> GetAsync(Guid playerId) =>
        context.PlayerTutorials.AsNoTracking().Where(t => t.PlayerId == playerId).ToListAsync();

    public Task<PlayerTutorial?> GetOneAsync(Guid playerId, string key) =>
        context.PlayerTutorials.AsNoTracking().FirstOrDefaultAsync(t => t.PlayerId == playerId && t.Key == key);

    public async Task<bool> TryMarkDoneAsync(Guid playerId, string key, DateTime now)
    {
        // Строка есть — отмечаем, только если ещё не отмечена: из двух параллельных запросов пройдёт один
        int updated = await context.PlayerTutorials
            .Where(t => t.PlayerId == playerId && t.Key == key && t.DoneAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.DoneAt, now).SetProperty(t => t.UpdatedAt, now));
        if (updated > 0)
            return true;
        if (await context.PlayerTutorials.AnyAsync(t => t.PlayerId == playerId && t.Key == key))
            return false;

        var row = new PlayerTutorial { PlayerId = playerId, Key = key, DoneAt = now, UpdatedAt = now };
        context.PlayerTutorials.Add(row);
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Строку только что создал параллельный запрос — выдача уже его
            return false;
        }
        finally
        {
            context.Entry(row).State = EntityState.Detached;
        }
    }

    public Task SetStepAsync(Guid playerId, string key, int step, DateTime now) =>
        UpsertAsync(playerId, key,
            update: rows => rows.ExecuteUpdateAsync(s => s.SetProperty(t => t.Step, step).SetProperty(t => t.UpdatedAt, now)),
            create: () => new PlayerTutorial { PlayerId = playerId, Key = key, Step = step, UpdatedAt = now });

    public Task<int> CountAsync(Guid playerId) =>
        context.PlayerTutorials.CountAsync(t => t.PlayerId == playerId);

    public Task SetStatusAsync(Guid playerId, string key, TutorialStatus status, DateTime now) =>
        UpsertAsync(playerId, key,
            update: rows => rows.ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, status)
                .SetProperty(t => t.UpdatedAt, now)),
            create: () => new PlayerTutorial { PlayerId = playerId, Key = key, Status = status, UpdatedAt = now });

    public Task MarkDoneAsync(Guid playerId, string key, DateTime now) =>
        UpsertAsync(playerId, key,
            // Уже отмеченная строка не обновляется, но и не создаётся заново: она есть
            update: async rows => await rows.AnyAsync(t => t.DoneAt != null)
                ? 1
                : await rows.ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.DoneAt, now)
                    .SetProperty(t => t.UpdatedAt, now)),
            create: () => new PlayerTutorial { PlayerId = playerId, Key = key, DoneAt = now, UpdatedAt = now });

    public async Task ResetStatusesAsync(Guid playerId, DateTime now, IReadOnlyCollection<string> keep)
    {
        await context.PlayerTutorials
            .Where(t => t.PlayerId == playerId && t.DoneAt == null && !keep.Contains(t.Key))
            .ExecuteDeleteAsync();
        await context.PlayerTutorials
            .Where(t => t.PlayerId == playerId && t.Status != null && !keep.Contains(t.Key))
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, (TutorialStatus?)null)
                .SetProperty(t => t.UpdatedAt, now));
    }

    private async Task UpsertAsync(Guid playerId, string key, Func<IQueryable<PlayerTutorial>, Task<int>> update, Func<PlayerTutorial> create)
    {
        var rows = context.PlayerTutorials.Where(t => t.PlayerId == playerId && t.Key == key);
        for (int attempt = 0; attempt < Attempts; attempt++)
        {
            if (await update(rows) > 0)
                return;

            var row = create();
            context.PlayerTutorials.Add(row);
            try
            {
                await context.SaveChangesAsync();
                return;
            }
            catch (DbUpdateException) when (attempt < Attempts - 1)
            {
                // Строку успел создать параллельный запрос — обновим её
            }
            finally
            {
                context.Entry(row).State = EntityState.Detached;
            }
        }
    }
}
