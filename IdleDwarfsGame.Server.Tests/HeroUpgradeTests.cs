using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class HeroUpgradeTests : IDisposable
{
    private static readonly EntityFactory Factory = new();

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly HeroesRepository _heroes;
    private readonly HeroUpgradeService _service;
    private readonly Guid _playerId = Guid.NewGuid();
    private readonly Hero _hero;

    public HeroUpgradeTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        var squad = StarterSquad.Create(_playerId);
        _hero = squad[0];

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester", Gold = 100_000, Experience = 100_000 });
        _context.Heroes.AddRange(squad);
        _context.SaveChanges();

        _heroes = new HeroesRepository(_context);
        _service = new HeroUpgradeService(
            _heroes, new PlayerRepository(_context), new ExpeditionRepository(_context), Factory);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Cost_GrowsWithLevelAndRank_AndStopsAtMax()
    {
        var first = HeroUpgradeService.CostFor(rank: 1, level: 1)!;
        Assert.Equal(30, first.Gold);
        Assert.Equal(15, first.Experience);

        Assert.True(HeroUpgradeService.CostFor(1, 10)!.Gold > first.Gold);
        Assert.Equal(first.Gold * 3, HeroUpgradeService.CostFor(3, 1)!.Gold);
        Assert.NotNull(HeroUpgradeService.CostFor(1, HeroUpgradeService.MaxLevel - 1));
        Assert.Null(HeroUpgradeService.CostFor(1, HeroUpgradeService.MaxLevel));
    }

    [Fact]
    public async Task Upgrade_SpendsResources_AndRaisesStatsAndPower()
    {
        var before = HeroInfo.From(Factory, _hero);
        var cost = before.Upgrade.Cost!;

        var result = await _service.UpgradeAsync(_playerId, _hero.Id);

        Assert.Equal(2, result.Hero.Upgrade.Level);
        Assert.Equal(100_000 - cost.Gold, result.Resources.Gold);
        Assert.Equal(100_000 - cost.Experience, result.Resources.Experience);

        // Превью перед прокачкой совпадает с тем, что получилось после
        Assert.Equal(before.Upgrade.Next!.MaxHp, result.Hero.MaxHp);
        Assert.Equal(before.Upgrade.Next.Attack, result.Hero.Attack);
        Assert.Equal(before.Upgrade.Next.Power, result.Hero.Power.Total);
        Assert.True(result.Hero.Power.Total > before.Power.Total);
        Assert.True(result.Hero.Power.Level > 0);

        var stored = await _context.Heroes.AsNoTracking().SingleAsync(h => h.Id == _hero.Id);
        Assert.Equal(2, stored.UpgradeLevel);
    }

    [Fact]
    public async Task Upgrade_AppliesToCombatUnit()
    {
        await _service.UpgradeAsync(_playerId, _hero.Id);
        var hero = await _context.Heroes.AsNoTracking().SingleAsync(h => h.Id == _hero.Id);

        var world = new World();
        int id = Factory.CreateUnit(world, hero, teamId: 1, position: 1);

        Assert.Equal(HeroUpgradeService.ScaleStat(hero.MaxHp, 2), world.Health[id].Max);
        Assert.Equal(HeroUpgradeService.ScaleStat(hero.Attack, 2), world.Attack[id].Damage);
        Assert.True(world.Health[id].Max > hero.MaxHp);
    }

    [Fact]
    public async Task Upgrade_WithoutEnoughGold_ChangesNothing()
    {
        await SetResources(gold: 10, experience: 100_000);

        var error = await Assert.ThrowsAsync<HeroUpgradeException>(() => _service.UpgradeAsync(_playerId, _hero.Id));

        Assert.Equal(HeroUpgradeError.NotEnoughGold, error.Code);
        await AssertUnchanged(gold: 10, experience: 100_000);
    }

    [Fact]
    public async Task Upgrade_WithoutEnoughExperience_ChangesNothing()
    {
        await SetResources(gold: 100_000, experience: 5);

        var error = await Assert.ThrowsAsync<HeroUpgradeException>(() => _service.UpgradeAsync(_playerId, _hero.Id));

        Assert.Equal(HeroUpgradeError.NotEnoughExperience, error.Code);
        await AssertUnchanged(gold: 100_000, experience: 5);
    }

    [Fact]
    public async Task Upgrade_StopsAtMaxLevel()
    {
        for (int level = HeroUpgradeService.MinLevel; level < HeroUpgradeService.MaxLevel; level++)
            await _service.UpgradeAsync(_playerId, _hero.Id);

        var error = await Assert.ThrowsAsync<HeroUpgradeException>(() => _service.UpgradeAsync(_playerId, _hero.Id));
        Assert.Equal(HeroUpgradeError.MaxLevel, error.Code);

        var info = HeroInfo.From(Factory, await _context.Heroes.AsNoTracking().SingleAsync(h => h.Id == _hero.Id));
        Assert.Equal(HeroUpgradeService.MaxLevel, info.Upgrade.Level);
        Assert.Null(info.Upgrade.Cost);
        Assert.Null(info.Upgrade.Next);
    }

    [Fact]
    public async Task Upgrade_OfAnotherPlayersHero_IsRejected()
    {
        var error = await Assert.ThrowsAsync<HeroUpgradeException>(() => _service.UpgradeAsync(Guid.NewGuid(), _hero.Id));

        Assert.Equal(HeroUpgradeError.HeroNotFound, error.Code);
        await AssertUnchanged(gold: 100_000, experience: 100_000);
    }

    [Fact]
    public async Task Upgrade_FromStaleLevel_DoesNotChargeTwice()
    {
        // Второй клик пришёл с уже устаревшим уровнем: ни уровень, ни ресурсы не должны поменяться второй раз
        var stale = _hero.WithUpgradeLevel(1);
        await _service.UpgradeAsync(_playerId, _hero.Id);
        var afterFirst = await new PlayerRepository(_context).GetResourcesAsync(_playerId);

        var cost = HeroUpgradeService.CostFor(stale.Level, stale.UpgradeLevel)!;
        Assert.Equal(HeroUpgradeOutcome.LevelChanged, await _heroes.UpgradeAsync(stale, cost));

        var stored = await _context.Heroes.AsNoTracking().SingleAsync(h => h.Id == _hero.Id);
        Assert.Equal(2, stored.UpgradeLevel);
        var resources = await new PlayerRepository(_context).GetResourcesAsync(_playerId);
        Assert.Equal(afterFirst!.Gold, resources!.Gold);
        Assert.Equal(afterFirst.Experience, resources.Experience);
    }

    [Fact]
    public async Task Upgrade_WhenResourcesRunOutMidway_RollsBackLevel()
    {
        await SetResources(gold: 0, experience: 0);

        // Минуем проверку сервиса: репозиторий сам не даёт уйти в минус и откатывает повышение уровня
        var cost = HeroUpgradeService.CostFor(_hero.Level, _hero.UpgradeLevel)!;
        Assert.Equal(HeroUpgradeOutcome.NotEnoughResources, await _heroes.UpgradeAsync(_hero, cost));

        await AssertUnchanged(gold: 0, experience: 0);
    }

    private Task SetResources(long gold, long experience) => _context.Players
        .Where(p => p.Id == _playerId)
        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, gold).SetProperty(p => p.Experience, experience));

    private async Task AssertUnchanged(long gold, long experience)
    {
        var stored = await _context.Heroes.AsNoTracking().SingleAsync(h => h.Id == _hero.Id);
        Assert.Equal(HeroUpgradeService.MinLevel, stored.UpgradeLevel);

        var resources = await new PlayerRepository(_context).GetResourcesAsync(_playerId);
        Assert.Equal(gold, resources!.Gold);
        Assert.Equal(experience, resources.Experience);
    }
}
