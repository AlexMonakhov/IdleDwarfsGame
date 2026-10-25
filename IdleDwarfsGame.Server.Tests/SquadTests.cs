using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SquadTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly HeroesRepository _repository;
    private readonly Guid _playerId = Guid.NewGuid();

    public SquadTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
        _repository = new HeroesRepository(_context);

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester" });
        _context.Heroes.AddRange(StarterSquad.Create(_playerId));
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void StarterSquad_IsFiveBasicLevelOneDwarfsInSquad()
    {
        var squad = StarterSquad.Create(_playerId, new Random(42));

        Assert.Equal(StarterSquad.Size, squad.Count);
        Assert.All(squad, h =>
        {
            Assert.Equal(_playerId, h.PlayerId);
            Assert.Equal("Dwarf", h.UnitType);
            Assert.Equal(HeroLevels.Min, h.Level);
            Assert.Equal(HeroElement.Basic, h.Element);
        });
        Assert.Equal(new int?[] { 1, 2, 3, 4, 5 }, squad.Select(h => h.PlaceInSquad));
        Assert.Equal(squad.Count, squad.Select(h => h.Name).Distinct().Count());
    }

    [Fact]
    public async Task SaveSquad_ReordersAndRemovesHeroes()
    {
        var heroes = await _repository.GetByPlayerAsync(_playerId);
        var ordered = heroes.OrderBy(h => h.PlaceInSquad).ToList();

        // Последний встаёт вперёд, второй уходит из отряда, последний слот пустой
        await _repository.SaveSquadAsync(_playerId, [ordered[4].Id, ordered[0].Id, ordered[2].Id, ordered[3].Id, null]);

        var squad = await _repository.GetSquadAsync(_playerId);
        Assert.Equal(new[] { ordered[4].Id, ordered[0].Id, ordered[2].Id, ordered[3].Id }, squad.Select(h => h.Id));
        Assert.Equal(new int?[] { 1, 2, 3, 4 }, squad.Select(h => h.PlaceInSquad));
        Assert.Null((await _repository.GetByIdAsync(ordered[1].Id))!.PlaceInSquad);
    }

    [Fact]
    public async Task SaveSquad_RejectsInvalidInput()
    {
        var heroId = (await _repository.GetByPlayerAsync(_playerId)).First().Id;
        var demoHeroId = HeroSeedData.Heroes.First(h => h.PlayerId == HeroSeedData.DemoPlayerId).Id;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveSquadAsync(_playerId, [null, null]));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveSquadAsync(_playerId, [heroId, heroId]));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveSquadAsync(_playerId, [heroId, demoHeroId]));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.SaveSquadAsync(_playerId, Enumerable.Repeat<Guid?>(null, StarterSquad.Size).Append(heroId).ToList()));
    }
}
