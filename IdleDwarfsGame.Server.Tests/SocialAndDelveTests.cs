using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Гонка глубины гильдий, помощь друзьям (шахта, наёмник), ставки на «Бой дня», глубинный спуск
public class SocialAndDelveTests : IDisposable
{
    // Вторник недели, начавшейся в понедельник 9 ноября
    private static readonly DateTime Tuesday = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Week = new(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(Tuesday);
    private readonly User _alice;
    private readonly User _bob;
    private readonly User _carol;

    private readonly GuildService _guilds;
    private readonly GuildDepthService _depth;
    private readonly FriendHelpService _friendHelp;
    private readonly ArenaBetService _bets;
    private readonly DelveService _delve;

    public SocialAndDelveTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _alice = AddUser("Alice");
        _bob = AddUser("Bob");
        _carol = AddUser("Carol");
        _context.SaveChanges();

        var heroes = new HeroesRepository(_context);
        var players = new PlayerRepository(_context);
        var factory = new EntityFactory();
        var engine = new CombatEngine(new Random(1));
        var campaignRepository = new CampaignRepository(_context);
        ICombatScenario[] scenarios = [new CampaignScenario(campaignRepository, factory)];
        var combat = new CombatService(new WorldFactory(heroes, factory), engine, scenarios);
        var sessions = new SessionService(new UserSessionRepository(_context), new FriendRepository(_context), _clock);
        var guildRepository = new GuildRepository(_context);
        var expeditions = new ExpeditionRepository(_context);

        _guilds = new GuildService(guildRepository, new UserRepository(_context), players, heroes, factory, combat, sessions, _clock, NoActivity.Instance);
        _depth = new GuildDepthService(_context, guildRepository, heroes, expeditions, players, _clock);
        var campaign = new CampaignService(campaignRepository, heroes, factory, scenarios, combat, new RewardRepository(_context),
            new Random(1), _clock, NoActivity.Instance);
        _friendHelp = new FriendHelpService(_context, new MiningRepository(_context), heroes, players, factory, engine, scenarios, campaign, _clock);
        _bets = new ArenaBetService(_context, new PvpRepository(_context), factory, players, _clock);
        _delve = new DelveService(_context, heroes, campaignRepository, players, factory, engine, scenarios, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    /* ===== Гонка глубины ===== */

    [Fact]
    public void DepthRules_StorageCaps_ButScoreKeepsGrowing()
    {
        var dwarf = new GuildDepthDwarf { WeekStart = Week, HeroId = Guid.NewGuid(), BaseRate = 10, UpdatedAt = Tuesday };

        GuildDepthRules.Settle(dwarf, 1, Tuesday.AddHours(20));

        Assert.Equal(200, dwarf.Mined, 6);
        Assert.Equal(10 * GuildDepthRules.StorageHours, dwarf.Stored, 6);

        // После конца гонки добыча не идёт
        GuildDepthRules.Settle(dwarf, 1, Week.AddDays(10));
        Assert.Equal(10 * (GuildDepthRules.EndOf(Week) - Tuesday).TotalHours, dwarf.Mined, 6);
    }

    [Fact]
    public async Task DepthRace_RanksGuilds_AndPaysBadgesByPlace()
    {
        var iron = (await _guilds.CreateAsync(_alice.Id, "Iron Beards")).Guild!.Id;
        await _guilds.CreateAsync(_carol.Id, "Stone Fists");
        await _guilds.SendRequestAsync(_bob.Id, iron);
        await _guilds.AcceptRequestAsync(_alice.Id, _bob.Id);

        var aliceDwarf = (await AddDwarvesAsync(_alice.PlayerId, 1, rank: 2)).Single();
        var bobDwarf = (await AddDwarvesAsync(_bob.PlayerId, 1)).Single();
        var carolDwarf = (await AddDwarvesAsync(_carol.PlayerId, 1)).Single();

        await _depth.PlaceAsync(_alice.Id, aliceDwarf.Id);
        await _depth.PlaceAsync(_bob.Id, bobDwarf.Id);
        await _depth.PlaceAsync(_carol.Id, carolDwarf.Id);

        // Дворф в гонке занят: ни в шахту, ни в экспедицию
        Assert.Contains(aliceDwarf.Id, await new HeroesRepository(_context).GetMiningHeroIdsAsync(_alice.PlayerId));

        _clock.Advance(TimeSpan.FromHours(10));
        var overview = await _depth.GetAsync(_alice.Id);
        Assert.Equal(1, overview.GuildPlace);
        Assert.Equal(2, overview.GuildCount);
        Assert.Equal(2, overview.Members.Count);
        Assert.True(overview.MyDwarf!.StoredGold > 0);

        // Сбор: руда со склада — в золото, очки гильдии остаются
        long goldBefore = await GoldAsync(_alice.PlayerId);
        overview = await _depth.CollectAsync(_alice.Id);
        Assert.True(await GoldAsync(_alice.PlayerId) > goldBefore);
        Assert.Equal(0, overview.MyDwarf!.Stored);
        Assert.True(overview.GuildScore > 0);

        // Подъёмник платит улучшающий, а ускоряет всех дворфов гильдии
        await SetGoldAsync(_bob.PlayerId, 10_000_000);
        double aliceRate = overview.MyDwarf.Rate;
        await _depth.UpgradeShaftAsync(_bob.Id);
        Assert.True((await _depth.GetAsync(_alice.Id)).MyDwarf!.Rate > aliceRate);

        // До конца гонки забирать нечего; после — значки по месту
        await Assert.ThrowsAsync<GuildDepthException>(() => _depth.ClaimAsync(_alice.Id));
        _clock.Advance(GuildDepthRules.EndOf(Week) - _clock.UtcNow + TimeSpan.FromHours(1));

        long badgesBefore = await BadgesAsync(_carol.PlayerId);
        var carol = await _depth.ClaimAsync(_carol.Id);
        Assert.Null(carol.Pending);
        Assert.Equal(GuildDepthRules.HonorBadgesFor(2), await BadgesAsync(_carol.PlayerId) - badgesBefore);
        Assert.Equal(GuildDepthError.NothingToClaim, (await Assert.ThrowsAsync<GuildDepthException>(() => _depth.ClaimAsync(_carol.Id))).Code);

        var alice = await _depth.ClaimAsync(_alice.Id);
        Assert.Equal(GuildDepthRules.HonorBadgesFor(1), alice.Resources.HonorBadges);
    }

    [Fact]
    public async Task DepthRace_OneDwarfPerWeek_EvenAfterSwitchingGuilds()
    {
        await _guilds.CreateAsync(_alice.Id, "Iron Beards");
        var stone = (await _guilds.CreateAsync(_carol.Id, "Stone Fists")).Guild!.Id;
        var dwarves = await AddDwarvesAsync(_alice.PlayerId, 2);

        await _depth.PlaceAsync(_alice.Id, dwarves[0].Id);
        await _guilds.LeaveAsync(_alice.Id);
        await _guilds.SendRequestAsync(_alice.Id, stone);
        await _guilds.AcceptRequestAsync(_carol.Id, _alice.Id);

        var error = await Assert.ThrowsAsync<GuildDepthException>(() => _depth.PlaceAsync(_alice.Id, dwarves[1].Id));
        Assert.Equal(GuildDepthError.PlacedInOtherGuild, error.Code);
        Assert.True((await _depth.GetAsync(_alice.Id)).PlacedElsewhere);
    }

    /* ===== Друзья ===== */

    [Fact]
    public async Task MineHelp_FillsFriendStorage_OncePerDay()
    {
        await MakeFriendsAsync(_alice, _bob);
        var dwarf = (await AddDwarvesAsync(_bob.PlayerId, 1)).Single();
        var mine = new Mine { Id = Guid.NewGuid(), PlayerId = _bob.PlayerId, Plot = 1, Type = MineType.Gold, Level = 2, UpdatedAt = Tuesday };
        _context.Mines.Add(mine);
        _context.MineDwarves.Add(new MineDwarf { MineId = mine.Id, HeroId = dwarf.Id, Slot = 1 });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _friendHelp.HelpMineAsync(_alice.Id, _bob.Id, mine.Id);

        Assert.Equal(Math.Floor(MiningService.RatePerHour(MineType.Gold, 1, 1) * FriendHelpRules.HelpHours), result.Added);
        Assert.True(result.Gold > 0);
        Assert.True(result.Mines.HelpedToday);
        Assert.Equal(FriendHelpError.AlreadyHelped,
            (await Assert.ThrowsAsync<FriendHelpException>(() => _friendHelp.HelpMineAsync(_alice.Id, _bob.Id, mine.Id))).Code);
        Assert.Equal(FriendHelpError.NotFriends,
            (await Assert.ThrowsAsync<FriendHelpException>(() => _friendHelp.HelpMineAsync(_carol.Id, _bob.Id, mine.Id))).Code);
    }

    [Fact]
    public async Task Mercenary_FightsInTheSquad_AndRewardsTheLender()
    {
        await MakeFriendsAsync(_alice, _bob);
        var mercenaries = await _friendHelp.GetMercenariesAsync(_alice.Id);
        var bob = Assert.Single(mercenaries.Friends);
        var hero = bob.Heroes.First();

        var result = await _friendHelp.FightCampaignAsync(_alice.Id, 1, _bob.Id, hero.HeroId, 1);

        Assert.Contains(result.Fight.Combat.Units, u => u.Team == 1 && u.HeroId == hero.HeroId && u.Position == 1);
        Assert.Equal(FriendHelpRules.LenderElderSeals, await _context.Players.Where(p => p.Id == _bob.PlayerId).Select(p => p.ElderSeals).SingleAsync());
        Assert.Equal(FriendHelpError.AlreadyHired,
            (await Assert.ThrowsAsync<FriendHelpException>(() => _friendHelp.FightCampaignAsync(_alice.Id, 1, _bob.Id, hero.HeroId, 1))).Code);
        // Отряд друга не изменился: герой остался на своём месте у него
        Assert.Equal(hero.Unit.Position, await _context.Heroes.Where(h => h.Id == hero.HeroId).Select(h => h.PlaceInSquad).SingleAsync());
    }

    /* ===== Ставки ===== */

    [Fact]
    public void BetPayout_SplitsPoolAmongWinners_OrRefunds()
    {
        var winner = new ArenaBet { Side = 1, Amount = 50 };
        var loser = new ArenaBet { Side = 2, Amount = 50 };

        Assert.Equal(90, ArenaBetRules.PayoutFor(winner, 1, 50, 50));
        Assert.Equal(0, ArenaBetRules.PayoutFor(loser, 1, 50, 50));
        // На победителя не ставил никто — ставки возвращаются
        Assert.Equal(50, ArenaBetRules.PayoutFor(loser, 1, 0, 50));
    }

    [Fact]
    public async Task FightOfTheDay_TakesBets_ResolvesAfterWindow_AndReplaysTheSameFight()
    {
        await _context.Players.Where(p => p.Id == _alice.PlayerId || p.Id == _bob.PlayerId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.SquadPower, 1000));
        await _context.Players.Where(p => p.Id == _carol.PlayerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.BloodCoins, 100));

        var overview = await _bets.GetAsync(_carol.Id);
        var match = overview.Current!;
        Assert.True(match.Open);
        Assert.Equal(ArenaBetError.OwnMatch,
            (await Assert.ThrowsAsync<ArenaBetException>(() => _bets.PlaceAsync(match.A.UserId, 1, 10))).Code);

        overview = await _bets.PlaceAsync(_carol.Id, 1, 40);
        Assert.Equal(60, overview.Resources.BloodCoins);
        Assert.Equal(40, overview.Current!.A.Pool);

        _clock.Advance(TimeSpan.FromHours(ArenaBetRules.WindowHours));
        overview = await _bets.GetAsync(_carol.Id);
        var resolved = overview.Previous!;
        Assert.NotNull(resolved.Winner);

        var replay = await _bets.ReplayAsync(resolved.WindowStart);
        Assert.Equal(resolved.Winner, replay.Combat.WinningTeam);
        Assert.Equal(resolved.Rounds, replay.Combat.Rounds);

        // Ставили одни — на победу банк возвращается за вычетом доли, на поражение — ноль
        overview = await _bets.ClaimAsync(_carol.Id);
        Assert.Equal(resolved.Winner == 1 ? 60 + 36 : 60, overview.Resources.BloodCoins);
    }

    /* ===== Глубинный спуск ===== */

    [Fact]
    public void DelveMap_IsTheSameAllWeek_AndEndsWithABoss()
    {
        var map = DelveRules.MapOf(Week);
        Assert.Equal(map, DelveRules.MapOf(Week));
        Assert.All(map.Where(n => n.Floor == 1), n => Assert.Equal(DelveNodeType.Fight, n.Type));
        Assert.Equal(DelveNodeType.Boss, Assert.Single(map, n => n.Floor == DelveRules.Floors).Type);
    }

    [Fact]
    public async Task Delve_KeepsHpBetweenFights_AndAllowsOneRunPerDay()
    {
        var overview = await _delve.StartAsync(_alice.Id);
        Assert.Equal(DelveRunStatus.Active, overview.Run!.Status);
        Assert.Equal(DelveError.AlreadyRanToday, (await Assert.ThrowsAsync<DelveException>(() => _delve.StartAsync(_alice.Id))).Code);

        var move = await _delve.MoveAsync(_alice.Id, 0);
        Assert.Equal(DelveNodeType.Fight, move.Node);
        Assert.NotNull(move.Combat);

        var run = move.Overview.Run!;
        if (move.Won == true)
        {
            Assert.Equal(1, run.Floor);
            // HP героев после боя не восстановилось
            var after = move.Combat!.Units.Where(u => u.Team == 1).ToList();
            Assert.Equal(after.Count, run.Units.Count + run.Fallen.Count);
            Assert.Equal(DelveError.InvalidMove, (await Assert.ThrowsAsync<DelveException>(() => _delve.MoveAsync(_alice.Id, 0 + 2))).Code);
        }
        else
        {
            Assert.Equal(DelveRunStatus.Dead, run.Status);
        }

        // Назавтра — новый забег, вчерашний закрывается
        _clock.Advance(TimeSpan.FromDays(1));
        overview = await _delve.StartAsync(_alice.Id);
        Assert.Equal(0, overview.Run!.Floor);
        Assert.Equal(1, await _context.DelveRuns.CountAsync(r => r.UserId == _alice.Id && r.Status == DelveRunStatus.Active));
    }

    [Fact]
    public async Task DelveCamp_HealsAndRevives()
    {
        var overview = await _delve.StartAsync(_alice.Id);
        var run = await _context.DelveRuns.SingleAsync(r => r.Id == overview.Run!.Id);
        var heroes = run.Squad();
        run.SetHp(heroes.ToDictionary(h => h.Id, h => h.Id == heroes[0].Id ? 0.0 : 0.5));
        run.Pending = DelvePending.Camp;
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        overview = await _delve.CampAsync(_alice.Id, "revive", heroes[0].Id);
        Assert.Empty(overview.Run!.Fallen);
        Assert.Equal(DelvePending.None, overview.Run.Pending);
        Assert.Equal(DelveError.NothingPending, (await Assert.ThrowsAsync<DelveException>(() => _delve.CampAsync(_alice.Id, "rest", null))).Code);
    }

    /* ===== Вспомогательное ===== */

    private User AddUser(string username)
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
        _context.Heroes.AddRange(StarterSquad.Create(player.Id, new Random(1)));
        return user;
    }

    private async Task<List<Hero>> AddDwarvesAsync(Guid playerId, int count, int rank = 1)
    {
        var random = new Random(count * 31 + rank);
        var dwarves = Enumerable.Range(0, count).Select(_ => HeroGenerator.Create(playerId, rank, "", random)).ToList();
        _context.Heroes.AddRange(dwarves);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return dwarves;
    }

    private async Task MakeFriendsAsync(User a, User b)
    {
        _context.Friendships.Add(new Friendship { UserId = a.Id, FriendId = b.Id, CreatedAt = Tuesday });
        _context.Friendships.Add(new Friendship { UserId = b.Id, FriendId = a.Id, CreatedAt = Tuesday });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    private Task<long> GoldAsync(Guid playerId) => _context.Players.Where(p => p.Id == playerId).Select(p => p.Gold).SingleAsync();
    private Task<long> BadgesAsync(Guid playerId) => _context.Players.Where(p => p.Id == playerId).Select(p => p.HonorBadges).SingleAsync();
    private Task SetGoldAsync(Guid playerId, long gold) =>
        _context.Players.Where(p => p.Id == playerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, gold));

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
