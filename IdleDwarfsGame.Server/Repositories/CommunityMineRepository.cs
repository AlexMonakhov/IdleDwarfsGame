using Microsoft.EntityFrameworkCore;

public class CommunityMineRepository(GameDbContext context) : ICommunityMineRepository
{
    public async Task<List<CommunityDeposit>> GetDepositsAsync(DateTime eventStart)
    {
        return await context.CommunityDeposits
            .AsNoTracking()
            .Where(d => d.EventStart == eventStart)
            .OrderBy(d => d.Index)
            .ToListAsync();
    }

    public async Task<DepositState?> GetStateAsync(Guid depositId)
    {
        var deposit = await context.CommunityDeposits.AsNoTracking().FirstOrDefaultAsync(d => d.Id == depositId);
        if (deposit == null)
            return null;

        var workers = await context.DepositWorkers.AsNoTracking().Where(w => w.DepositId == depositId).ToListAsync();
        var dwarves = await context.DepositDwarves.AsNoTracking().Where(d => d.DepositId == depositId).ToListAsync();
        return new DepositState(deposit, workers, dwarves);
    }

    public async Task<List<DepositState>> GetStatesAsync(DateTime eventStart)
    {
        var deposits = await GetDepositsAsync(eventStart);
        var ids = deposits.Select(d => d.Id).ToList();
        var workers = await context.DepositWorkers.AsNoTracking().Where(w => ids.Contains(w.DepositId)).ToListAsync();
        var dwarves = await context.DepositDwarves.AsNoTracking().Where(d => ids.Contains(d.DepositId)).ToListAsync();
        return deposits
            .Select(d => new DepositState(d, workers.Where(w => w.DepositId == d.Id).ToList(), dwarves.Where(x => x.DepositId == d.Id).ToList()))
            .ToList();
    }

    public async Task<bool> TryCreateAsync(CommunityDeposit deposit)
    {
        context.CommunityDeposits.Add(deposit);
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Месторождение с этим номером только что создал параллельный запрос
            return false;
        }
        finally
        {
            context.ChangeTracker.Clear();
        }
    }

    public async Task<DepositSaveOutcome> SaveAsync(DepositSave save)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var deposit = save.Deposit;

        // Условный апдейт: из двух параллельных изменений месторождения пройдёт только первое
        int updated = await context.CommunityDeposits
            .Where(d => d.Id == deposit.Id && d.Version == save.ExpectedVersion)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.OreMined, deposit.OreMined)
                .SetProperty(d => d.UpdatedAt, deposit.UpdatedAt)
                .SetProperty(d => d.SpeedLevel, deposit.SpeedLevel)
                .SetProperty(d => d.ExhaustedAt, deposit.ExhaustedAt)
                .SetProperty(d => d.ClosedAt, deposit.ClosedAt)
                .SetProperty(d => d.Version, save.ExpectedVersion + 1));
        if (updated == 0)
            return DepositSaveOutcome.Changed;

        if (save.PayerId is { } payer && save.Gold > 0)
        {
            long gold = save.Gold;
            int paid = await context.Players
                .Where(p => p.Id == payer && p.Gold >= gold)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, p => p.Gold - gold));
            if (paid == 0)
                return DepositSaveOutcome.NotEnoughGold;
        }

        // Вклады: строка месторождения заблокирована апдейтом выше — параллельной вставки вклада быть не может
        foreach (var worker in save.Workers)
        {
            int changed = await context.DepositWorkers
                .Where(w => w.DepositId == deposit.Id && w.PlayerId == worker.PlayerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(w => w.Mined, worker.Mined)
                    .SetProperty(w => w.UpgradePoints, worker.UpgradePoints)
                    .SetProperty(w => w.Upgrades, worker.Upgrades));
            if (changed == 0)
                context.DepositWorkers.Add(new DepositWorker
                {
                    DepositId = deposit.Id, PlayerId = worker.PlayerId, Mined = worker.Mined,
                    UpgradePoints = worker.UpgradePoints, Upgrades = worker.Upgrades
                });
        }

        if (save.ReleaseAll)
        {
            await context.DepositDwarves.Where(d => d.DepositId == deposit.Id).ExecuteDeleteAsync();
        }
        else if (save.RemoveHeroIds.Count > 0)
        {
            await context.DepositDwarves
                .Where(d => d.DepositId == deposit.Id && save.RemoveHeroIds.Contains(d.HeroId))
                .ExecuteDeleteAsync();
        }

        if (!save.ReleaseAll)
            context.DepositDwarves.AddRange(save.AddDwarves);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Дворф уже работает на другом месторождении (уникальный HeroId)
            context.ChangeTracker.Clear();
            return DepositSaveOutcome.HeroBusy;
        }
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return DepositSaveOutcome.Saved;
    }

    public async Task<List<(CommunityDeposit Deposit, DepositWorker Worker)>> GetUnclaimedAsync(Guid playerId, DateTime eventStart)
    {
        var rows = await context.DepositWorkers
            .AsNoTracking()
            .Where(w => w.PlayerId == playerId)
            .Join(context.CommunityDeposits.Where(d => d.EventStart == eventStart && d.ClosedAt != null),
                w => w.DepositId, d => d.Id, (w, d) => new { Deposit = d, Worker = w })
            .Where(x => !context.DepositRewardClaims.Any(c => c.DepositId == x.Deposit.Id && c.PlayerId == playerId))
            .OrderBy(x => x.Deposit.Index)
            .ToListAsync();
        return rows.Select(r => (r.Deposit, r.Worker)).ToList();
    }

    public async Task<bool> ClaimAsync(Guid playerId, DateTime eventStart, IReadOnlyList<DepositRewardClaim> claims)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        context.DepositRewardClaims.AddRange(claims);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();
            return false;
        }
        context.ChangeTracker.Clear();

        await EventWallets.AddAsync(context, playerId, eventStart, claims.Sum(c => c.Currency));
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return true;
    }

    public async Task<Dictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<Guid> playerIds)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => playerIds.Contains(u.PlayerId))
            .ToDictionaryAsync(u => u.PlayerId, u => u.Username);
    }
}
