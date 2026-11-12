using Microsoft.EntityFrameworkCore;

// Причина отказа — фронт показывает по коду переведённый текст (pvp.bets.errors.<code>)
public enum ArenaBetError
{
    NoMatch,
    BettingClosed,
    AlreadyBet,
    InvalidAmount,
    InvalidSide,
    OwnMatch,
    NotEnoughCoins,
    NothingToClaim,
    NotResolved
}

public class ArenaBetException(ArenaBetError code, string message) : InvalidOperationException(message)
{
    public ArenaBetError Code { get; } = code;
}

// «Бой дня»: каждые WindowHours часов сервер выбирает два сильных отряда (снимки на начало окна), игроки ставят
// кровавые монеты на исход. Ставки принимаются всё окно, бой проводится после его конца — с сохранённым сидом,
// поэтому его можно пересмотреть сколько угодно раз, и итог не изменится.
// Выплата — тотализатор: весь банк за вычетом HouseCut делится между угадавшими пропорционально ставкам;
// если на победителя не ставил никто — ставки возвращаются
public static class ArenaBetRules
{
    public const int WindowHours = 6;
    public static readonly TimeSpan Window = TimeSpan.FromHours(WindowHours);

    // Ставка: PvP приносит ~40 монет в сутки — максимальная ставка около двух дней боёв
    public const long MinBet = 5;
    public const long MaxBet = 100;
    public const double HouseCut = 0.1;

    // Из скольких сильнейших отрядов выбирается пара (соседи по мощи — чтобы бой был равным)
    public const int CandidatePool = 6;

    public static DateTime WindowOf(DateTime utc) => new(utc.Ticks - utc.Ticks % Window.Ticks, DateTimeKind.Utc);

    public static long PayoutFor(ArenaBet bet, int winner, long poolA, long poolB)
    {
        long winnerPool = winner == 1 ? poolA : poolB;
        if (winnerPool == 0)
            return bet.Amount;
        if (bet.Side != winner)
            return 0;
        return (long)Math.Floor((poolA + poolB) * (1 - HouseCut) * bet.Amount / winnerPool);
    }

    // Во сколько раз вырастет ставка на сторону при нынешнем банке (с учётом своей ставки amount)
    public static double MultiplierFor(long sidePool, long otherPool, long amount = 0)
    {
        double side = sidePool + amount;
        return side <= 0 ? 0 : (side + otherPool) * (1 - HouseCut) / side;
    }
}

// Концепт: сервис работает с GameDbContext напрямую, без отдельного репозитория
public class ArenaBetService(
    GameDbContext db,
    IPvpRepository pvp,
    IEntityFactory entityFactory,
    IPlayerRepository players,
    TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<ArenaBetOverview> GetAsync(Guid userId)
    {
        var now = Now;
        await ResolveDueAsync(now);
        var window = ArenaBetRules.WindowOf(now);
        var current = await EnsureMatchAsync(window);
        var previous = await db.ArenaBetMatches.AsNoTracking()
            .Where(m => m.WindowStart < window && m.Winner != null)
            .OrderByDescending(m => m.WindowStart)
            .FirstOrDefaultAsync();
        var playerId = await PlayerIdAsync(userId);

        return new ArenaBetOverview
        {
            ServerTime = now,
            Current = current == null ? null : await DescribeAsync(current, userId, now),
            Previous = previous == null ? null : await DescribeAsync(previous, userId, now),
            Claimable = await ClaimableAsync(userId),
            MinBet = ArenaBetRules.MinBet,
            MaxBet = ArenaBetRules.MaxBet,
            HouseCut = ArenaBetRules.HouseCut,
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }

    public async Task<ArenaBetOverview> PlaceAsync(Guid userId, int side, long amount)
    {
        var now = Now;
        if (side is not (1 or 2))
            throw new ArenaBetException(ArenaBetError.InvalidSide, "Pick a side");
        if (amount < ArenaBetRules.MinBet || amount > ArenaBetRules.MaxBet)
            throw new ArenaBetException(ArenaBetError.InvalidAmount, $"A bet is from {ArenaBetRules.MinBet} to {ArenaBetRules.MaxBet} blood coins");

        var window = ArenaBetRules.WindowOf(now);
        var match = await EnsureMatchAsync(window)
            ?? throw new ArenaBetException(ArenaBetError.NoMatch, "There is no fight of the day yet");
        if (match.AUserId == userId || match.BUserId == userId)
            throw new ArenaBetException(ArenaBetError.OwnMatch, "You can't bet on your own fight");

        var playerId = await PlayerIdAsync(userId);
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.ArenaBets.Add(new ArenaBet
        {
            WindowStart = window,
            UserId = userId,
            PlayerId = playerId,
            Side = side,
            Amount = amount,
            PlacedAt = now
        });
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new ArenaBetException(ArenaBetError.AlreadyBet, "You have already bet on this fight");
        }
        int paid = await db.Players.Where(p => p.Id == playerId && p.BloodCoins >= amount)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.BloodCoins, p => p.BloodCoins - amount));
        if (paid == 0)
            throw new ArenaBetException(ArenaBetError.NotEnoughCoins, $"You need {amount} blood coins");
        await transaction.CommitAsync();
        db.ChangeTracker.Clear();
        return await GetAsync(userId);
    }

    // Забрать выплаты за все проведённые бои разом
    public async Task<ArenaBetOverview> ClaimAsync(Guid userId)
    {
        var now = Now;
        await ResolveDueAsync(now);
        var bets = await db.ArenaBets
            .Where(b => b.UserId == userId && b.ClaimedAt == null)
            .Join(db.ArenaBetMatches.Where(m => m.Winner != null), b => b.WindowStart, m => m.WindowStart, (b, m) => b)
            .ToListAsync();
        if (bets.Count == 0)
            throw new ArenaBetException(ArenaBetError.NothingToClaim, "Nothing to claim");

        long total = 0;
        foreach (var bet in bets)
        {
            var match = await db.ArenaBetMatches.AsNoTracking().FirstAsync(m => m.WindowStart == bet.WindowStart);
            var (poolA, poolB) = await PoolsAsync(bet.WindowStart);
            bet.Payout = ArenaBetRules.PayoutFor(bet, match.Winner!.Value, poolA, poolB);
            bet.ClaimedAt = now;
            total += bet.Payout.Value;
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.SaveChangesAsync();
        var playerId = await PlayerIdAsync(userId);
        if (total > 0)
            await players.AddCurrenciesAsync(playerId, total, 0);
        await transaction.CommitAsync();
        db.ChangeTracker.Clear();
        return await GetAsync(userId);
    }

    // Пересмотреть проведённый бой: тот же сид — тот же бой
    public async Task<ArenaBetReplay> ReplayAsync(DateTime windowStart)
    {
        var window = ArenaBetRules.WindowOf(DateTime.SpecifyKind(windowStart, DateTimeKind.Utc));
        var match = await db.ArenaBetMatches.AsNoTracking().FirstOrDefaultAsync(m => m.WindowStart == window)
            ?? throw new ArenaBetException(ArenaBetError.NoMatch, "Fight not found");
        if (match.Winner == null)
            throw new ArenaBetException(ArenaBetError.NotResolved, "The fight hasn't happened yet");
        return new ArenaBetReplay
        {
            AName = match.AName,
            BName = match.BName,
            Background = PvpArena.Background,
            Combat = Simulate(match)
        };
    }

    /* ===== Бой ===== */

    private CombatResult Simulate(ArenaBetMatch match)
    {
        var world = new World();
        entityFactory.CreateSquad(world, SquadOf(match.ASquadJson), teamId: 1);
        new PvpScenario(entityFactory, SquadOf(match.BSquadJson)).SpawnDefenders(world);
        world.AddGlobalEffect(new RisingFuryArenaEffect());
        return PreparedFight.Run(new CombatEngine(new Random(match.Seed)), world, PvpArena.FightRounds);
    }

    private static List<Hero> SquadOf(string json) => new RankedSquad { HeroesJson = json }.Heroes();

    // Провести бои окон, которые уже закончились
    private async Task ResolveDueAsync(DateTime now)
    {
        var due = await db.ArenaBetMatches
            .Where(m => m.Winner == null && m.WindowStart <= now - ArenaBetRules.Window)
            .ToListAsync();
        foreach (var match in due)
        {
            var combat = Simulate(match);
            match.Winner = combat.WinningTeam;
            match.Rounds = combat.Rounds;
            match.ResolvedAt = now;
        }
        if (due.Count > 0)
        {
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Тот же бой одновременно провёл другой запрос — итог тот же (сид общий)
            }
            db.ChangeTracker.Clear();
        }
    }

    // Пара окна создаётся лениво первым запросом: соседние по мощи отряды среди сильнейших.
    // Сначала — закреплённые отряды рейтинговой арены, если их меньше двух — отряды защиты обычной
    private async Task<ArenaBetMatch?> EnsureMatchAsync(DateTime window)
    {
        var existing = await db.ArenaBetMatches.AsNoTracking().FirstOrDefaultAsync(m => m.WindowStart == window);
        if (existing != null)
            return existing;

        var candidates = await db.RankedSquads.AsNoTracking()
            .Join(db.Users, s => s.UserId, u => u.Id, (s, u) => new { s.UserId, u.Username, s.Power, s.HeroesJson })
            .OrderByDescending(x => x.Power)
            .Take(ArenaBetRules.CandidatePool)
            .ToListAsync();
        var pool = candidates.Select(c => (c.UserId, c.Username, c.Power, Json: c.HeroesJson)).ToList();

        if (pool.Count < 2)
        {
            var casual = (await pvp.GetCandidatesAsync(Guid.Empty))
                .OrderByDescending(c => c.Power)
                .Take(ArenaBetRules.CandidatePool)
                .ToList();
            var squads = await pvp.GetDefenseSquadsAsync(casual.Select(c => c.PlayerId).ToList());
            pool = casual
                .Where(c => squads.ContainsKey(c.PlayerId))
                .Select(c => (c.UserId, c.Username, c.Power, Json: RankedSquad.Serialize(squads[c.PlayerId])))
                .ToList();
        }
        if (pool.Count < 2)
            return null;

        var random = new Random((int)(window.Ticks / ArenaBetRules.Window.Ticks));
        int first = random.Next(pool.Count - 1);
        var (a, b) = random.Next(2) == 0 ? (pool[first], pool[first + 1]) : (pool[first + 1], pool[first]);
        var match = new ArenaBetMatch
        {
            WindowStart = window,
            AUserId = a.UserId,
            BUserId = b.UserId,
            AName = a.Username,
            BName = b.Username,
            ASquadJson = a.Json,
            BSquadJson = b.Json,
            APower = a.Power,
            BPower = b.Power,
            Seed = random.Next()
        };
        db.ArenaBetMatches.Add(match);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            return await db.ArenaBetMatches.AsNoTracking().FirstOrDefaultAsync(m => m.WindowStart == window);
        }
        db.ChangeTracker.Clear();
        return match;
    }

    /* ===== Ответ ===== */

    private async Task<(long A, long B)> PoolsAsync(DateTime window)
    {
        var pools = await db.ArenaBets.Where(b => b.WindowStart == window)
            .GroupBy(b => b.Side)
            .Select(g => new { Side = g.Key, Sum = g.Sum(b => b.Amount) })
            .ToListAsync();
        return (pools.FirstOrDefault(p => p.Side == 1)?.Sum ?? 0, pools.FirstOrDefault(p => p.Side == 2)?.Sum ?? 0);
    }

    private async Task<ArenaBetMatchInfo> DescribeAsync(ArenaBetMatch match, Guid userId, DateTime now)
    {
        var (poolA, poolB) = await PoolsAsync(match.WindowStart);
        var counts = await db.ArenaBets.Where(b => b.WindowStart == match.WindowStart)
            .GroupBy(b => b.Side).Select(g => new { Side = g.Key, Count = g.Count() }).ToListAsync();
        var bet = await db.ArenaBets.AsNoTracking().FirstOrDefaultAsync(b => b.WindowStart == match.WindowStart && b.UserId == userId);

        var world = new World();
        entityFactory.CreateSquad(world, SquadOf(match.ASquadJson), teamId: 1);
        entityFactory.CreateSquad(world, SquadOf(match.BSquadJson), teamId: 2);
        var units = CombatSnapshot.Units(world);

        return new ArenaBetMatchInfo
        {
            WindowStart = match.WindowStart,
            ClosesAt = match.WindowStart + ArenaBetRules.Window,
            Open = match.Winner == null && now < match.WindowStart + ArenaBetRules.Window,
            A = new ArenaBetSideInfo
            {
                UserId = match.AUserId, Name = match.AName, Power = match.APower, Pool = poolA,
                Bettors = counts.FirstOrDefault(c => c.Side == 1)?.Count ?? 0,
                Multiplier = ArenaBetRules.MultiplierFor(poolA, poolB),
                Units = units.Where(u => u.Team == 1).ToList()
            },
            B = new ArenaBetSideInfo
            {
                UserId = match.BUserId, Name = match.BName, Power = match.BPower, Pool = poolB,
                Bettors = counts.FirstOrDefault(c => c.Side == 2)?.Count ?? 0,
                Multiplier = ArenaBetRules.MultiplierFor(poolB, poolA),
                Units = units.Where(u => u.Team == 2).ToList()
            },
            Winner = match.Winner,
            Rounds = match.Rounds,
            MyBet = bet == null ? null : new ArenaMyBetInfo
            {
                Side = bet.Side,
                Amount = bet.Amount,
                Payout = bet.Payout ?? (match.Winner is { } w ? ArenaBetRules.PayoutFor(bet, w, poolA, poolB) : null),
                Claimed = bet.ClaimedAt != null
            }
        };
    }

    private async Task<long> ClaimableAsync(Guid userId)
    {
        var bets = await db.ArenaBets.AsNoTracking()
            .Where(b => b.UserId == userId && b.ClaimedAt == null)
            .Join(db.ArenaBetMatches.Where(m => m.Winner != null), b => b.WindowStart, m => m.WindowStart, (b, m) => new { Bet = b, Winner = m.Winner!.Value })
            .ToListAsync();
        long total = 0;
        foreach (var x in bets)
        {
            var (poolA, poolB) = await PoolsAsync(x.Bet.WindowStart);
            total += ArenaBetRules.PayoutFor(x.Bet, x.Winner, poolA, poolB);
        }
        return total;
    }

    private async Task<Guid> PlayerIdAsync(Guid userId) =>
        await db.Users.Where(u => u.Id == userId).Select(u => u.PlayerId).FirstOrDefaultAsync();
}
