using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class RankedPvpTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc));
    private readonly PvpService _casual;
    private readonly RankedPvpService _service;

    public RankedPvpTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        var heroes = new HeroesRepository(_context);
        var factory = new EntityFactory();
        var combat = new CombatService(new WorldFactory(heroes, factory), new CombatEngine(new Random(1)), []);
        var pvp = new PvpRepository(_context);
        var users = new UserRepository(_context);
        _casual = new PvpService(pvp, users, heroes, factory, combat, _clock);
        _service = new RankedPvpService(pvp, users, heroes, factory, combat, _casual, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task WithoutLockedSquad_NoOpponentsAndNoFights()
    {
        var alice = AddUser("Alice");
        var bob = AddUser("Bob");
        _context.SaveChanges();
        await _service.LockSquadAsync(bob.Id);

        var overview = await _service.GetOverviewAsync(alice.Id);

        Assert.Null(overview.Squad);
        Assert.True(overview.CanLockSquad);
        Assert.NotEmpty(overview.AccountSquad);
        Assert.Empty(overview.Opponents);
        Assert.Equal(RankedArena.StartRating, overview.MyRating);
        Assert.Equal(PvpError.NoRankedSquad, (await Assert.ThrowsAsync<PvpException>(() => _service.FightAsync(alice.Id, bob.Id))).Code);
    }

    [Fact]
    public async Task Squad_CanBeChangedOnceADay()
    {
        var alice = AddUser("Alice");
        _context.SaveChanges();

        var locked = await _service.LockSquadAsync(alice.Id);
        Assert.NotNull(locked.Squad);
        Assert.False(locked.CanLockSquad);
        Assert.Equal(new DateTime(2026, 10, 29, 0, 0, 0, DateTimeKind.Utc), locked.Squad!.CanChangeAt);
        Assert.Equal(PvpError.SquadChangeCooldown, (await Assert.ThrowsAsync<PvpException>(() => _service.LockSquadAsync(alice.Id))).Code);

        // Ровно в 00:00 UTC — снова можно
        _clock.Advance(TimeSpan.FromHours(12));
        Assert.True((await _service.GetOverviewAsync(alice.Id)).CanLockSquad);
        Assert.False((await _service.LockSquadAsync(alice.Id)).CanLockSquad);
    }

    [Fact]
    public async Task LockedSquad_IsASnapshot_AccountChangesDoNotAffectIt()
    {
        var alice = AddUser("Alice");
        _context.SaveChanges();

        var locked = await _service.LockSquadAsync(alice.Id);
        int lockedPower = locked.Squad!.Power;

        // Прокачка героев и уход героя из отряда аккаунта
        var heroes = _context.Heroes.Where(h => h.PlayerId == alice.PlayerId).ToList();
        foreach (var hero in heroes)
            hero.Attack *= 3;
        heroes.First().PlaceInSquad = null;
        _context.SaveChanges();

        var after = await _service.GetOverviewAsync(alice.Id);
        Assert.Equal(lockedPower, after.Squad!.Power);
        Assert.Equal(locked.Squad.Units.Count, after.Squad.Units.Count);
        Assert.NotEqual(lockedPower, after.AccountPower);
        Assert.Equal(locked.Squad.Units.Count - 1, after.AccountSquad.Count);
    }

    [Fact]
    public async Task Fight_UsesSnapshots_AndChangesOnlyRankedRating()
    {
        var alice = AddUser("Alice");
        var bob = AddUser("Bob", attackFactor: 3);
        _context.SaveChanges();
        await _service.LockSquadAsync(alice.Id);
        await _service.LockSquadAsync(bob.Id);

        // После закрепления Алиса стала втрое сильнее, а Боб — слабее: в бою этого не видно
        foreach (var hero in _context.Heroes.Where(h => h.PlayerId == alice.PlayerId))
            hero.Attack *= 3;
        foreach (var hero in _context.Heroes.Where(h => h.PlayerId == bob.PlayerId))
            hero.Attack = 1;
        _context.SaveChanges();

        var opponent = Assert.Single((await _service.GetOverviewAsync(alice.Id)).Opponents);
        Assert.Equal(bob.Id, opponent.UserId);
        Assert.Equal(RankedArena.StartRating, opponent.Rating);
        Assert.Equal(PvpRating.WinPoints, opponent.WinPoints);

        var fight = await _service.FightAsync(alice.Id, bob.Id);

        Assert.False(fight.Won);
        Assert.Equal(-PvpRating.LossPoints, fight.Points);
        Assert.All(fight.Seasons, s => Assert.Equal(RankedArena.StartRating - PvpRating.LossPoints, s.Points));

        var overview = await _service.GetOverviewAsync(alice.Id);
        Assert.Equal(RankedArena.StartRating - PvpRating.LossPoints, overview.MyRating);
        Assert.All(overview.Seasons, s =>
        {
            Assert.Equal(1, s.Losses);
            Assert.Equal(1, s.Rank);
        });
        var battle = Assert.Single(overview.History);
        Assert.Equal(RankedArena.StartRating, battle.MyRating);
        Assert.Equal(RankedArena.StartRating, battle.OpponentRating);

        // Обычная арена ничего не знает об этом бое
        var casual = await _casual.GetOverviewAsync(alice.Id);
        Assert.Empty(casual.History);
        Assert.All(casual.Seasons, s => Assert.Null(s.Rank));
    }

    [Fact]
    public async Task Matchmaking_ByRatingWithinFivePercent()
    {
        var alice = AddUser("Alice");
        var near = AddUser("Near");
        var far = AddUser("Far");
        _context.SaveChanges();
        foreach (var user in new[] { alice, near, far })
            await _service.LockSquadAsync(user.Id);

        // Рейтинги месячного сезона: 1000 / 1040 (+4%) / 1100 (+10%)
        var month = PvpSeasons.StartOf(RankedArena.MatchSeason, _clock.UtcNow);
        _context.PvpSeasonScores.AddRange(
            Score(near.Id, month, 1040),
            Score(far.Id, month, 1100));
        _context.SaveChanges();

        // Шести соперников нет — окно расширилось бы до Far; поэтому добавляем ещё четверых в окне
        for (int i = 1; i <= 4; i++)
        {
            var rival = AddUser($"Rival{i}");
            _context.SaveChanges();
            await _service.LockSquadAsync(rival.Id);
            _context.PvpSeasonScores.Add(Score(rival.Id, month, 980 + i * 10));
            _context.SaveChanges();
        }

        var overview = await _service.GetOverviewAsync(alice.Id);

        Assert.Equal(RankedArena.StartSpreadPercent, overview.RatingSpreadPercent);
        Assert.Equal(PvpMatchmaking.OpponentCount, overview.Opponents.Count);
        Assert.Contains(overview.Opponents, o => o.UserId == near.Id);
        Assert.DoesNotContain(overview.Opponents, o => o.UserId == far.Id);
        Assert.Equal(4.0, overview.Opponents.Single(o => o.UserId == near.Id).RatingDiffPercent);
        Assert.Equal(PvpError.NotAnOpponent, (await Assert.ThrowsAsync<PvpException>(() => _service.FightAsync(alice.Id, far.Id))).Code);
    }

    /* ===== Помощники ===== */

    private static PvpSeasonScore Score(Guid userId, DateTime start, int points) => new()
    {
        UserId = userId,
        Mode = PvpMode.Ranked,
        Type = RankedArena.MatchSeason,
        Start = start,
        Points = points,
        UpdatedAt = start
    };

    private User AddUser(string username, double attackFactor = 1)
    {
        var player = new Player { Id = Guid.NewGuid(), Name = username };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            NormalizedUsername = UserRepository.Normalize(username),
            Email = $"{username}@test.local",
            NormalizedEmail = UserRepository.Normalize($"{username}@test.local"),
            PlayerId = player.Id,
            Player = player
        };
        _context.Users.Add(user);

        var squad = StarterSquad.Create(player.Id, new Random(1));
        foreach (var hero in squad)
            hero.Attack = (int)Math.Round(hero.Attack * attackFactor);
        _context.Heroes.AddRange(squad);
        return user;
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
