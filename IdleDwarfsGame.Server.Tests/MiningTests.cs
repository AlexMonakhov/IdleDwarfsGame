using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class MiningTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(new DateTime(2026, 11, 2, 10, 0, 0, DateTimeKind.Utc));
    private readonly MiningService _service;
    private readonly HeroesRepository _heroes;
    private readonly MiningRepository _mining;
    private readonly Guid _playerId = Guid.NewGuid();
    // Герои в инвентаре (не в отряде) — только их можно ставить в шахты
    private readonly List<Hero> _reserve;

    public MiningTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _reserve = StarterSquad.Create(_playerId);
        _reserve.ForEach(h => h.PlaceInSquad = null);

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester", Gold = 100_000, Experience = 100_000 });
        _context.Heroes.AddRange(StarterSquad.Create(_playerId));
        _context.Heroes.AddRange(_reserve);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        _heroes = new HeroesRepository(_context);
        _mining = new MiningRepository(_context);
        _service = new MiningService(_mining, _heroes, new ExpeditionRepository(_context), _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private Task<MiningWallet> Wallet() => _mining.GetWalletAsync(_playerId);

    private async Task SetGold(long gold)
    {
        await _context.Players.Where(p => p.Id == _playerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, gold));
    }

    private static async Task<MiningError> ErrorOf(Func<Task> action) =>
        (await Assert.ThrowsAsync<MiningException>(action)).Code;

    [Fact]
    public async Task Build_FirstMineIsFree_NextCostsGold_AndPlotsAreUnique()
    {
        var first = await _service.BuildAsync(_playerId, 1, MineType.Gold);
        Assert.Equal(100_000, first.Wallet.Gold);
        Assert.Equal(1, first.Mine.Level);

        var board = await _service.GetBoardAsync(_playerId);
        Assert.Equal(MiningService.BuildGold, board.BuildCost.Gold);

        var second = await _service.BuildAsync(_playerId, 2, MineType.Ruby);
        Assert.Equal(100_000 - MiningService.BuildGold, second.Wallet.Gold);
        Assert.Equal(GemType.Ruby, second.Mine.Gem);

        Assert.Equal(MiningError.PlotTaken, await ErrorOf(() => _service.BuildAsync(_playerId, 1, MineType.Experience)));
        Assert.Equal(MiningError.PlotNotFound, await ErrorOf(() => _service.BuildAsync(_playerId, MiningService.PlotCount + 1, MineType.Gold)));
    }

    [Fact]
    public async Task Build_WithoutGold_SavesNothing()
    {
        await _service.BuildAsync(_playerId, 1, MineType.Gold);
        await SetGold(0);

        Assert.Equal(MiningError.NotEnoughGold, await ErrorOf(() => _service.BuildAsync(_playerId, 2, MineType.Gold)));
        Assert.Single(await _mining.GetMinesAsync(_playerId));
    }

    [Fact]
    public async Task Dig_CreditsResourceRightAway_AndLimitsClickRate()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;

        var dug = await _service.DigAsync(_playerId, mine.Id, 10);
        Assert.Equal(10, dug.Clicks);
        Assert.Equal(10 * mine.PerClick, dug.Mined);
        Assert.Equal(100_000 + dug.Mined, dug.Wallet.Gold);

        // Следующая пачка в ту же секунду — кликов сверх лимита не засчитываем
        var spam = await _service.DigAsync(_playerId, mine.Id, MiningService.MaxClicksPerBatch);
        Assert.Equal(0, spam.Mined);

        _clock.Advance(TimeSpan.FromSeconds(1));
        var next = await _service.DigAsync(_playerId, mine.Id, MiningService.MaxClicksPerBatch);
        Assert.Equal(MiningService.MaxClicksPerSecond, next.Clicks);
    }

    [Fact]
    public async Task Dig_CrystalMine_FillsCommonGemStack()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Sapphire)).Mine;

        await _service.DigAsync(_playerId, mine.Id, 5);
        _clock.Advance(TimeSpan.FromSeconds(1));
        var dug = await _service.DigAsync(_playerId, mine.Id, 3);

        Assert.Equal(8, dug.Wallet.Crystals[GemType.Sapphire]);
        var stack = await _context.PlayerItems.SingleAsync(i => i.PlayerId == _playerId);
        Assert.Equal((ItemType.Gem, "Sapphire", Rarity.Common, 1, 8), (stack.ItemType, stack.SubType, stack.Rarity, stack.Level, stack.Quantity));
    }

    [Fact]
    public async Task Upgrade_PaysCost_AndIsCappedByMineLevel()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var cost = MiningService.UpgradeCost(MineType.Gold, MineUpgrade.Yield, 1);

        var upgraded = await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Yield);
        Assert.Equal(100_000 - cost.Gold, upgraded.Wallet.Gold);
        Assert.Equal(MiningService.PerClick(MineType.Gold, 2), upgraded.Mine.PerClick);

        for (int level = 2; level < MiningService.UpgradesPerLevel; level++)
            await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Yield);
        Assert.Equal(MiningError.MineLevelTooLow, await ErrorOf(() => _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Yield)));

        await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Level);
        await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Yield);
    }

    [Fact]
    public async Task Upgrade_CrystalMine_NeedsItsCrystals()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Topaz)).Mine;
        var cost = MiningService.UpgradeCost(MineType.Topaz, MineUpgrade.Speed, 1);
        Assert.Equal(GemType.Topaz, cost.Gem);

        Assert.Equal(MiningError.NotEnoughCrystals, await ErrorOf(() => _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Speed)));
        // Золото за неудачную попытку не списалось
        Assert.Equal(100_000, (await Wallet()).Gold);

        await _service.DigAsync(_playerId, mine.Id, cost.Crystals);
        var upgraded = await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Speed);
        Assert.False(upgraded.Wallet.Crystals.ContainsKey(GemType.Topaz));
        Assert.Equal(2, upgraded.Mine.Upgrades.Single(u => u.Upgrade == MineUpgrade.Speed).Level);
    }

    [Fact]
    public async Task Dwarf_UnlocksWithMineLevel_FillsStorage_AndIsCollected()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var dwarf = _reserve[0];

        Assert.Equal(MiningError.DwarfLocked, await ErrorOf(() => _service.AssignDwarfAsync(_playerId, mine.Id, dwarf.Id)));
        await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Level);
        await _service.AssignDwarfAsync(_playerId, mine.Id, dwarf.Id);
        long goldBefore = (await Wallet()).Gold;

        _clock.Advance(TimeSpan.FromHours(1));
        var board = await _service.GetBoardAsync(_playerId);
        Assert.Equal(MiningService.RatePerHour(MineType.Gold, 1, 1), board.Mines[0].Stored, 6);

        // Склад полон — дальше дворф не копит
        _clock.Advance(TimeSpan.FromDays(2));
        var collected = await _service.CollectAsync(_playerId, mine.Id);
        Assert.Equal(MiningService.Capacity(MineType.Gold, 1), collected.Collected);
        Assert.Equal(goldBefore + collected.Collected, collected.Wallet.Gold);
        Assert.Equal(0, collected.Mine.Stored);

        Assert.Equal(MiningError.NothingToCollect, await ErrorOf(() => _service.CollectAsync(_playerId, mine.Id)));
    }

    [Fact]
    public async Task Dwarf_StoredIsKept_WhenSpeedChangesOrDwarfLeaves()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Experience)).Mine;
        await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Level);
        await _service.AssignDwarfAsync(_playerId, mine.Id, _reserve[0].Id);

        _clock.Advance(TimeSpan.FromMinutes(30));
        var upgraded = await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Speed);
        Assert.Equal(MiningService.RatePerHour(MineType.Experience, 1, 1) / 2, upgraded.Mine.Stored, 6);

        _clock.Advance(TimeSpan.FromMinutes(30));
        var removed = await _service.RemoveDwarfAsync(_playerId, mine.Id);
        double expected = MiningService.RatePerHour(MineType.Experience, 1, 1) / 2 + MiningService.RatePerHour(MineType.Experience, 2, 1) / 2;
        Assert.Equal(expected, removed.Mine.Stored, 6);

        // Без дворфа склад больше не растёт
        _clock.Advance(TimeSpan.FromHours(5));
        Assert.Equal(expected, (await _service.GetBoardAsync(_playerId)).Mines[0].Stored, 6);
    }

    [Fact]
    public async Task Dwarf_IsBusy_ForSquadExpeditionsAndOtherMines()
    {
        var first = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var second = (await _service.BuildAsync(_playerId, 2, MineType.Gold)).Mine;
        await _service.UpgradeAsync(_playerId, first.Id, MineUpgrade.Level);
        await _service.UpgradeAsync(_playerId, second.Id, MineUpgrade.Level);
        var dwarf = _reserve[0];

        await _service.AssignDwarfAsync(_playerId, first.Id, dwarf.Id);

        Assert.Equal(MiningError.HeroBusy, await ErrorOf(() => _service.AssignDwarfAsync(_playerId, second.Id, dwarf.Id)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _heroes.SaveSquadAsync(_playerId, [dwarf.Id]));
        Assert.Contains(dwarf.Id, await _heroes.GetMiningHeroIdsAsync(_playerId));

        var expeditions = new ExpeditionService(new ExpeditionRepository(_context), _heroes, new PlayerRepository(_context), new EntityFactory(), _clock);
        var offer = (await expeditions.GetBoardAsync(_playerId)).Expeditions.First();
        await Assert.ThrowsAsync<InvalidOperationException>(() => expeditions.StartAsync(_playerId, offer.Id, [dwarf.Id]));

        var squadHero = (await _heroes.GetSquadAsync(_playerId)).First();
        Assert.Equal(MiningError.HeroInSquad, await ErrorOf(() => _service.AssignDwarfAsync(_playerId, second.Id, squadHero.Id)));
    }

    [Fact]
    public async Task Save_WithStaleVersion_IsRejected()
    {
        var info = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var stale = (await _mining.GetAsync(_playerId, info.Id))!;

        await _service.UpgradeAsync(_playerId, info.Id, MineUpgrade.Speed);

        stale.SpeedLevel = 5;
        Assert.Equal(MiningSaveOutcome.Changed, await _mining.SaveAsync(stale, stale.Version, MiningChange.None));
        Assert.Equal(2, (await _mining.GetAsync(_playerId, info.Id))!.SpeedLevel);
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
