using Microsoft.EntityFrameworkCore;

public class ExpeditionRepository(GameDbContext context) : IExpeditionRepository
{
    public async Task<List<Expedition>> GetBoardAsync(Guid playerId, DateTime boardStartsAt)
    {
        return await context.Expeditions
            .AsNoTracking()
            .Include(e => e.Heroes.OrderBy(h => h.Slot))
            .Where(e => e.PlayerId == playerId
                && (e.BoardStartsAt == boardStartsAt || e.Status == ExpeditionStatus.InProgress))
            .OrderByDescending(e => e.BoardStartsAt)
            .ThenBy(e => e.Slot)
            .ToListAsync();
    }

    public async Task<bool> ReplaceBoardAsync(Guid playerId, DateTime boardStartsAt, IReadOnlyList<Expedition> offers)
    {
        // Запущенные и выполненные задания старых досок остаются — у первых ещё не забрана награда, вторые — история
        await context.Expeditions
            .Where(e => e.PlayerId == playerId && e.Status == ExpeditionStatus.Available && e.BoardStartsAt < boardStartsAt)
            .ExecuteDeleteAsync();

        await context.Expeditions.AddRangeAsync(offers);
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Доску уже создал параллельный запрос (уникальный индекс PlayerId + BoardStartsAt + Slot)
            context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<Expedition?> GetAsync(Guid playerId, Guid expeditionId)
    {
        return await context.Expeditions
            .AsNoTracking()
            .Include(e => e.Heroes.OrderBy(h => h.Slot))
            .FirstOrDefaultAsync(e => e.Id == expeditionId && e.PlayerId == playerId);
    }

    public async Task<HashSet<Guid>> GetBusyHeroIdsAsync(Guid playerId)
    {
        var ids = await context.Expeditions
            .Where(e => e.PlayerId == playerId)
            .SelectMany(e => e.Heroes)
            .Select(h => h.HeroId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task StartAsync(Expedition expedition)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт: из двух одновременных стартов одного задания пройдёт только первый
        int started = await context.Expeditions
            .Where(e => e.Id == expedition.Id && e.Status == ExpeditionStatus.Available)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Status, ExpeditionStatus.InProgress)
                .SetProperty(e => e.StartedAt, expedition.StartedAt)
                .SetProperty(e => e.EndsAt, expedition.EndsAt)
                .SetProperty(e => e.Power, expedition.Power)
                .SetProperty(e => e.Gold, expedition.Gold)
                .SetProperty(e => e.Experience, expedition.Experience));
        if (started == 0)
            throw new InvalidOperationException("Expedition is already started");

        await context.ExpeditionHeroes.AddRangeAsync(expedition.Heroes);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Уникальный индекс по HeroId: героя только что отправили в другое задание
            context.ChangeTracker.Clear();
            throw new InvalidOperationException("One of the heroes is already on an expedition");
        }

        await transaction.CommitAsync();
    }

    public async Task<bool> ClaimAsync(Expedition expedition, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт защищает от двойного начисления при повторном клике
        int claimed = await context.Expeditions
            .Where(e => e.Id == expedition.Id && e.Status == ExpeditionStatus.InProgress && e.EndsAt <= now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Status, ExpeditionStatus.Claimed)
                .SetProperty(e => e.ClaimedAt, now));
        if (claimed == 0)
            return false;

        await context.ExpeditionHeroes
            .Where(h => h.ExpeditionId == expedition.Id)
            .ExecuteDeleteAsync();

        await context.Players
            .Where(p => p.Id == expedition.PlayerId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Gold, p => p.Gold + expedition.Gold)
                .SetProperty(p => p.Experience, p => p.Experience + expedition.Experience));

        await transaction.CommitAsync();
        return true;
    }
}
