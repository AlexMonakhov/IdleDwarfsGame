using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class PvpTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    // Среда: неделя началась в понедельник 26-го, месяц — 1-го
    private readonly TestClock _clock = new(new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc));
    private readonly PvpService _service;

    public PvpTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        var heroes = new HeroesRepository(_context);
        var factory = new EntityFactory();
        var combat = new CombatService(new WorldFactory(heroes, factory), new CombatEngine(new Random(1)), []);
        _service = new PvpService(new PvpRepository(_context), new UserRepository(_context), heroes, factory, combat, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    /* ===== Правила ===== */

    [Fact]
    public void Matchmaking_WidensSpreadByOnePercent_UntilFiveOpponents()
    {
        // Мощь 1000; соперники отличаются на 2, 5, 9, 12, 13.5 и 30%
        var candidates = new[] { 1020, 950, 1090, 880, 1135, 1300 }.Select(Candidate).ToList();

        var pick = PvpMatchmaking.Choose(candidates, c => c.Id, c => c.Power, 1000, seed: 1);

        Assert.Equal(14, pick.SpreadPercent);
        Assert.Equal([1020, 950, 1090, 880, 1135], pick.Opponents.Select(c => c.Power));
    }

    [Fact]
    public void Matchmaking_KeepsTenPercent_AndPicksSameFiveForSameSeed()
    {
        var candidates = Enumerable.Range(0, 12).Select(i => Candidate(950 + i * 9)).Append(Candidate(2000)).ToList();

        var first = PvpMatchmaking.Choose(candidates, c => c.Id, c => c.Power, 1000, seed: 7);
        var again = PvpMatchmaking.Choose(candidates.AsEnumerable().Reverse(), c => c.Id, c => c.Power, 1000, seed: 7);

        Assert.Equal(PvpMatchmaking.StartSpreadPercent, first.SpreadPercent);
        Assert.Equal(PvpMatchmaking.OpponentCount, first.Opponents.Count);
        Assert.All(first.Opponents, c => Assert.InRange(c.Power, 900, 1100));
        Assert.Equal(first.Opponents.Select(c => c.Id), again.Opponents.Select(c => c.Id));
    }

    [Fact]
    public void Rating_PaysMoreForStrongerOpponents_AndTakesMoreForWeaker()
    {
        Assert.Equal(PvpRating.WinPoints, PvpRating.WinFor(1000, 1000));
        Assert.Equal(PvpRating.LossPoints, PvpRating.LossFor(1000, 1000));

        Assert.True(PvpRating.WinFor(1000, 1100) > PvpRating.WinFor(1000, 900));
        Assert.True(PvpRating.LossFor(1000, 900) > PvpRating.LossFor(1000, 1100));

        Assert.Equal(PvpRating.MaxPoints, PvpRating.WinFor(1000, 5000));
        Assert.Equal(PvpRating.MinPoints, PvpRating.LossFor(1000, 5000));
        Assert.Equal(-PvpRating.LossFor(1000, 900), PvpRating.DeltaFor(false, 1000, 900));
    }

    [Fact]
    public void Seasons_StartOnDayMondayAndFirstOfMonth()
    {
        var now = new DateTime(2026, 11, 1, 15, 30, 0, DateTimeKind.Utc); // воскресенье

        Assert.Equal(new DateTime(2026, 11, 1), PvpSeasons.StartOf(PvpSeasonType.Daily, now));
        Assert.Equal(new DateTime(2026, 10, 26), PvpSeasons.StartOf(PvpSeasonType.Weekly, now));
        Assert.Equal(new DateTime(2026, 11, 1), PvpSeasons.StartOf(PvpSeasonType.Monthly, now));
        Assert.Equal(new DateTime(2026, 12, 1), PvpSeasons.EndOf(PvpSeasonType.Monthly, new DateTime(2026, 11, 1)));
    }

    [Fact]
    public void RisingFury_RaisesAllDamageByTenPercentEveryFiveRounds()
    {
        // Два непробиваемых юнита без заклинаний: видно только, как растёт урон базовой атаки
        var world = new World();
        var factory = new EntityFactory();
        factory.CreateUnit(world, Tank(speed: 9), teamId: 1, position: 1);
        factory.CreateUnit(world, Tank(speed: 1), teamId: 2, position: 1);
        world.AddGlobalEffect(new RisingFuryArenaEffect());

        var logs = new CombatEngine(new Random(1)).Run(world, maxRounds: 12).Logs;

        int DamageOn(int round) => logs.First(l => l.Round == round && l.EventType == CombatEventType.Attack).Damage;
        Assert.Equal(100, DamageOn(5));
        Assert.Equal(110, DamageOn(6));
        Assert.Equal(120, DamageOn(11));
        Assert.Equal([10, 20], logs.Where(l => l.EventType == CombatEventType.ArenaBuff).Select(l => l.DamageBonusPercent!.Value));
    }

    /* ===== Бои ===== */

    [Fact]
    public async Task Win_RemovesOpponentForTheDay_AndAddsPointsInEverySeason()
    {
        var alice = AddUser("Alice", attackFactor: 3);
        var bob = AddUser("Bob");
        _context.SaveChanges();

        var before = await _service.GetOverviewAsync(alice.Id);
        var opponent = Assert.Single(before.Opponents);
        Assert.Equal(bob.Id, opponent.UserId);
        Assert.Equal(before.MyPower, opponent.Forecast.PlayerPower);
        Assert.Equal(PvpArena.Background, before.Arena.Background);

        var fight = await _service.FightAsync(alice.Id, bob.Id);

        Assert.True(fight.Won);
        Assert.Equal(opponent.WinPoints, fight.Points);
        Assert.All(fight.Seasons, s => Assert.Equal(fight.Points, s.Points));

        var after = await _service.GetOverviewAsync(alice.Id);
        Assert.Empty(after.Opponents);
        Assert.Equal(1, after.BeatenToday);
        Assert.All(after.Seasons, s =>
        {
            Assert.Equal(fight.Points, s.Points);
            Assert.Equal(1, s.Wins);
            Assert.Equal(1, s.Rank);
            Assert.True(Assert.Single(s.Top).IsMe);
        });
        Assert.True(Assert.Single(after.History).IsAttack);

        Assert.Equal(PvpError.AlreadyBeaten, (await Assert.ThrowsAsync<PvpException>(() => _service.FightAsync(alice.Id, bob.Id))).Code);

        // Защитник видит бой в истории, его очки не меняются
        var defence = Assert.Single((await _service.GetOverviewAsync(bob.Id)).History);
        Assert.False(defence.IsAttack);
        Assert.False(defence.Won);
        Assert.Equal(0, defence.Points);

        // Новые сутки — соперник снова в подборе, а дневной сезон начинается с нуля
        _clock.Advance(TimeSpan.FromDays(1));
        var nextDay = await _service.GetOverviewAsync(alice.Id);
        Assert.Equal(bob.Id, Assert.Single(nextDay.Opponents).UserId);
        Assert.Equal(0, nextDay.Seasons.Single(s => s.Type == PvpSeasonType.Daily).Points);
        Assert.Null(nextDay.Seasons.Single(s => s.Type == PvpSeasonType.Daily).Rank);
        Assert.Equal(fight.Points, nextDay.Seasons.Single(s => s.Type == PvpSeasonType.Weekly).Points);
    }

    [Fact]
    public async Task Loss_KeepsOpponent_AndPointsDoNotGoBelowZero()
    {
        var alice = AddUser("Alice");
        var bob = AddUser("Bob", attackFactor: 3);
        _context.SaveChanges();

        var fight = await _service.FightAsync(alice.Id, bob.Id);

        Assert.False(fight.Won);
        Assert.True(fight.Points < 0);
        Assert.All(fight.Seasons, s => Assert.Equal(0, s.Points));

        var overview = await _service.GetOverviewAsync(alice.Id);
        Assert.Equal(bob.Id, Assert.Single(overview.Opponents).UserId);
        Assert.All(overview.Seasons, s => Assert.Equal(1, s.Losses));
    }

    [Fact]
    public async Task Fight_OnlyAgainstMatchedOpponents()
    {
        var alice = AddUser("Alice");
        // Шесть равных по мощи соперников: в подбор попадут пятеро, шестой — нет
        var others = Enumerable.Range(1, 6).Select(i => AddUser($"Rival{i}")).ToList();
        _context.SaveChanges();

        var matched = (await _service.GetOverviewAsync(alice.Id)).Opponents.Select(o => o.UserId).ToHashSet();
        Assert.Equal(PvpMatchmaking.OpponentCount, matched.Count);
        var outsider = others.Single(u => !matched.Contains(u.Id));

        Assert.Equal(PvpError.NotAnOpponent, (await Assert.ThrowsAsync<PvpException>(() => _service.FightAsync(alice.Id, outsider.Id))).Code);
        Assert.Equal(PvpError.OpponentNotFound, (await Assert.ThrowsAsync<PvpException>(() => _service.FightAsync(alice.Id, Guid.NewGuid()))).Code);
        Assert.Equal(PvpError.NotAnOpponent, (await Assert.ThrowsAsync<PvpException>(() => _service.FightAsync(alice.Id, alice.Id))).Code);
    }

    [Fact]
    public async Task TestAccounts_GetFullSquadsWithOneSpellEach_AndAreSeededOnce()
    {
        await TestAccountsSeed.SeedAsync(_context, new PasswordHasher<User>());
        await TestAccountsSeed.SeedAsync(_context, new PasswordHasher<User>());

        var users = await _context.Users.Where(u => u.Username.StartsWith(TestAccountsSeed.UsernamePrefix)).ToListAsync();
        Assert.Equal(TestAccountsSeed.Count, users.Count);

        var playerIds = users.Select(u => u.PlayerId).ToList();
        var heroes = await _context.Heroes.Where(h => playerIds.Contains(h.PlayerId)).ToListAsync();
        Assert.Equal(TestAccountsSeed.Count * StarterSquad.Size, heroes.Count);
        Assert.All(heroes, h =>
        {
            Assert.NotNull(h.PlaceInSquad);
            Assert.True(SpellRegistry.All.ContainsKey(Assert.Single(h.SpellIds)));
        });
        // Разные ранги героев и уровни заклинаний
        Assert.Equal(HeroLevels.Max, heroes.Select(h => h.Level).Distinct().Count());
        Assert.Equal(3, heroes.Select(h => SpellRegistry.All[h.SpellIds[0]].Level).Distinct().Count());
    }

    /* ===== Помощники ===== */

    private sealed record TestCandidate(Guid Id, int Power);

    private static TestCandidate Candidate(int power) => new(Guid.NewGuid(), power);

    private static Hero Tank(int speed) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Tank",
        MaxHp = 1_000_000,
        Attack = 100,
        Speed = speed,
        MaxEnergy = 100,
        EnergyPerAttack = 0
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
