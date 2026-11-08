using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class CommunityMineTests : IDisposable
{
    // Вторник недели общих шахт (ротация начинается 2 ноября)
    private static readonly DateTime EventStart = new(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Tuesday = new(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(Tuesday);
    private readonly CommunityMineService _service;
    private readonly HeroesRepository _heroes;
    private readonly Guid _alice;
    private readonly Guid _bob;

    public CommunityMineTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _alice = AddPlayer("Alice");
        _bob = AddPlayer("Bob");
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        _heroes = new HeroesRepository(_context);
        _service = new CommunityMineService(new CommunityMineRepository(_context), _heroes, new ExpeditionRepository(_context),
            new PlayerRepository(_context), new ActivityRepository(_context), new FixedShopRepository(_context), _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private Guid AddPlayer(string name)
    {
        var player = new Player { Id = Guid.NewGuid(), Name = name, Gold = 1_000_000 };
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = name,
            NormalizedUsername = UserRepository.Normalize(name),
            Email = $"{name}@test.local",
            NormalizedEmail = UserRepository.Normalize($"{name}@test.local"),
            PlayerId = player.Id,
            Player = player
        });
        _context.Heroes.AddRange(StarterSquad.Create(player.Id, new Random(1)));
        return player.Id;
    }

    // Свободные дворфы вне отряда
    private async Task<List<Hero>> AddDwarvesAsync(Guid playerId, int count, int rank = 1)
    {
        var random = new Random(count * 31 + rank);
        var dwarves = Enumerable.Range(0, count).Select(_ => HeroGenerator.Create(playerId, rank, "", random)).ToList();
        _context.Heroes.AddRange(dwarves);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return dwarves;
    }

    private async Task<DepositInfo> DepositAsync(Guid playerId, int index) =>
        (await _service.GetAsync(playerId)).Deposits.Single(d => d.Index == index);

    private async Task<long> WalletAsync(Guid playerId) =>
        (await _context.PlayerEventWallets.AsNoTracking().FirstOrDefaultAsync(w => w.PlayerId == playerId && w.EventStart == EventStart))?.Currency ?? 0;

    /* ===== Правила ===== */

    [Fact]
    public void Settle_SplitsOreByRate_AndStopsAtTheLastOre()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var deposit = new CommunityDeposit { Id = Guid.NewGuid(), OreTotal = 100, UpdatedAt = Tuesday };
        var workers = new List<DepositWorker>();
        var rates = new Dictionary<Guid, double> { [a] = 10, [b] = 30 };

        Assert.False(CommunityMineRules.Settle(deposit, workers, rates, Tuesday.AddHours(1), Tuesday.AddDays(5)));
        Assert.Equal(40, deposit.OreMined, 6);
        Assert.Equal(10, workers.Single(w => w.PlayerId == a).Mined, 6);
        Assert.Equal(30, workers.Single(w => w.PlayerId == b).Mined, 6);

        // Осталось 60 при 40 в час — кончится через полтора часа, дальше руда не прибавляется
        Assert.True(CommunityMineRules.Settle(deposit, workers, rates, Tuesday.AddHours(10), Tuesday.AddDays(5)));
        Assert.Equal(100, deposit.OreMined, 6);
        Assert.Equal(Tuesday.AddHours(2.5), deposit.ExhaustedAt!.Value, TimeSpan.FromSeconds(1));
        Assert.Equal(100, workers.Sum(w => w.Mined), 6);
    }

    [Fact]
    public void Reward_NeedsMinShare_AndScalesWithWhatWasMined()
    {
        var deposit = new CommunityDeposit { Kind = DepositKind.Iron, OreTotal = 1_000, OreMined = 1_000 };
        Assert.Equal(0, CommunityMineRules.RewardFor(deposit, new DepositWorker { Mined = 5 }));

        int full = CommunityMineRules.RewardFor(deposit, new DepositWorker { Mined = 500 });
        Assert.Equal((int)Math.Round(CommunityMineRules.RewardBase * 2 + CommunityMineRules.RewardPool * 2 * 0.5), full);

        deposit.OreMined = 500;
        Assert.Equal((int)Math.Round(full / 2.0), CommunityMineRules.RewardFor(deposit, new DepositWorker { Mined = 500 }));
    }

    [Fact]
    public void Miner_DigsFaster_AndRankAndLevelHelp()
    {
        var hero = new Hero { Level = 1, UpgradeLevel = 1, Profession = HeroProfession.Warrior };
        Assert.Equal(CommunityMineRules.BaseDwarfRate, CommunityMineRules.DwarfRate(hero));
        Assert.Equal(CommunityMineRules.BaseDwarfRate * CommunityMineRules.MinerBonus,
            CommunityMineRules.DwarfRate(new Hero { Level = 1, UpgradeLevel = 1, Profession = HeroProfession.Miner }));
        Assert.True(CommunityMineRules.DwarfRate(new Hero { Level = 3, UpgradeLevel = 20, Profession = HeroProfession.Warrior }) > 2 * CommunityMineRules.BaseDwarfRate);
    }

    /* ===== Карта ===== */

    [Fact]
    public async Task EventStart_OpensTwoDeposits_SizedByActivePlayers()
    {
        await new ActivityRepository(_context).AddAsync(_alice, Tuesday.Date, ActivityKind.MineDig, 5);
        await new ActivityRepository(_context).AddAsync(_bob, Tuesday.Date, ActivityKind.MineDig, 5);
        _clock.Advance(EventStart - Tuesday);

        var overview = await _service.GetAsync(_alice);

        Assert.True(overview.Open);
        Assert.Equal(2, overview.Deposits.Count);
        Assert.Equal(DepositKind.Copper, overview.Deposits[0].Kind);
        Assert.Equal(CommunityMineRules.CopperMaxRank, overview.Deposits[0].MaxRank);
        Assert.Equal(CommunityMineRules.OreOf(DepositKind.Iron, 2), overview.Deposits[1].OreTotal);
        Assert.Equal(EventStart + CommunityMineRules.SpawnInterval, overview.NextSpawnAt);
    }

    [Fact]
    public async Task Deposits_AppearOnSchedule_ButNoMoreThanMaxOpen()
    {
        var overview = await _service.GetAsync(_alice);
        // Вторник 10:00 — 34 часа ивента: по расписанию их уже больше, но открытых не больше MaxOpen
        Assert.Equal(CommunityMineRules.MaxOpen, overview.Deposits.Count);
        Assert.All(overview.Deposits, d => Assert.Null(d.ClosedAt));
    }

    [Fact]
    public async Task OtherWeek_HasNoDeposits()
    {
        _clock.Advance(TimeSpan.FromDays(7));
        var overview = await _service.GetAsync(_alice);

        Assert.False(overview.Open);
        Assert.Empty(overview.Deposits);
        Assert.Empty(await _context.CommunityDeposits.ToListAsync());
    }

    /* ===== Дворфы ===== */

    [Fact]
    public async Task Send_MakesDwarvesBusyEverywhere_AndRecallFreesThem()
    {
        var dwarves = await AddDwarvesAsync(_alice, 2);
        var iron = await DepositAsync(_alice, 1);

        var overview = await _service.SendAsync(_alice, iron.Id, dwarves.Select(d => d.Id).ToList());
        var deposit = overview.Deposits.Single(d => d.Id == iron.Id);
        Assert.Equal(2, deposit.MyDwarves.Count);
        Assert.Equal(dwarves.Sum(CommunityMineRules.DwarfRate), deposit.Rate, 6);
        Assert.NotNull(deposit.EtaAt);

        // Заняты для личных шахт, экспедиций и отряда
        Assert.Contains(dwarves[0].Id, await _heroes.GetMiningHeroIdsAsync(_alice));
        Assert.Contains(dwarves[0].Id, await new MiningRepository(_context).GetHeroIdsAsync(_alice));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _heroes.SaveSquadAsync(_alice, [dwarves[0].Id, null, null, null, null]));

        var copper = await DepositAsync(_alice, 0);
        var error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.SendAsync(_alice, copper.Id, [dwarves[0].Id]));
        Assert.Equal(CommunityMineError.HeroBusy, error.Code);

        await _service.RecallAsync(_alice, iron.Id, dwarves[0].Id);
        Assert.DoesNotContain(dwarves[0].Id, await _heroes.GetMiningHeroIdsAsync(_alice));
        Assert.Contains(dwarves[1].Id, await _heroes.GetMiningHeroIdsAsync(_alice));
    }

    [Fact]
    public async Task Send_RefusesSquadHeroes_StrongDwarvesOnCopper_AndTooMany()
    {
        var squadHero = (await _heroes.GetSquadAsync(_alice)).First();
        var copper = await DepositAsync(_alice, 0);
        var iron = await DepositAsync(_alice, 1);

        var error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.SendAsync(_alice, iron.Id, [squadHero.Id]));
        Assert.Equal(CommunityMineError.HeroInSquad, error.Code);

        var strong = await AddDwarvesAsync(_alice, 1, rank: 3);
        error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.SendAsync(_alice, copper.Id, [strong[0].Id]));
        Assert.Equal(CommunityMineError.RankTooHigh, error.Code);

        var crowd = await AddDwarvesAsync(_alice, CommunityMineRules.MaxDwarvesPerPlayer + 1);
        error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.SendAsync(_alice, iron.Id, crowd.Select(d => d.Id).ToList()));
        Assert.Equal(CommunityMineError.TooManyDwarves, error.Code);

        var foreign = await AddDwarvesAsync(_bob, 1);
        error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.SendAsync(_alice, iron.Id, [foreign[0].Id]));
        Assert.Equal(CommunityMineError.HeroNotFound, error.Code);
    }

    [Fact]
    public async Task Mining_SplitsTheOreBetweenPlayersByTheirDwarves()
    {
        var aliceDwarves = await AddDwarvesAsync(_alice, 1);
        var bobDwarves = await AddDwarvesAsync(_bob, 3);
        var iron = await DepositAsync(_alice, 1);
        await _service.SendAsync(_alice, iron.Id, aliceDwarves.Select(d => d.Id).ToList());
        await _service.SendAsync(_bob, iron.Id, bobDwarves.Select(d => d.Id).ToList());

        _clock.Advance(TimeSpan.FromHours(2));
        var alice = await DepositAsync(_alice, 1);
        var bob = await DepositAsync(_bob, 1);

        double aliceRate = aliceDwarves.Sum(CommunityMineRules.DwarfRate);
        double bobRate = bobDwarves.Sum(CommunityMineRules.DwarfRate);
        Assert.Equal(aliceRate * 2, alice.MyContribution, 3);
        Assert.Equal(bobRate * 2, bob.MyContribution, 3);
        Assert.Equal((aliceRate + bobRate) * 2, alice.OreMined, 3);
        Assert.Equal(2, alice.Participants);
        Assert.Equal("Bob", alice.Top[0].Name);
    }

    /* ===== Улучшение ===== */

    [Fact]
    public async Task Upgrade_SpeedsUpEveryone_ChargesGold_AndCountsForTheUpgrader()
    {
        var dwarves = await AddDwarvesAsync(_bob, 2);
        var iron = await DepositAsync(_bob, 1);
        await _service.SendAsync(_bob, iron.Id, dwarves.Select(d => d.Id).ToList());
        int cost = iron.UpgradeGold!.Value;

        var overview = await _service.UpgradeAsync(_alice, iron.Id);
        var deposit = overview.Deposits.Single(d => d.Id == iron.Id);

        Assert.Equal(2, deposit.SpeedLevel);
        Assert.Equal(dwarves.Sum(CommunityMineRules.DwarfRate) * CommunityMineRules.SpeedFactor(2), deposit.Rate, 6);
        Assert.Equal(deposit.OreTotal * CommunityMineRules.UpgradeShare, deposit.MyContribution, 6);
        Assert.Equal(1_000_000 - cost, (await _context.Players.AsNoTracking().FirstAsync(p => p.Id == _alice)).Gold);
    }

    [Fact]
    public async Task Upgrade_WithoutGold_ChangesNothing()
    {
        await _context.Players.Where(p => p.Id == _alice).ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, 0L));
        var iron = await DepositAsync(_alice, 1);

        var error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.UpgradeAsync(_alice, iron.Id));
        Assert.Equal(CommunityMineError.NotEnoughGold, error.Code);
        Assert.Equal(1, (await DepositAsync(_alice, 1)).SpeedLevel);
    }

    [Fact]
    public async Task UpgradePrice_DependsOnTheDepositKind()
    {
        var iron = await DepositAsync(_alice, 1);
        Assert.Equal(DepositKind.Iron, iron.Kind);
        Assert.Equal(CommunityMineRules.UpgradeGold(DepositKind.Iron, 1), iron.UpgradeGold);

        var kinds = Enum.GetValues<DepositKind>().Select(k => CommunityMineRules.UpgradeGold(k, 1)!.Value).ToList();
        Assert.Equal(kinds.OrderBy(g => g), kinds);
        Assert.True(kinds[^1] > 50 * kinds[0]);
        Assert.Null(CommunityMineRules.UpgradeGold(DepositKind.Copper, CommunityMineRules.MaxSpeedLevel));
    }

    [Fact]
    public async Task Upgrades_AreLimitedPerPlayer_OthersCanContinue()
    {
        var iron = await DepositAsync(_alice, 1);
        for (int i = 0; i < CommunityMineRules.MaxUpgradesPerPlayer; i++)
            await _service.UpgradeAsync(_alice, iron.Id);

        var error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.UpgradeAsync(_alice, iron.Id));
        Assert.Equal(CommunityMineError.UpgradeLimit, error.Code);

        var mine = await DepositAsync(_alice, 1);
        Assert.Equal(CommunityMineRules.MaxUpgradesPerPlayer, mine.MyUpgrades);
        Assert.Equal(mine.OreTotal * CommunityMineRules.UpgradeShare * CommunityMineRules.MaxUpgradesPerPlayer, mine.MyContribution, 6);

        var after = (await _service.UpgradeAsync(_bob, iron.Id)).Deposits.Single(d => d.Id == iron.Id);
        Assert.Equal(CommunityMineRules.MaxUpgradesPerPlayer + 2, after.SpeedLevel);
        Assert.Equal(1, after.MyUpgrades);
    }

    /* ===== Закрытие и награды ===== */

    [Fact]
    public async Task Exhausted_Deposit_Closes_FreesDwarves_AndPaysByShare()
    {
        var dwarves = await AddDwarvesAsync(_alice, 3);
        var copper = await DepositAsync(_alice, 0);
        await _service.SendAsync(_alice, copper.Id, dwarves.Select(d => d.Id).ToList());

        // 300 руды при 30+ в час — к вечеру выработано
        _clock.Advance(TimeSpan.FromHours(12));
        await _service.TickAsync();

        var deposit = await _context.CommunityDeposits.AsNoTracking().SingleAsync(d => d.Id == copper.Id);
        Assert.NotNull(deposit.ClosedAt);
        Assert.Equal(deposit.OreTotal, deposit.OreMined, 6);
        Assert.Empty(await _heroes.GetMiningHeroIdsAsync(_alice));

        var overview = await _service.GetAsync(_alice);
        var pending = Assert.Single(overview.Pending);
        Assert.Equal(1, pending.Share, 6);
        int expected = CommunityMineRules.RewardBase + CommunityMineRules.RewardPool;
        Assert.Equal(expected, pending.Currency);
        // Место выработанного заняло новое месторождение
        Assert.Equal(CommunityMineRules.MaxOpen, overview.Deposits.Count(d => d.ClosedAt == null));

        var claim = await _service.ClaimAsync(_alice);
        Assert.Equal(expected, claim.Currency);
        Assert.Equal(expected, await WalletAsync(_alice));
        var error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.ClaimAsync(_alice));
        Assert.Equal(CommunityMineError.NothingToClaim, error.Code);
    }

    [Fact]
    public async Task EventEnd_ClosesUnfinishedDeposits_WithPartialRewards()
    {
        var dwarves = await AddDwarvesAsync(_alice, 1);
        var iron = await DepositAsync(_alice, 1);
        await _service.SendAsync(_alice, iron.Id, dwarves.Select(d => d.Id).ToList());

        // Воскресенье — день лавки: копать нельзя, руда за субботу ещё засчитана
        _clock.Advance(new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc) - _clock.UtcNow);
        var overview = await _service.GetAsync(_alice);

        Assert.False(overview.Open);
        Assert.All(overview.Deposits, d => Assert.NotNull(d.ClosedAt));
        Assert.Empty(await _heroes.GetMiningHeroIdsAsync(_alice));

        var pending = Assert.Single(overview.Pending);
        var deposit = overview.Deposits.Single(d => d.Id == iron.Id);
        Assert.True(deposit.OreMined < deposit.OreTotal || deposit.ExhaustedAt != null);
        Assert.True(pending.Currency > 0);

        var error = await Assert.ThrowsAsync<CommunityMineException>(() => _service.SendAsync(_alice, iron.Id, [dwarves[0].Id]));
        Assert.Equal(CommunityMineError.EventNotActive, error.Code);
    }

    [Fact]
    public async Task Leaderboard_SumsContributionOverTheEvent()
    {
        var aliceDwarves = await AddDwarvesAsync(_alice, 2);
        var bobDwarves = await AddDwarvesAsync(_bob, 1);
        await _service.SendAsync(_alice, (await DepositAsync(_alice, 1)).Id, [aliceDwarves[0].Id]);
        await _service.SendAsync(_alice, (await DepositAsync(_alice, 2)).Id, [aliceDwarves[1].Id]);
        await _service.SendAsync(_bob, (await DepositAsync(_bob, 1)).Id, [bobDwarves[0].Id]);

        _clock.Advance(TimeSpan.FromHours(1));
        var overview = await _service.GetAsync(_alice);

        Assert.Equal(2, overview.Leaderboard.Count);
        Assert.Equal(aliceDwarves.Sum(CommunityMineRules.DwarfRate), overview.MyContribution, 3);
        Assert.Equal(overview.Leaderboard[0].PlayerId == _alice ? 1 : 2, overview.MyRank);
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
