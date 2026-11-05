using Microsoft.EntityFrameworkCore;

public class PvpRepository(GameDbContext context) : IPvpRepository
{
    public async Task<List<PvpCandidate>> GetCandidatesAsync(Guid excludeUserId) =>
        await QueryCandidates(context.Users.Where(u => u.Id != excludeUserId)).ToListAsync();

    public async Task<PvpCandidate?> GetCandidateAsync(Guid userId) =>
        await QueryCandidates(context.Users.Where(u => u.Id == userId)).FirstOrDefaultAsync();

    // Только id, имя и сохранённая мощь: героев подбор не трогает. Мощь 0 — отряда защиты нет, в PvP такой не выходит
    private IQueryable<PvpCandidate> QueryCandidates(IQueryable<User> users) => users
        .AsNoTracking()
        .Join(context.Players, u => u.PlayerId, p => p.Id, (u, p) => new { u.Id, u.Username, u.PlayerId, p.SquadPower })
        .Where(x => x.SquadPower > 0)
        .Select(x => new PvpCandidate(x.Id, x.Username, x.PlayerId, x.SquadPower));

    // Отряды одним запросом. Запасной отряд (HeroesRepository.GetSquadAsync) здесь не нужен:
    // у каждого аккаунта отряд расставлен с регистрации, а пустой отряд в PvP не выходит
    public async Task<Dictionary<Guid, List<Hero>>> GetDefenseSquadsAsync(IReadOnlyCollection<Guid> playerIds)
    {
        if (playerIds.Count == 0)
            return [];

        var heroes = await context.Heroes
            .AsNoTracking()
            .Include(h => h.Equipment)
            .Where(h => h.PlaceInSquad != null && playerIds.Contains(h.PlayerId))
            .ToListAsync();
        return heroes
            .GroupBy(h => h.PlayerId)
            .ToDictionary(g => g.Key, g => g.OrderBy(h => h.PlaceInSquad).Take(StarterSquad.Size).ToList());
    }

    public async Task<Dictionary<Guid, int>> GetSquadPowersAsync() =>
        await context.Players.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.SquadPower);

    public async Task SetSquadPowerAsync(Guid playerId, int power) =>
        await context.Players
            .Where(p => p.Id == playerId && p.SquadPower != power)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.SquadPower, power));

    public async Task<List<RankedCandidate>> GetRankedCandidatesAsync(Guid excludeUserId, PvpSeasonType type, DateTime start) =>
        await QueryRankedCandidatesAsync(context.RankedSquads.Where(s => s.UserId != excludeUserId), type, start);

    public async Task<RankedCandidate?> GetRankedCandidateAsync(Guid userId, PvpSeasonType type, DateTime start) =>
        (await QueryRankedCandidatesAsync(context.RankedSquads.Where(s => s.UserId == userId), type, start)).FirstOrDefault();

    // Мощь снимка вместе с именем и очками в сезоне подбора — одним запросом и без JSON героев
    private async Task<List<RankedCandidate>> QueryRankedCandidatesAsync(IQueryable<RankedSquad> squads, PvpSeasonType type, DateTime start)
    {
        var rows = await squads
            .AsNoTracking()
            .Where(s => s.Power > 0)
            .Select(s => new
            {
                s.UserId,
                s.Power,
                Username = context.Users.Where(u => u.Id == s.UserId).Select(u => u.Username).FirstOrDefault(),
                Rating = context.PvpSeasonScores
                    .Where(x => x.UserId == s.UserId && x.Mode == PvpMode.Ranked && x.Type == type && x.Start == start)
                    .Select(x => (int?)x.Points)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return rows
            .Select(r => new RankedCandidate(r.UserId, r.Username ?? string.Empty, r.Power, r.Rating))
            .ToList();
    }

    public async Task<Dictionary<Guid, List<Hero>>> GetRankedSquadsAsync(IReadOnlyCollection<Guid> userIds)
    {
        if (userIds.Count == 0)
            return [];

        var squads = await context.RankedSquads
            .AsNoTracking()
            .Where(s => userIds.Contains(s.UserId))
            .ToListAsync();
        return squads.ToDictionary(s => s.UserId, s => s.Heroes());
    }

    public async Task<List<RankedSquad>> GetAllRankedSquadsAsync() =>
        await context.RankedSquads.AsNoTracking().ToListAsync();

    public async Task SetRankedSquadPowerAsync(Guid userId, int power) =>
        await context.RankedSquads
            .Where(s => s.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Power, power));

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
                    .SetProperty(x => x.HeroesJson, squad.HeroesJson)
                    .SetProperty(x => x.Power, squad.Power)) > 0;
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

    public async Task<List<PvpScout>> GetScoutsAsync(Guid userId, DateTime day) =>
        await context.PvpScouts
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Day == day)
            .ToListAsync();

    public async Task<bool> SaveScoutAsync(PvpScout scout, int dailyLimit)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // UPDATE без изменений блокирует строку игрока до коммита: две параллельные разведки не проскочат лимит,
        // прочитав один и тот же счёт (в SQL Server на READ COMMITTED сам COUNT от этого не защищает)
        await context.Users
            .Where(u => u.Id == scout.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Username, u => u.Username));

        int updated = await context.PvpScouts
            .Where(s => s.UserId == scout.UserId && s.Day == scout.Day && s.Mode == scout.Mode && s.OpponentUserId == scout.OpponentUserId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.WinChance, scout.WinChance)
                .SetProperty(x => x.PlayerPower, scout.PlayerPower)
                .SetProperty(x => x.EnemyPower, scout.EnemyPower)
                .SetProperty(x => x.ScoutedAt, scout.ScoutedAt));
        if (updated == 0)
        {
            if (await context.PvpScouts.CountAsync(s => s.UserId == scout.UserId && s.Day == scout.Day) >= dailyLimit)
                return false;
            context.PvpScouts.Add(scout);
            await context.SaveChangesAsync();
            context.Entry(scout).State = EntityState.Detached;
        }

        await transaction.CommitAsync();
        return true;
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
