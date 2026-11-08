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
        _service = new MiningService(_mining, _heroes, new ExpeditionRepository(_context), _clock, NoActivity.Instance);
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

        // Золотая шахта на старте: 200 в час, клик — 45 секунд работы = 2.5 золота
        Assert.Equal(2.5, mine.PerClick, 6);
        var dug = await _service.DigAsync(_playerId, mine.Id, 10);
        Assert.Equal(10, dug.Clicks);
        Assert.Equal(25, dug.Mined);
        Assert.Equal(100_000 + dug.Mined, dug.Wallet.Gold);

        // Следующая пачка в ту же секунду — кликов сверх лимита не засчитываем
        var spam = await _service.DigAsync(_playerId, mine.Id, MiningService.MaxClicksPerBatch);
        Assert.Equal(0, spam.Mined);

        _clock.Advance(TimeSpan.FromSeconds(1));
        var next = await _service.DigAsync(_playerId, mine.Id, MiningService.MaxClicksPerBatch);
        Assert.Equal(MiningService.MaxClicksPerSecond, next.Clicks);
    }

    [Fact]
    public async Task Dig_CrystalMine_GivesAFractionPerClick_AndKeepsTheRemainder()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Sapphire)).Mine;
        // 6 в час, клик — 45 секунд работы: 0.075 камня, а не целый камень, как раньше
        Assert.Equal(0.075, mine.PerClick, 6);

        var first = await _service.DigAsync(_playerId, mine.Id, 40);
        Assert.Equal(3, first.Mined);

        _clock.Advance(TimeSpan.FromSeconds(1));
        var second = await _service.DigAsync(_playerId, mine.Id, 10);
        Assert.Equal(0, second.Mined);
        Assert.Equal(0.75, second.Mine.DigProgress, 6);

        _clock.Advance(TimeSpan.FromSeconds(1));
        var third = await _service.DigAsync(_playerId, mine.Id, 10);
        Assert.Equal(1, third.Mined);
        Assert.Equal(0.5, third.Mine.DigProgress, 6);

        Assert.Equal(4, third.Wallet.Crystals[GemType.Sapphire]);
        var stack = await _context.PlayerItems.SingleAsync(i => i.PlayerId == _playerId);
        Assert.Equal((ItemType.Gem, "Sapphire", Rarity.Common, 1, 4), (stack.ItemType, stack.SubType, stack.Rarity, stack.Level, stack.Quantity));
    }

    [Fact]
    public async Task Dig_SpendsPickaxeEnergy_SharedByAllMines_AndItRecoversOverTime()
    {
        var gold = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var xp = (await _service.BuildAsync(_playerId, 2, MineType.Experience)).Mine;

        // Полная кирка — DigEnergyMax кликов на все шахты вместе
        int clicks = 0;
        for (int batch = 0; batch < 10; batch++)
        {
            _clock.Advance(TimeSpan.FromSeconds(10));
            clicks += (await _service.DigAsync(_playerId, batch % 2 == 0 ? gold.Id : xp.Id, MiningService.MaxClicksPerBatch)).Clicks;
        }
        // Пока кликали (по 10 секунд между пачками), кирка успела чуть восстановиться: ~0.56 клика за 10 секунд
        double regenerated = 9 * 10 / 3600.0 * MiningService.DigEnergyPerHour;
        Assert.InRange(clicks, MiningService.DigEnergyMax, MiningService.DigEnergyMax + (int)Math.Ceiling(regenerated));

        // Сразу же, в другой шахте (её лимит частоты уже прошёл): кирка пуста, клики не засчитываются
        var tired = await _service.DigAsync(_playerId, gold.Id, MiningService.MaxClicksPerBatch);
        Assert.Equal(0, tired.Clicks);
        Assert.InRange(tired.Wallet.DigEnergy, 0, 1);

        // За полчаса восстанавливается половина
        _clock.Advance(TimeSpan.FromMinutes(30));
        var board = await _service.GetBoardAsync(_playerId);
        // плюс остаток меньше клика, что был в кирке
        Assert.InRange(board.Wallet.DigEnergy, MiningService.DigEnergyPerHour / 2, MiningService.DigEnergyPerHour / 2 + 1);
        Assert.Equal(MiningService.DigEnergyMax, board.DigEnergyMax);
    }

    [Fact]
    public async Task Dig_ParallelClicksInTwoMines_CannotSpendTheSameEnergyTwice()
    {
        var gold = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var stale = (await _mining.GetAsync(_playerId, gold.Id))!;
        var wallet = await _mining.GetWalletAsync(_playerId);

        // Другая вкладка успела потратить кирку — запись по устаревшему чтению не проходит
        await _service.DigAsync(_playerId, gold.Id, 5);
        stale.Version++;
        var change = MiningChange.Credit(MineType.Gold, 1) with { Energy = new DigEnergySpend(wallet.DigEnergyAt, 100, _clock.UtcNow) };
        Assert.Equal(MiningSaveOutcome.Changed, await _mining.SaveAsync(stale, stale.Version, change));
    }

    [Fact]
    public async Task Upgrade_PaysCost_AndIsCappedByMineLevel()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var cost = MiningService.UpgradeCost(MineType.Gold, MineUpgrade.Yield, 1);

        var upgraded = await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Yield);
        Assert.Equal(100_000 - cost.Gold, upgraded.Wallet.Gold);
        Assert.Equal(MiningService.PerClick(MineType.Gold, 1, 2), upgraded.Mine.PerClick, 6);

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

        // Клик приносит долю камня — копаем пачками, пока не наберётся ровно на улучшение
        int mined = 0;
        while (mined < cost.Crystals)
        {
            _clock.Advance(TimeSpan.FromSeconds(1));
            mined += (await _service.DigAsync(_playerId, mine.Id, 1)).Mined;
        }
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
        Assert.Equal(MiningService.Capacity(MineType.Gold, 1, 1, 1), collected.Collected);
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

        var expeditions = new ExpeditionService(new ExpeditionRepository(_context), _heroes, new PlayerRepository(_context), new EntityFactory(), _clock, NoActivity.Instance);
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

    [Fact]
    public async Task Save_DwarfTakenByAnotherMine_IsRejectedByUniqueIndex()
    {
        var first = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var second = (await _service.BuildAsync(_playerId, 2, MineType.Gold)).Mine;
        await _service.UpgradeAsync(_playerId, first.Id, MineUpgrade.Level);
        await _service.UpgradeAsync(_playerId, second.Id, MineUpgrade.Level);
        var dwarf = _reserve[0];
        await _service.AssignDwarfAsync(_playerId, first.Id, dwarf.Id);

        // Как при параллельном запросе: проверка занятости в сервисе уже пройдена, ловит только индекс
        var raced = (await _mining.GetAsync(_playerId, second.Id))!;
        Assert.Equal(MiningSaveOutcome.HeroTaken,
            await _mining.SaveAsync(raced, raced.Version, MiningChange.None, MineDwarfChange.Put(dwarf.Id, 1)));
        Assert.Empty((await _mining.GetAsync(_playerId, second.Id))!.Dwarves);
    }

    [Fact]
    public async Task Pickaxe_Upgrades_RaiseCapacityRecoveryAndClickYield_AndCostGoldAndExperience()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        var cost = MiningService.PickaxeCost(PickaxeUpgrade.Capacity, 1)!;

        var capacity = await _service.UpgradePickaxeAsync(_playerId, PickaxeUpgrade.Capacity);
        Assert.Equal(MiningService.DigEnergyMax + MiningService.EnergyPerCapacityLevel, capacity.DigEnergyMax);
        Assert.Equal(100_000 - cost.Gold, capacity.Wallet.Gold);
        Assert.Equal(100_000 - cost.Experience, capacity.Wallet.Experience);
        // Запас не появился из воздуха: был полным (200), стал 200 из 250 и дальше восстанавливается
        Assert.Equal(MiningService.DigEnergyMax, capacity.Wallet.DigEnergy, 6);

        var recovery = await _service.UpgradePickaxeAsync(_playerId, PickaxeUpgrade.Recovery);
        Assert.Equal(MiningService.DigEnergyPerHour + MiningService.EnergyPerRecoveryLevel, recovery.DigEnergyPerHour);

        // Сила кирки умножает добычу клика во всех шахтах
        var strength = await _service.UpgradePickaxeAsync(_playerId, PickaxeUpgrade.Strength);
        Assert.Equal(mine.PerClick * MiningService.StrengthFactor(2), strength.Mines[0].PerClick, 6);
        Assert.Equal(2, strength.Pickaxe.Single(p => p.Upgrade == PickaxeUpgrade.Strength).Level);

        _clock.Advance(TimeSpan.FromSeconds(10));
        var dug = await _service.DigAsync(_playerId, mine.Id, 10);
        Assert.Equal((int)Math.Floor(10 * strength.Mines[0].PerClick + 1e-9), dug.Mined);
    }

    [Fact]
    public async Task Pickaxe_Upgrade_StopsAtMaxLevel_AndNeedsResources()
    {
        await SetGold(0);
        Assert.Equal(MiningError.NotEnoughGold, await ErrorOf(() => _service.UpgradePickaxeAsync(_playerId, PickaxeUpgrade.Capacity)));

        await SetGold(10_000_000);
        await _context.Players.Where(p => p.Id == _playerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Experience, 10_000_000));
        for (int level = 1; level < MiningService.PickaxeMaxLevel; level++)
            await _service.UpgradePickaxeAsync(_playerId, PickaxeUpgrade.Recovery);

        Assert.Equal(MiningError.MaxLevel, await ErrorOf(() => _service.UpgradePickaxeAsync(_playerId, PickaxeUpgrade.Recovery)));
        Assert.Null(MiningService.PickaxeCost(PickaxeUpgrade.Recovery, MiningService.PickaxeMaxLevel));
    }

    [Fact]
    public void DwarfSlots_OpenWithMineLevel_UpToThree()
    {
        Assert.Equal(0, MiningService.DwarfSlots(1));
        Assert.Equal(1, MiningService.DwarfSlots(2));
        Assert.Equal(2, MiningService.DwarfSlots(4));
        Assert.Equal(3, MiningService.DwarfSlots(7));
        Assert.Equal(3, MiningService.DwarfSlots(MiningService.MaxLevel));
        Assert.Equal(3, MiningService.MaxDwarves);
        Assert.Equal(4, MiningService.NextSlotLevel(2));
        Assert.Null(MiningService.NextSlotLevel(MiningService.MaxLevel));
    }

    [Fact]
    public async Task SeveralDwarves_FillSlots_DigTogether_AndLeaveOneByOne()
    {
        var mine = (await _service.BuildAsync(_playerId, 1, MineType.Gold)).Mine;
        for (int level = 1; level < 4; level++)
            await _service.UpgradeAsync(_playerId, mine.Id, MineUpgrade.Level);

        await _service.AssignDwarfAsync(_playerId, mine.Id, _reserve[0].Id);
        var two = await _service.AssignDwarfAsync(_playerId, mine.Id, _reserve[1].Id);
        Assert.Equal([_reserve[0].Id, _reserve[1].Id], two.Mine.HeroIds);
        Assert.Equal(2, two.Mine.DwarfSlots);
        Assert.Equal(7, two.Mine.NextSlotLevel);

        // На 4-м уровне мест два — третьему некуда
        Assert.Equal(MiningError.NoFreeSlot, await ErrorOf(() => _service.AssignDwarfAsync(_playerId, mine.Id, _reserve[2].Id)));

        // Двое копают вдвое быстрее, склад у них тоже вдвое больше
        double perDwarf = MiningService.RatePerHour(MineType.Gold, 1, 1);
        _clock.Advance(TimeSpan.FromHours(1));
        var board = await _service.GetBoardAsync(_playerId);
        Assert.Equal(perDwarf * 2, board.Mines[0].Stored, 6);
        Assert.Equal(MiningService.Capacity(MineType.Gold, 1, 1, 1) * 2, board.Mines[0].Capacity);

        // Отзываем конкретного — второй остаётся, его место освобождается для другого
        var left = await _service.RemoveDwarfAsync(_playerId, mine.Id, _reserve[0].Id);
        Assert.Equal([_reserve[1].Id], left.Mine.HeroIds);
        Assert.Equal(perDwarf * 2, left.Mine.Stored, 6);
        var refilled = await _service.AssignDwarfAsync(_playerId, mine.Id, _reserve[2].Id);
        Assert.Equal(2, refilled.Mine.HeroIds.Count);

        // Без указания дворфа отозвать нельзя, когда их несколько
        Assert.Equal(MiningError.NoDwarf, await ErrorOf(() => _service.RemoveDwarfAsync(_playerId, mine.Id)));
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
