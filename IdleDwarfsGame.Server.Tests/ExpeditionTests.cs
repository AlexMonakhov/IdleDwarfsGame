using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ExpeditionTests : IDisposable
{
    private static readonly EntityFactory Factory = new();

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(new DateTime(2026, 10, 25, 7, 30, 0, DateTimeKind.Utc));
    private readonly ExpeditionService _service;
    private readonly HeroesRepository _heroes;
    private readonly Guid _playerId = Guid.NewGuid();
    // Герои в инвентаре (не в отряде) — только их можно отправлять
    private readonly List<Hero> _reserve;

    public ExpeditionTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _reserve = StarterSquad.Create(_playerId);
        _reserve.ForEach(h => h.PlaceInSquad = null);

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester" });
        _context.Heroes.AddRange(StarterSquad.Create(_playerId));
        _context.Heroes.AddRange(_reserve);
        _context.SaveChanges();

        _heroes = new HeroesRepository(_context);
        _service = new ExpeditionService(
            new ExpeditionRepository(_context), _heroes, new PlayerRepository(_context), Factory, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Reward_GrowsWithPower_AndPerHourRateDropsWithDuration()
    {
        Assert.Equal(20, ExpeditionService.RewardFor(1, 0).Gold);
        Assert.Equal(10, ExpeditionService.RewardFor(1, 0).Experience);
        Assert.True(ExpeditionService.RewardFor(1, 800).Gold > ExpeditionService.RewardFor(1, 400).Gold);

        double PerHour(int hours) => ExpeditionService.RewardFor(hours, 800).Gold / (double)hours;
        Assert.True(PerHour(1) > PerHour(4));
        Assert.True(PerHour(4) > PerHour(16));
        // Но в сумме длинное задание всё равно выгоднее короткого
        Assert.True(ExpeditionService.RewardFor(16, 800).Gold > ExpeditionService.RewardFor(4, 800).Gold);
    }

    [Fact]
    public async Task Board_HasOneOfferPerDuration_AndIsStableWithinWindow()
    {
        var board = await _service.GetBoardAsync(_playerId);

        Assert.Equal(ExpeditionService.Durations, board.Expeditions.Select(e => e.DurationHours));
        Assert.All(board.Expeditions, e => Assert.Equal(ExpeditionStatus.Available, e.Status));
        Assert.Equal(new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc), board.RefreshAt);

        _clock.Advance(TimeSpan.FromHours(4));
        var again = await _service.GetBoardAsync(_playerId);
        Assert.Equal(board.Expeditions.Select(e => e.Id), again.Expeditions.Select(e => e.Id));
    }

    [Fact]
    public async Task Board_Refresh_ReplacesOffers_ButKeepsStartedExpeditions()
    {
        var board = await _service.GetBoardAsync(_playerId);
        var longest = board.Expeditions.Single(e => e.DurationHours == 16);
        await _service.StartAsync(_playerId, longest.Id, [_reserve[0].Id]);

        _clock.Advance(ExpeditionService.BoardLifetime);
        var refreshed = await _service.GetBoardAsync(_playerId);

        // Новая доска из трёх заданий + задание со старой доски, которое ещё идёт
        Assert.Equal(4, refreshed.Expeditions.Count);
        Assert.Contains(refreshed.Expeditions, e => e.Id == longest.Id && e.Status == ExpeditionStatus.InProgress);
        Assert.DoesNotContain(refreshed.Expeditions, e => board.Expeditions.Any(old => old.Id == e.Id && old.Id != longest.Id));
        Assert.Equal(3, await _context.Expeditions.CountAsync(e => e.Status == ExpeditionStatus.Available));
    }

    [Fact]
    public async Task Start_LocksRewardByPower_AndMarksHeroesBusy()
    {
        var offer = (await _service.GetBoardAsync(_playerId)).Expeditions.First();
        var sent = _reserve.Take(ExpeditionService.MaxHeroes).ToList();

        var started = await _service.StartAsync(_playerId, offer.Id, sent.Select(h => h.Id).ToList());

        int power = sent.Sum(h => CombatPower.OfHero(Factory, h).Total);
        Assert.Equal(ExpeditionStatus.InProgress, started.Status);
        Assert.Equal(power, started.Power);
        Assert.Equal(ExpeditionService.RewardFor(offer.DurationHours, power).Gold, started.Reward.Gold);
        Assert.Equal(_clock.UtcNow.AddHours(offer.DurationHours), started.EndsAt);
        Assert.Equal(sent.Select(h => h.Id), started.HeroIds);

        var busy = await new ExpeditionRepository(_context).GetBusyHeroIdsAsync(_playerId);
        Assert.Equal(sent.Select(h => h.Id).ToHashSet(), busy);
    }

    [Fact]
    public async Task Start_RejectsInvalidHeroes()
    {
        var offers = (await _service.GetBoardAsync(_playerId)).Expeditions;
        var squadHero = (await _heroes.GetSquadAsync(_playerId)).First();
        var demoHero = HeroSeedData.Heroes.First(h => h.PlayerId == HeroSeedData.DemoPlayerId);

        Task Start(params Guid[] ids) => _service.StartAsync(_playerId, offers[0].Id, ids);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Start());
        await Assert.ThrowsAsync<InvalidOperationException>(() => Start(_reserve.Take(5).Select(h => h.Id).ToArray()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Start(_reserve[0].Id, _reserve[0].Id));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Start(squadHero.Id));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Start(demoHero.Id));

        // Герой уже на другом задании, и одно задание нельзя запустить дважды
        await _service.StartAsync(_playerId, offers[1].Id, [_reserve[1].Id]);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Start(_reserve[1].Id));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartAsync(_playerId, offers[1].Id, [_reserve[2].Id]));
    }

    [Fact]
    public async Task Start_RejectsOfferFromPreviousBoard()
    {
        var offer = (await _service.GetBoardAsync(_playerId)).Expeditions.First();
        _clock.Advance(ExpeditionService.BoardLifetime);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartAsync(_playerId, offer.Id, [_reserve[0].Id]));
    }

    [Fact]
    public async Task Claim_OnlyAfterEnd_CreditsResourcesOnce_AndFreesHeroes()
    {
        var offer = (await _service.GetBoardAsync(_playerId)).Expeditions.Single(e => e.DurationHours == 1);
        var started = await _service.StartAsync(_playerId, offer.Id, [_reserve[0].Id, _reserve[1].Id]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ClaimAsync(_playerId, offer.Id));

        _clock.Advance(TimeSpan.FromHours(1));
        var board = await _service.GetBoardAsync(_playerId);
        Assert.Equal(ExpeditionStatus.Ready, board.Expeditions.Single(e => e.Id == offer.Id).Status);

        var result = await _service.ClaimAsync(_playerId, offer.Id);
        Assert.Equal(started.Reward.Gold, result.Reward.Gold);
        Assert.Equal(started.Reward.Gold, result.Resources.Gold);
        Assert.Equal(started.Reward.Experience, result.Resources.Experience);
        Assert.Empty(await new ExpeditionRepository(_context).GetBusyHeroIdsAsync(_playerId));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ClaimAsync(_playerId, offer.Id));
        Assert.Equal(started.Reward.Gold, (await new PlayerRepository(_context).GetResourcesAsync(_playerId))!.Gold);

        // До смены доски выполненное задание остаётся на ней, но запустить его снова нельзя
        board = await _service.GetBoardAsync(_playerId);
        Assert.Equal(ExpeditionStatus.Claimed, board.Expeditions.Single(e => e.Id == offer.Id).Status);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartAsync(_playerId, offer.Id, [_reserve[0].Id]));
    }

    [Fact]
    public async Task SaveSquad_RejectsHeroesOnExpedition()
    {
        var offer = (await _service.GetBoardAsync(_playerId)).Expeditions.First();
        await _service.StartAsync(_playerId, offer.Id, [_reserve[0].Id]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _heroes.SaveSquadAsync(_playerId, [_reserve[0].Id]));
        await _heroes.SaveSquadAsync(_playerId, [_reserve[1].Id]);
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
