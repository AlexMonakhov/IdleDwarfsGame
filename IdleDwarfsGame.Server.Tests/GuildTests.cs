using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class GuildTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc));
    private readonly GuildRepository _repository;
    private readonly GuildService _service;
    private readonly User _alice;
    private readonly User _bob;
    private readonly User _carol;

    public GuildTests()
    {
        // In-memory SQLite живёт, пока открыто соединение. Шаблоны боссов приходят из сидов (HeroSeedData)
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _alice = AddUser("Alice");
        _bob = AddUser("Bob");
        _carol = AddUser("Carol");
        _context.SaveChanges();

        _repository = new GuildRepository(_context);
        var heroes = new HeroesRepository(_context);
        var factory = new EntityFactory();
        var combat = new CombatService(new WorldFactory(heroes, factory), new CombatEngine(new Random(1)), []);
        var sessions = new SessionService(new UserSessionRepository(_context), new FriendRepository(_context), _clock);
        _service = new GuildService(_repository, new UserRepository(_context), new PlayerRepository(_context), heroes, factory,
            combat, sessions, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    /* ===== Гильдия ===== */

    [Fact]
    public async Task Create_MakesLeader_AndRejectsTakenNames()
    {
        var overview = await _service.CreateAsync(_alice.Id, "  Iron   Beards ");

        Assert.Equal("Iron Beards", overview.Guild!.Name);
        Assert.Equal(GuildRole.Leader, overview.Guild.MyRole);
        Assert.Equal(_alice.Id, Assert.Single(overview.Guild.Members).UserId);

        Assert.Equal(GuildError.NameTaken, (await Assert.ThrowsAsync<GuildException>(() => _service.CreateAsync(_bob.Id, "iron beards"))).Code);
        Assert.Equal(GuildError.AlreadyInGuild, (await Assert.ThrowsAsync<GuildException>(() => _service.CreateAsync(_alice.Id, "Other"))).Code);
        Assert.Equal(GuildError.InvalidName, (await Assert.ThrowsAsync<GuildException>(() => _service.CreateAsync(_bob.Id, "<b>"))).Code);
    }

    [Fact]
    public async Task JoinRequest_AcceptedByLeader_RemovesOtherRequests()
    {
        var guildId = await CreateGuild(_alice, "Iron Beards");
        var otherId = await CreateGuild(_carol, "Stone Fists");

        await _service.SendRequestAsync(_bob.Id, guildId);
        await _service.SendRequestAsync(_bob.Id, otherId);

        var search = await _service.SearchAsync(_bob.Id, "beard");
        Assert.True(Assert.Single(search).Requested);
        Assert.Equal(2, (await _service.GetOverviewAsync(_bob.Id)).Outgoing.Count);
        Assert.Equal("Bob", Assert.Single((await _service.GetOverviewAsync(_alice.Id)).Guild!.Incoming).Username);

        // Принять заявку может только глава гильдии
        Assert.Equal(GuildError.NotInGuild, (await Assert.ThrowsAsync<GuildException>(() => _service.AcceptRequestAsync(_bob.Id, _bob.Id))).Code);

        await _service.AcceptRequestAsync(_alice.Id, _bob.Id);

        var bob = await _service.GetOverviewAsync(_bob.Id);
        Assert.Equal(guildId, bob.Guild!.Id);
        Assert.Equal(GuildRole.Member, bob.Guild.MyRole);
        Assert.Empty(bob.Guild.Incoming);
        Assert.Empty(bob.Outgoing);
        Assert.Equal(0, await _context.GuildJoinRequests.CountAsync());

        Assert.Equal(GuildError.NotLeader, (await Assert.ThrowsAsync<GuildException>(() => _service.KickAsync(_bob.Id, _alice.Id))).Code);
        Assert.Equal(GuildError.AlreadyInGuild, (await Assert.ThrowsAsync<GuildException>(() => _service.SendRequestAsync(_bob.Id, otherId))).Code);
    }

    [Fact]
    public async Task Requests_CanBeDeclinedAndCancelled()
    {
        var guildId = await CreateGuild(_alice, "Iron Beards");

        await _service.SendRequestAsync(_bob.Id, guildId);
        Assert.Equal(GuildError.RequestAlreadySent, (await Assert.ThrowsAsync<GuildException>(() => _service.SendRequestAsync(_bob.Id, guildId))).Code);
        await _service.DeclineRequestAsync(_alice.Id, _bob.Id);
        Assert.Equal(GuildError.RequestNotFound, (await Assert.ThrowsAsync<GuildException>(() => _service.AcceptRequestAsync(_alice.Id, _bob.Id))).Code);

        await _service.SendRequestAsync(_bob.Id, guildId);
        await _service.CancelRequestAsync(_bob.Id, guildId);
        Assert.Empty((await _service.GetOverviewAsync(_alice.Id)).Guild!.Incoming);
    }

    [Fact]
    public async Task LeaderLeaving_PassesLead_LastMemberDisbands()
    {
        var guildId = await CreateGuild(_alice, "Iron Beards");
        await Join(_bob, guildId);
        _clock.Advance(TimeSpan.FromMinutes(1));
        await Join(_carol, guildId);

        await _service.KickAsync(_alice.Id, _carol.Id);
        Assert.Null((await _service.GetOverviewAsync(_carol.Id)).Guild);

        await _service.LeaveAsync(_alice.Id);
        Assert.Equal(GuildRole.Leader, (await _service.GetOverviewAsync(_bob.Id)).Guild!.MyRole);

        await _service.GetBossesAsync(_bob.Id);
        await _service.LeaveAsync(_bob.Id);
        Assert.Equal(0, await _context.Guilds.CountAsync());
        Assert.Equal(0, await _context.GuildBosses.CountAsync());
        Assert.Equal(GuildError.NotInGuild, (await Assert.ThrowsAsync<GuildException>(() => _service.LeaveAsync(_bob.Id))).Code);
    }

    /* ===== Боссы ===== */

    [Fact]
    public async Task Attack_DamagesSharedBoss_AndStartsCooldown()
    {
        var guildId = await CreateGuild(_alice, "Iron Beards");
        await Join(_bob, guildId);

        var result = await _service.AttackAsync(_alice.Id, GuildBossTier.Easy);
        Assert.True(result.Damage > 0);
        Assert.False(result.Killed);
        Assert.Equal(result.MaxHp - result.Damage, result.BossHp);
        Assert.Equal(_clock.UtcNow + GuildBosses.AttackCooldown, result.NextAttackAt);

        // Тот же босс у всех участников
        var bob = await _service.GetBossesAsync(_bob.Id);
        var easy = bob.Bosses.First(b => b.Tier == GuildBossTier.Easy);
        Assert.Equal(result.BossHp, easy.CurrentHp);
        Assert.Null(easy.NextAttackAt);
        Assert.Equal(result.Damage, Assert.Single(easy.Damage).Damage);

        // Второй удар — только через час; босса другого уровня бить можно
        var error = await Assert.ThrowsAsync<GuildException>(() => _service.AttackAsync(_alice.Id, GuildBossTier.Easy));
        Assert.Equal(GuildError.OnCooldown, error.Code);
        await _service.AttackAsync(_alice.Id, GuildBossTier.Medium);

        _clock.Advance(GuildBosses.AttackCooldown);
        var second = await _service.AttackAsync(_alice.Id, GuildBossTier.Easy);
        Assert.Equal(result.BossHp - second.Damage, second.BossHp);
    }

    [Fact]
    public async Task Cooldown_FollowsPlayer_ToAnotherGuild()
    {
        await CreateGuild(_alice, "Iron Beards");
        await _service.AttackAsync(_alice.Id, GuildBossTier.Easy);

        await _service.LeaveAsync(_alice.Id);
        await CreateGuild(_alice, "Stone Fists");

        var error = await Assert.ThrowsAsync<GuildException>(() => _service.AttackAsync(_alice.Id, GuildBossTier.Easy));
        Assert.Equal(GuildError.OnCooldown, error.Code);
    }

    [Fact]
    public async Task KilledBoss_RewardOncePerMember_AndOncePerDayAcrossGuilds()
    {
        var guildId = await CreateGuild(_alice, "Iron Beards");
        await Join(_bob, guildId);
        var otherId = await CreateGuild(_carol, "Stone Fists");

        Assert.Equal(GuildError.BossAlive, (await Assert.ThrowsAsync<GuildException>(() => _service.ClaimBossRewardAsync(_bob.Id, GuildBossTier.Easy))).Code);

        // Добиваем босса: оставляем ему чуть-чуть HP
        await SetBossHp(guildId, GuildBossTier.Easy, 1);
        var kill = await _service.AttackAsync(_alice.Id, GuildBossTier.Easy);
        Assert.True(kill.Killed);
        Assert.Equal(1, kill.Damage);
        Assert.Equal(0, kill.BossHp);
        Assert.Equal(GuildError.BossDefeated, (await Assert.ThrowsAsync<GuildException>(() => _service.AttackAsync(_bob.Id, GuildBossTier.Easy))).Code);

        // Награду забирает любой участник, в том числе тот, кто не бил
        var goldBefore = (await _context.Players.AsNoTracking().FirstAsync(p => p.Id == _bob.PlayerId)).Gold;
        var claimed = await _service.ClaimBossRewardAsync(_bob.Id, GuildBossTier.Easy);
        var definition = GuildBosses.Get(GuildBossTier.Easy);
        Assert.Equal(goldBefore + definition.Gold, claimed.Resources.Gold);
        Assert.Equal(definition.Chests.Length, await _context.PlayerChests.CountAsync(c => c.PlayerId == _bob.PlayerId));
        await _service.ClaimBossRewardAsync(_alice.Id, GuildBossTier.Easy);

        Assert.Equal(GuildError.RewardAlreadyClaimed, (await Assert.ThrowsAsync<GuildException>(() => _service.ClaimBossRewardAsync(_bob.Id, GuildBossTier.Easy))).Code);
        Assert.True((await _service.GetBossesAsync(_bob.Id)).Bosses.First(b => b.Tier == GuildBossTier.Easy).RewardClaimed);

        // Боб уходит в гильдию, где босс тоже убит, — но сегодня награду за этот уровень он уже брал
        await _service.LeaveAsync(_bob.Id);
        await Join(_bob, otherId);
        await SetBossHp(otherId, GuildBossTier.Easy, 1);
        await _service.AttackAsync(_carol.Id, GuildBossTier.Easy);
        Assert.Equal(GuildError.RewardAlreadyClaimed, (await Assert.ThrowsAsync<GuildException>(() => _service.ClaimBossRewardAsync(_bob.Id, GuildBossTier.Easy))).Code);
    }

    [Fact]
    public async Task Bosses_RefreshAtMidnightUtc()
    {
        var guildId = await CreateGuild(_alice, "Iron Beards");
        await SetBossHp(guildId, GuildBossTier.Easy, 1);
        await _service.AttackAsync(_alice.Id, GuildBossTier.Easy);
        await _service.ClaimBossRewardAsync(_alice.Id, GuildBossTier.Easy);

        var today = await _service.GetBossesAsync(_alice.Id);
        Assert.Equal(new DateTime(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc), today.ResetAt);

        _clock.Advance(TimeSpan.FromHours(12));
        var easy = (await _service.GetBossesAsync(_alice.Id)).Bosses.First(b => b.Tier == GuildBossTier.Easy);
        Assert.Equal(easy.MaxHp, easy.CurrentHp);
        Assert.Null(easy.DefeatedAt);
        Assert.False(easy.RewardClaimed);
        Assert.Empty(easy.Damage);
    }

    private async Task<Guid> CreateGuild(User leader, string name) =>
        (await _service.CreateAsync(leader.Id, name)).Guild!.Id;

    private async Task Join(User user, Guid guildId)
    {
        var leader = await _context.GuildMembers.AsNoTracking().FirstAsync(m => m.GuildId == guildId && m.Role == GuildRole.Leader);
        await _service.SendRequestAsync(user.Id, guildId);
        await _service.AcceptRequestAsync(leader.UserId, user.Id);
    }

    private async Task SetBossHp(Guid guildId, GuildBossTier tier, int hp)
    {
        // Боссы создаются при первом заходе в сутки
        await _service.GetBossesAsync((await _context.GuildMembers.AsNoTracking().FirstAsync(m => m.GuildId == guildId)).UserId);
        await _context.GuildBosses
            .Where(b => b.GuildId == guildId && b.Tier == tier)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.CurrentHp, hp));
    }

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

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
