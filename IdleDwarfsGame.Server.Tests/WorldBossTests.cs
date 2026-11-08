using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class WorldBossTests : IDisposable
{
    // Вторник недели мировых боссов (ротация: 2 ноября — шахты, 9 ноября — боссы)
    private static readonly DateTime BossWeek = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EventStart = new(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(BossWeek);
    private readonly WorldBossService _service;
    private readonly Guid _alice;
    private readonly Guid _bob;

    public WorldBossTests()
    {
        // In-memory SQLite живёт, пока открыто соединение. Шаблоны боссов приходят из сидов (GuildBossSeedData)
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _alice = AddPlayer("Alice");
        _bob = AddPlayer("Bob");
        _context.SaveChanges();

        var heroes = new HeroesRepository(_context);
        var factory = new EntityFactory();
        var combat = new CombatService(new WorldFactory(heroes, factory), new CombatEngine(new Random(1)), []);
        _service = new WorldBossService(new WorldBossRepository(_context), heroes, factory, combat, new FixedShopRepository(_context),
            new ActivityRepository(_context), _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private Guid AddPlayer(string name)
    {
        var player = new Player { Id = Guid.NewGuid(), Name = name };
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

    private async Task MarkActiveAsync(Guid playerId) =>
        await new ActivityRepository(_context).AddAsync(playerId, BossWeek.Date, ActivityKind.CampaignFight, 1);

    private async Task SetBossHpAsync(int hp) =>
        await _context.WorldBosses.ExecuteUpdateAsync(s => s.SetProperty(b => b.CurrentHp, hp));

    private async Task<long> WalletAsync(Guid playerId) =>
        (await _context.PlayerEventWallets.AsNoTracking().FirstOrDefaultAsync(w => w.PlayerId == playerId && w.EventStart == EventStart))?.Currency ?? 0;

    /* ===== Правила ===== */

    [Fact]
    public void Hp_GrowsWithStageAndActivePlayers_WithinBounds()
    {
        Assert.Equal(WorldBossRules.HpPerActivePlayer, WorldBossRules.HpOf(1, 1));
        Assert.Equal(WorldBossRules.HpPerActivePlayer * 10, WorldBossRules.HpOf(1, 10));
        Assert.True(WorldBossRules.HpOf(2, 10) > WorldBossRules.HpOf(1, 10));
        Assert.Equal(WorldBossRules.HpPerActivePlayer, WorldBossRules.HpOf(1, 0));
        Assert.Equal(WorldBossRules.MaxHp, WorldBossRules.HpOf(200, 10_000));
        Assert.True(WorldBossRules.LevelOf(3) > WorldBossRules.LevelOf(1));
        Assert.Equal(WorldBossRules.MaxLevel, WorldBossRules.LevelOf(1_000));
    }

    [Fact]
    public void KillReward_IsBasePlusShare_AndNothingWithoutDamage()
    {
        Assert.Equal(0, WorldBossRules.KillReward(1, 0, 1_000, lastHit: true));
        Assert.Equal(WorldBossRules.KillBase(1) + WorldBossRules.SharePool(1), WorldBossRules.KillReward(1, 1_000, 1_000, lastHit: false));
        Assert.Equal(WorldBossRules.KillBase(1) + WorldBossRules.SharePool(1) / 2 + WorldBossRules.LastHitBonus,
            WorldBossRules.KillReward(1, 500, 1_000, lastHit: true));
    }

    /* ===== Босс ===== */

    [Fact]
    public async Task BossWeek_SpawnsTheFirstStage_WithHpForActivePlayers()
    {
        await MarkActiveAsync(_alice);
        await MarkActiveAsync(_bob);

        var overview = await _service.GetAsync(_alice);

        Assert.True(overview.Open);
        Assert.Equal(1, overview.Boss!.Stage);
        Assert.Equal(WorldBossRules.HpOf(1, 2), overview.Boss.MaxHp);
        Assert.Equal(overview.Boss.MaxHp, overview.Boss.CurrentHp);
        Assert.Equal(WorldBossRules.LevelOf(1), overview.Boss.Level);
        Assert.Single(await _context.WorldBosses.ToListAsync());
    }

    [Fact]
    public async Task MinesWeek_HasNoBoss_AndRefusesAttacks()
    {
        _clock.Advance(TimeSpan.FromDays(-7));

        var overview = await _service.GetAsync(_alice);
        Assert.False(overview.Open);
        Assert.Null(overview.Boss);

        var error = await Assert.ThrowsAsync<WorldBossException>(() => _service.AttackAsync(_alice));
        Assert.Equal(WorldBossError.EventNotActive, error.Code);
    }

    [Fact]
    public async Task Attack_DamagesTheSharedBoss_PaysCurrency_AndStartsCooldown()
    {
        var result = await _service.AttackAsync(_alice);

        Assert.True(result.Damage > 0);
        Assert.Equal(result.MaxHp - result.Damage, result.BossHp);
        Assert.Equal(_clock.UtcNow + WorldBossRules.AttackCooldown, result.NextAttackAt);
        Assert.Equal(WorldBossRules.AttackCurrency, await WalletAsync(_alice));

        // HP общее: Боб видит урон Алисы
        var bob = await _service.GetAsync(_bob);
        Assert.Equal(result.BossHp, bob.Boss!.CurrentHp);
        Assert.Equal("Alice", Assert.Single(bob.Boss.Top).Name);

        var error = await Assert.ThrowsAsync<WorldBossException>(() => _service.AttackAsync(_alice));
        Assert.Equal(WorldBossError.OnCooldown, error.Code);

        _clock.Advance(WorldBossRules.AttackCooldown);
        await _service.AttackAsync(_alice);
        var contribution = await _context.WorldBossContributions.AsNoTracking().SingleAsync(c => c.PlayerId == _alice);
        Assert.Equal(2, contribution.Attacks);
    }

    [Fact]
    public async Task Kill_SpawnsTheNextStronger_Stage_AndPaysEveryoneWhoHit()
    {
        await _service.AttackAsync(_bob);
        await SetBossHpAsync(1);

        var kill = await _service.AttackAsync(_alice);
        Assert.True(kill.Killed);
        Assert.Equal(1, kill.Damage);

        var overview = await _service.GetAsync(_alice);
        Assert.Equal(2, overview.Boss!.Stage);
        Assert.True(overview.Boss.Level > WorldBossRules.LevelOf(1));
        Assert.True(overview.Boss.MaxHp > kill.MaxHp);

        var alicePending = Assert.Single(overview.Pending);
        Assert.True(alicePending.LastHit);
        var bobPending = Assert.Single((await _service.GetAsync(_bob)).Pending);
        Assert.False(bobPending.LastHit);
        Assert.True(bobPending.Currency > WorldBossRules.KillBase(1));

        var claim = await _service.ClaimAsync(_bob);
        Assert.Equal(bobPending.Currency, claim.Currency);
        Assert.Equal(WorldBossRules.AttackCurrency + bobPending.Currency, await WalletAsync(_bob));
        Assert.Empty(claim.Overview.Pending);

        var error = await Assert.ThrowsAsync<WorldBossException>(() => _service.ClaimAsync(_bob));
        Assert.Equal(WorldBossError.NothingToClaim, error.Code);
    }

    [Fact]
    public async Task ShopDay_NoAttacksOrNewBosses_ButRewardsCanBeClaimed()
    {
        await SetBossHpAsync(1);
        await _service.GetAsync(_alice);
        await SetBossHpAsync(1);
        await _service.AttackAsync(_alice);

        // Воскресенье — ивент кончился, лавка ещё работает
        _clock.Advance(new DateTime(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc) - _clock.UtcNow);
        var overview = await _service.GetAsync(_alice);
        Assert.False(overview.Open);
        Assert.Equal(1, overview.Boss!.Stage);
        Assert.NotNull(overview.Boss.DefeatedAt);
        Assert.Single(await _context.WorldBosses.ToListAsync());

        var error = await Assert.ThrowsAsync<WorldBossException>(() => _service.AttackAsync(_alice));
        Assert.Equal(WorldBossError.EventNotActive, error.Code);

        var claim = await _service.ClaimAsync(_alice);
        Assert.True(claim.Currency > 0);
    }

    [Fact]
    public async Task Leaderboard_SumsDamageOverTheEvent()
    {
        var aliceHit = await _service.AttackAsync(_alice);
        var bobHit = await _service.AttackAsync(_bob);

        var overview = await _service.GetAsync(_alice);
        Assert.Equal(2, overview.Leaderboard.Count);
        Assert.Equal(aliceHit.Damage, overview.MyEventDamage);
        int expectedRank = aliceHit.Damage >= bobHit.Damage ? 1 : 2;
        Assert.Equal(expectedRank, overview.MyRank);
        Assert.True(overview.Leaderboard[0].Damage >= overview.Leaderboard[1].Damage);
    }

    [Fact]
    public async Task NoHeroes_IsRefusedBeforeTheCooldown()
    {
        // Пустой отряд добирается свободными героями (HeroesRepository.GetSquadAsync) — отказ, только когда героев нет вовсе
        await _context.Heroes.Where(h => h.PlayerId == _alice).ExecuteDeleteAsync();
        _context.ChangeTracker.Clear();

        var error = await Assert.ThrowsAsync<WorldBossException>(() => _service.AttackAsync(_alice));
        Assert.Equal(WorldBossError.NoSquad, error.Code);
        Assert.Empty(await _context.WorldBossCooldowns.ToListAsync());
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
