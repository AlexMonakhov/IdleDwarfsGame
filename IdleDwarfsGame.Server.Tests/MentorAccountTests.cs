using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Брокк-наставник (MentorAccount) и то, что новичок получает при регистрации: Брокка в друзьях и письмо с радужными монетами
public class MentorAccountTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();
    private readonly SquadPowerService _squadPower;

    public MentorAccountTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
        _squadPower = new SquadPowerService(new PvpRepository(_context), new EntityFactory());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Seed_CreatesBrokkWithTheTutorialFireDwarf_Once()
    {
        await MentorAccount.SeedAsync(_context, _hasher, _squadPower);
        await MentorAccount.SeedAsync(_context, _hasher, _squadPower);

        var user = await _context.Users.SingleAsync();
        Assert.Equal(MentorAccount.UserId, user.Id);
        Assert.Equal(MentorAccount.Username, user.Username);

        var hero = await _context.Heroes.SingleAsync(h => h.PlayerId == MentorAccount.PlayerId);
        var mentor = TutorialBattle.Mentor(user.PlayerId);
        Assert.Equal(MentorAccount.PlayerId, hero.PlayerId);
        Assert.Equal("Brokk", hero.Name);
        Assert.Equal(HeroElement.Fire, hero.Element);
        Assert.Equal(mentor.Level, hero.Level);
        Assert.Equal(mentor.MaxHp, hero.MaxHp);
        Assert.Equal(mentor.Attack, hero.Attack);
        Assert.Equal(mentor.SpellIds, hero.SpellIds);
        Assert.Equal(1, hero.PlaceInSquad);
        Assert.True(await _context.Players.Where(p => p.Id == MentorAccount.PlayerId).Select(p => p.SquadPower).SingleAsync() > 0);
    }

    [Fact]
    public async Task Seed_SkipsWhenTheNameIsTaken()
    {
        _context.Users.Add(NewUser("brokk"));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await MentorAccount.SeedAsync(_context, _hasher, _squadPower);

        Assert.False(await _context.Users.AnyAsync(u => u.Id == MentorAccount.UserId));
    }

    [Fact]
    public async Task Register_BefriendsBrokk_AndSendsRainbowCoinsByMail()
    {
        await MentorAccount.SeedAsync(_context, _hasher, _squadPower);

        var result = await Controller().Register(new RegisterRequest
        {
            Username = "Newbie", Email = "newbie@test.local", Password = "secret1", Locale = "ru"
        });
        Assert.IsType<OkObjectResult>(result.Result);

        var user = await _context.Users.SingleAsync(u => u.Username == "Newbie");
        Assert.Equal(0, await _context.Players.Where(p => p.Id == user.PlayerId).Select(p => p.RainbowCoins).SingleAsync());
        Assert.True(await new FriendRepository(_context).AreFriendsAsync(user.Id, MentorAccount.UserId));

        var mail = await _context.PlayerMails.Include(m => m.Attachments).SingleAsync(m => m.PlayerId == user.PlayerId);
        Assert.Equal(WelcomeMail.Title("ru"), mail.Title);
        var coins = Assert.Single(mail.Attachments);
        Assert.Equal(MailAttachmentKind.Currency, coins.Kind);
        Assert.Equal(nameof(MailCurrency.RainbowCoins), coins.Key);
        Assert.Equal(ShopRules.StarterRainbowCoins, coins.Amount);

        await Mail().ClaimAsync(user.PlayerId, mail.Id);
        Assert.Equal(ShopRules.StarterRainbowCoins, await _context.Players.Where(p => p.Id == user.PlayerId).Select(p => p.RainbowCoins).SingleAsync());
    }

    [Fact]
    public async Task Register_WithoutBrokk_StillSendsTheMail()
    {
        await Controller().Register(new RegisterRequest { Username = "Newbie", Email = "newbie@test.local", Password = "secret1" });

        var user = await _context.Users.SingleAsync();
        Assert.Equal(0, await _context.Friendships.CountAsync());
        Assert.Equal(WelcomeMail.Title(null), (await _context.PlayerMails.SingleAsync(m => m.PlayerId == user.PlayerId)).Title);
    }

    [Fact]
    public async Task RequestToBrokk_MakesFriendsImmediately()
    {
        await MentorAccount.SeedAsync(_context, _hasher, _squadPower);
        var alice = NewUser("Alice");
        _context.Users.Add(alice);
        await _context.SaveChangesAsync();

        var friends = new FriendRepository(_context);
        var service = new FriendService(friends, new UserRepository(_context), new SessionService(new UserSessionRepository(_context), friends, TimeProvider.System), TimeProvider.System);
        var result = await service.SendRequestAsync(alice.Id, "brokk");

        Assert.Equal(FriendRequestOutcome.Accepted, result.Outcome);
        Assert.True(await friends.AreFriendsAsync(alice.Id, MentorAccount.UserId));
        Assert.Equal(0, await _context.FriendRequests.CountAsync());
    }

    [Fact]
    public async Task Brokk_IsNotAnArenaOpponent()
    {
        await MentorAccount.SeedAsync(_context, _hasher, _squadPower);

        Assert.Empty(await new PvpRepository(_context).GetCandidatesAsync(Guid.NewGuid()));
    }

    private AuthController Controller() => new(
        _context, new UserRepository(_context), new FakeTokens(), _hasher, _squadPower, new FriendRepository(_context), Mail());

    private MailService Mail() => new(_context, new EntityFactory(), new PlayerRepository(_context), TimeProvider.System, new Random(1));

    private static User NewUser(string name)
    {
        var player = new Player { Id = Guid.NewGuid(), Name = name };
        return new User
        {
            Id = Guid.NewGuid(),
            Username = name,
            NormalizedUsername = UserRepository.Normalize(name),
            Email = $"{name}@test.local",
            NormalizedEmail = UserRepository.Normalize($"{name}@test.local"),
            PasswordHash = "x",
            CreatedAt = DateTime.UtcNow,
            PlayerId = player.Id,
            Player = player
        };
    }

    private sealed class FakeTokens : ITokenService
    {
        public (string Token, DateTime ExpiresAt) CreateToken(User user) => ("token", DateTime.UtcNow.AddHours(1));
    }
}
