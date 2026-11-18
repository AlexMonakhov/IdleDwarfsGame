using Microsoft.EntityFrameworkCore;

public class AchievementRepository(GameDbContext context) : IAchievementRepository
{
    public async Task<Dictionary<string, int>> GetClaimedAsync(Guid playerId)
    {
        return await context.PlayerAchievements
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId)
            .ToDictionaryAsync(a => a.Key, a => a.ClaimedTiers);
    }

    public async Task<AchievementStats> GetStatsAsync(Guid playerId)
    {
        var lifetime = await context.PlayerLifetimeActivities
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId)
            .ToDictionaryAsync(a => a.Kind, a => a.Count);

        int stages = await context.PlayerCampaignProgress.CountAsync(p => p.PlayerId == playerId);
        int towerFloor = await context.PlayerTowerProgress
            .Where(t => t.PlayerId == playerId)
            .MaxAsync(t => (int?)t.Floor) ?? 0;
        int heroes = await context.Heroes.CountAsync(h => h.PlayerId == playerId);
        int friends = await context.Friendships
            .CountAsync(f => context.Users.Any(u => u.PlayerId == playerId && u.Id == f.UserId));

        var login = await context.PlayerLoginCalendars
            .AsNoTracking()
            .Where(c => c.PlayerId == playerId)
            .Select(c => new { c.TotalDays, c.BestStreak })
            .FirstOrDefaultAsync();

        return new AchievementStats(lifetime, stages, towerFloor, heroes, friends, login?.TotalDays ?? 0, login?.BestStreak ?? 0);
    }

    public async Task<bool> ClaimAsync(Guid playerId, string key, int expectedClaimed, int claimedTiers, PlayerReward reward,
        int rainbowCoins, int elderSeals, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if (expectedClaimed > 0)
        {
            // Условный апдейт: двойной клик или вторая вкладка не заберут ступени дважды
            int updated = await context.PlayerAchievements
                .Where(a => a.PlayerId == playerId && a.Key == key && a.ClaimedTiers == expectedClaimed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.ClaimedTiers, claimedTiers)
                    .SetProperty(a => a.UpdatedAt, now));
            if (updated == 0)
                return false;
        }
        else
        {
            // Первая ступень: ключ (PlayerId, Key) не даст завести строку дважды
            var row = new PlayerAchievement { PlayerId = playerId, Key = key, ClaimedTiers = claimedTiers, UpdatedAt = now };
            context.PlayerAchievements.Add(row);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return false;
            }
            finally
            {
                context.ChangeTracker.Clear();
            }
        }

        await BonusGrant.ApplyAsync(context, playerId, reward, rainbowCoins, elderSeals);

        await transaction.CommitAsync();
        return true;
    }
}

public class LoginCalendarRepository(GameDbContext context) : ILoginCalendarRepository
{
    public async Task<PlayerLoginCalendar?> GetAsync(Guid playerId)
    {
        return await context.PlayerLoginCalendars
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PlayerId == playerId);
    }

    public async Task<bool> ClaimAsync(PlayerLoginCalendar next, int? expectedVersion, PlayerReward reward, int rainbowCoins, int elderSeals)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if (expectedVersion is { } version)
        {
            int updated = await context.PlayerLoginCalendars
                .Where(c => c.PlayerId == next.PlayerId && c.Version == version)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Cycle, next.Cycle)
                    .SetProperty(c => c.Day, next.Day)
                    .SetProperty(c => c.LastClaimDay, next.LastClaimDay)
                    .SetProperty(c => c.Streak, next.Streak)
                    .SetProperty(c => c.BestStreak, next.BestStreak)
                    .SetProperty(c => c.TotalDays, next.TotalDays)
                    .SetProperty(c => c.Version, next.Version));
            if (updated == 0)
                return false;
        }
        else
        {
            // Первая награда: ключ PlayerId не даст получить её дважды
            context.PlayerLoginCalendars.Add(next);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return false;
            }
            finally
            {
                context.ChangeTracker.Clear();
            }
        }

        await BonusGrant.ApplyAsync(context, next.PlayerId, reward, rainbowCoins, elderSeals);

        await transaction.CommitAsync();
        return true;
    }
}

// Награда с валютами: золото, опыт и сундуки — как у наград за бои (RewardGrant), плюс радужные монеты и печати.
// Вызывается внутри транзакции того, кто выдаёт
public static class BonusGrant
{
    public static async Task ApplyAsync(GameDbContext context, Guid playerId, PlayerReward reward, int rainbowCoins, int elderSeals)
    {
        await RewardGrant.ApplyAsync(context, playerId, reward);

        if (rainbowCoins > 0 || elderSeals > 0)
        {
            await context.Players
                .Where(p => p.Id == playerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.RainbowCoins, p => p.RainbowCoins + rainbowCoins)
                    .SetProperty(p => p.ElderSeals, p => p.ElderSeals + elderSeals));
        }

        context.ChangeTracker.Clear();
    }
}
