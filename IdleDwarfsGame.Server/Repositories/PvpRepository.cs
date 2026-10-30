using Microsoft.EntityFrameworkCore;

public class PvpRepository(GameDbContext context) : IPvpRepository
{
    public async Task<List<PvpCandidate>> GetCandidatesAsync(Guid excludeUserId)
    {
        var users = await context.Users
            .AsNoTracking()
            .Where(u => u.Id != excludeUserId)
            .Select(u => new { u.Id, u.Username, u.PlayerId })
            .ToListAsync();
        return await WithSquadsAsync(users.Select(u => (u.Id, u.Username, u.PlayerId)).ToList());
    }

    public async Task<PvpCandidate?> GetCandidateAsync(Guid userId)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Username, u.PlayerId })
            .FirstOrDefaultAsync();
        if (user == null)
            return null;
        return (await WithSquadsAsync([(user.Id, user.Username, user.PlayerId)])).FirstOrDefault();
    }

    // Отряды одним запросом. Запасной отряд (HeroesRepository.GetSquadAsync) здесь не нужен:
    // у каждого аккаунта отряд расставлен с регистрации, а пустой отряд в PvP не выходит
    private async Task<List<PvpCandidate>> WithSquadsAsync(List<(Guid UserId, string Username, Guid PlayerId)> users)
    {
        var playerIds = users.Select(u => u.PlayerId).ToList();
        var heroes = await context.Heroes
            .AsNoTracking()
            .Where(h => h.PlaceInSquad != null && playerIds.Contains(h.PlayerId))
            .ToListAsync();
        var squads = heroes
            .GroupBy(h => h.PlayerId)
            .ToDictionary(g => g.Key, g => g.OrderBy(h => h.PlaceInSquad).Take(StarterSquad.Size).ToList());

        return users
            .Where(u => squads.ContainsKey(u.PlayerId))
            .Select(u => new PvpCandidate(u.UserId, u.Username, squads[u.PlayerId]))
            .ToList();
    }

    public async Task<List<RankedCandidate>> GetRankedCandidatesAsync(Guid excludeUserId, PvpSeasonType type, DateTime start) =>
        await QueryRankedCandidatesAsync(context.RankedSquads.Where(s => s.UserId != excludeUserId), type, start);

    public async Task<RankedCandidate?> GetRankedCandidateAsync(Guid userId, PvpSeasonType type, DateTime start) =>
        (await QueryRankedCandidatesAsync(context.RankedSquads.Where(s => s.UserId == userId), type, start)).FirstOrDefault();

    // Снимок отряда вместе с именем и очками в сезоне подбора — одним запросом
    private async Task<List<RankedCandidate>> QueryRankedCandidatesAsync(IQueryable<RankedSquad> squads, PvpSeasonType type, DateTime start)
    {
        var rows = await squads
            .AsNoTracking()
            .Select(s => new
            {
                Squad = s,
                Username = context.Users.Where(u => u.Id == s.UserId).Select(u => u.Username).FirstOrDefault(),
                Rating = context.PvpSeasonScores
                    .Where(x => x.UserId == s.UserId && x.Mode == PvpMode.Ranked && x.Type == type && x.Start == start)
                    .Select(x => (int?)x.Points)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return rows
            .Select(r => new RankedCandidate(r.Squad.UserId, r.Username ?? string.Empty, r.Squad.Heroes(), r.Rating))
            .Where(c => c.Squad.Count > 0)
            .ToList();
    }

    public async Task<RankedSquad?> GetRankedSquadAsync(Guid userId)
    {
        return await context.RankedSquads
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<bool> SaveRankedSquadAsync(RankedSquad squad, DateTime? previousLockedAt)
    {
        // Условный апдейт: из двух одновременных перезакреплений пройдёт только первое
        if (previousLockedAt is { } lockedAt)
        {
            return await context.RankedSquads
                .Where(s => s.UserId == squad.UserId && s.LockedAt == lockedAt)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.LockedAt, squad.LockedAt)
                    .SetProperty(x => x.HeroesJson, squad.HeroesJson)) > 0;
        }

        context.RankedSquads.Add(squad);
        try
        {
            await context.SaveChangesAsync();
            context.Entry(squad).State = EntityState.Detached;
            return true;
        }
        catch (DbUpdateException)
        {
            // Первый снимок только что сохранил параллельный запрос
            context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<HashSet<Guid>> GetBeatenAsync(Guid attackerUserId, DateTime day, PvpMode mode)
    {
        var ids = await context.PvpBattles
            .AsNoTracking()
            .Where(b => b.AttackerUserId == attackerUserId && b.Day == day && b.Mode == mode && b.AttackerWon)
            .Select(b => b.DefenderUserId)
            .ToListAsync();
        return ids.ToHashSet();
    }

    public async Task<bool> RecordBattleAsync(PvpBattle battle, IReadOnlyList<(PvpSeasonType Type, DateTime Start)> seasons)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Уникальный индекс по победам и есть проверка: вторая победа над тем же соперником за сутки не запишется
        context.PvpBattles.Add(battle);
        try
        {
            await context.SaveChangesAsync();
            context.Entry(battle).State = EntityState.Detached;
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();
            return false;
        }

        foreach (var (type, start) in seasons)
            await AddPointsAsync(battle, type, start);

        await transaction.CommitAsync();
        return true;
    }

    // Условный апдейт строки сезона; строки нет — первый бой игрока в сезоне
    private async Task AddPointsAsync(PvpBattle battle, PvpSeasonType type, DateTime start)
    {
        var mode = battle.Mode;
        int delta = battle.Points;
        int wins = battle.AttackerWon ? 1 : 0;
        int losses = 1 - wins;
        // Рейтинговая арена начинает сезон не с нуля
        int initial = mode == PvpMode.Ranked ? RankedArena.StartRating : 0;

        for (int attempt = 0; attempt < 2; attempt++)
        {
            int updated = await context.PvpSeasonScores
                .Where(s => s.UserId == battle.AttackerUserId && s.Mode == mode && s.Type == type && s.Start == start)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Points, x => x.Points + delta < 0 ? 0 : x.Points + delta)
                    .SetProperty(x => x.Wins, x => x.Wins + wins)
                    .SetProperty(x => x.Losses, x => x.Losses + losses)
                    .SetProperty(x => x.UpdatedAt, battle.FoughtAt));
            if (updated > 0)
                return;

            var score = new PvpSeasonScore
            {
                UserId = battle.AttackerUserId,
                Mode = mode,
                Type = type,
                Start = start,
                Points = Math.Max(0, initial + delta),
                Wins = wins,
                Losses = losses,
                UpdatedAt = battle.FoughtAt
            };
            context.PvpSeasonScores.Add(score);
            try
            {
                await context.SaveChangesAsync();
                // Дальше строка меняется через ExecuteUpdate — отслеживаемая копия устарела бы
                context.Entry(score).State = EntityState.Detached;
                return;
            }
            catch (DbUpdateException)
            {
                // Строку только что создал параллельный бой — повторяем апдейт
                context.ChangeTracker.Clear();
            }
        }
    }

    public async Task<PvpSeasonScore?> GetScoreAsync(Guid userId, PvpMode mode, PvpSeasonType type, DateTime start)
    {
        return await context.PvpSeasonScores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Mode == mode && s.Type == type && s.Start == start);
    }

    public async Task<int> CountAheadAsync(PvpSeasonScore score)
    {
        return await context.PvpSeasonScores.CountAsync(s =>
            s.Mode == score.Mode && s.Type == score.Type && s.Start == score.Start &&
            (s.Points > score.Points
             || (s.Points == score.Points && s.Wins > score.Wins)
             || (s.Points == score.Points && s.Wins == score.Wins && s.UpdatedAt < score.UpdatedAt)));
    }

    public async Task<List<PvpLeaderboardEntry>> GetLeaderboardAsync(PvpMode mode, PvpSeasonType type, DateTime start, int limit)
    {
        var rows = await context.PvpSeasonScores
            .AsNoTracking()
            .Where(s => s.Mode == mode && s.Type == type && s.Start == start)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.Wins)
            .ThenBy(s => s.UpdatedAt)
            .Take(limit)
            .Select(s => new
            {
                s.UserId,
                Username = context.Users.Where(u => u.Id == s.UserId).Select(u => u.Username).FirstOrDefault(),
                s.Points,
                s.Wins,
                s.Losses
            })
            .ToListAsync();

        return rows.Select(r => new PvpLeaderboardEntry(r.UserId, r.Username ?? string.Empty, r.Points, r.Wins, r.Losses)).ToList();
    }

    public async Task<List<PvpHistoryEntry>> GetHistoryAsync(Guid userId, PvpMode mode, int limit)
    {
        var battles = await context.PvpBattles
            .AsNoTracking()
            .Where(b => b.Mode == mode && (b.AttackerUserId == userId || b.DefenderUserId == userId))
            .OrderByDescending(b => b.FoughtAt)
            .Take(limit)
            .ToListAsync();

        var userIds = battles.SelectMany(b => new[] { b.AttackerUserId, b.DefenderUserId }).Distinct().ToList();
        var names = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        return battles
            .Select(b => new PvpHistoryEntry(b, names.GetValueOrDefault(b.AttackerUserId, string.Empty), names.GetValueOrDefault(b.DefenderUserId, string.Empty)))
            .ToList();
    }
}
