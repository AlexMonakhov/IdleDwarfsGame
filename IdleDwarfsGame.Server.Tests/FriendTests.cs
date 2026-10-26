using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class FriendTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc));
    private readonly SessionService _sessions;
    private readonly FriendService _service;
    private readonly User _alice;
    private readonly User _bob;
    private readonly User _carol;

    public FriendTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _alice = AddUser("Alice");
        _bob = AddUser("Bob");
        _carol = AddUser("Carol");
        _context.SaveChanges();

        var friends = new FriendRepository(_context);
        _sessions = new SessionService(new UserSessionRepository(_context), friends, _clock);
        _service = new FriendService(friends, new UserRepository(_context), _sessions, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Request_ShowsUpOnBothSides_UntilAccepted()
    {
        var result = await _service.SendRequestAsync(_alice.Id, "bob");
        Assert.Equal(FriendRequestOutcome.Sent, result.Outcome);

        var alice = await _service.GetOverviewAsync(_alice.Id);
        var bob = await _service.GetOverviewAsync(_bob.Id);
        Assert.Equal("Bob", Assert.Single(alice.Outgoing).Username);
        Assert.Equal("Alice", Assert.Single(bob.Incoming).Username);
        Assert.Empty(alice.Friends);

        await _service.AcceptRequestAsync(_bob.Id, _alice.Id);

        alice = await _service.GetOverviewAsync(_alice.Id);
        bob = await _service.GetOverviewAsync(_bob.Id);
        Assert.Equal(_bob.Id, Assert.Single(alice.Friends).UserId);
        Assert.Equal(_alice.Id, Assert.Single(bob.Friends).UserId);
        Assert.Empty(alice.Outgoing);
        Assert.Empty(bob.Incoming);
    }

    [Fact]
    public async Task CounterRequest_MakesFriendsImmediately()
    {
        await _service.SendRequestAsync(_alice.Id, "Bob");
        var result = await _service.SendRequestAsync(_bob.Id, "Alice");

        Assert.Equal(FriendRequestOutcome.Accepted, result.Outcome);
        Assert.True(await new FriendRepository(_context).AreFriendsAsync(_alice.Id, _bob.Id));
        Assert.Equal(0, await _context.FriendRequests.CountAsync());
    }

    [Fact]
    public async Task SendRequest_RejectsInvalidTargets()
    {
        async Task<FriendError> Error(Guid from, string username) =>
            (await Assert.ThrowsAsync<FriendException>(() => _service.SendRequestAsync(from, username))).Code;

        Assert.Equal(FriendError.UserNotFound, await Error(_alice.Id, "Nobody"));
        Assert.Equal(FriendError.CannotAddYourself, await Error(_alice.Id, "ALICE"));

        await _service.SendRequestAsync(_alice.Id, "Bob");
        Assert.Equal(FriendError.RequestAlreadySent, await Error(_alice.Id, "Bob"));

        await _service.AcceptRequestAsync(_bob.Id, _alice.Id);
        Assert.Equal(FriendError.AlreadyFriends, await Error(_alice.Id, "Bob"));
        Assert.Equal(FriendError.AlreadyFriends, await Error(_bob.Id, "Alice"));
    }

    [Fact]
    public async Task Requests_CanBeDeclinedAndCancelled_OnlyByTheirSides()
    {
        await _service.SendRequestAsync(_alice.Id, "Bob");
        await _service.SendRequestAsync(_alice.Id, "Carol");

        // Принять или отклонить может только получатель, отозвать — только отправитель
        await Assert.ThrowsAsync<FriendException>(() => _service.AcceptRequestAsync(_alice.Id, _bob.Id));
        await Assert.ThrowsAsync<FriendException>(() => _service.DeclineRequestAsync(_alice.Id, _bob.Id));
        await Assert.ThrowsAsync<FriendException>(() => _service.CancelRequestAsync(_bob.Id, _alice.Id));

        await _service.DeclineRequestAsync(_bob.Id, _alice.Id);
        await _service.CancelRequestAsync(_alice.Id, _carol.Id);

        Assert.Equal(0, await _context.FriendRequests.CountAsync());
        await Assert.ThrowsAsync<FriendException>(() => _service.AcceptRequestAsync(_bob.Id, _alice.Id));

        // После отказа можно позвать снова
        await _service.SendRequestAsync(_alice.Id, "Bob");
    }

    [Fact]
    public async Task RemoveFriend_RemovesBothSides()
    {
        await _service.SendRequestAsync(_alice.Id, "Bob");
        await _service.AcceptRequestAsync(_bob.Id, _alice.Id);

        await _service.RemoveFriendAsync(_bob.Id, _alice.Id);

        Assert.Empty((await _service.GetOverviewAsync(_alice.Id)).Friends);
        Assert.Empty((await _service.GetOverviewAsync(_bob.Id)).Friends);
        var error = await Assert.ThrowsAsync<FriendException>(() => _service.RemoveFriendAsync(_alice.Id, _bob.Id));
        Assert.Equal(FriendError.NotFriends, error.Code);

        // Помириться можно
        await MakeFriends(_alice, _bob);
        Assert.Single((await _service.GetOverviewAsync(_bob.Id)).Friends);
    }

    [Fact]
    public async Task SendRequest_RespectsOutgoingLimit()
    {
        var targets = Enumerable.Range(0, FriendService.MaxOutgoingRequests + 1).Select(i => AddUser($"Dwarf{i}")).ToList();
        await _context.SaveChangesAsync();

        foreach (var target in targets.Take(FriendService.MaxOutgoingRequests))
            await _service.SendRequestAsync(_alice.Id, target.Username);

        var error = await Assert.ThrowsAsync<FriendException>(() => _service.SendRequestAsync(_alice.Id, targets[^1].Username));
        Assert.Equal(FriendError.TooManyRequests, error.Code);
    }

    [Fact]
    public async Task Overview_ShowsPresence_OnlineFirst()
    {
        await MakeFriends(_alice, _bob);
        await MakeFriends(_alice, _carol);

        // Боб заходил и закрыл вкладку, Кэрол сейчас в игре
        var bobSession = await _sessions.StartAsync(_bob.Id, "test");
        _clock.Advance(TimeSpan.FromMinutes(10));
        await _sessions.EndAsync(_bob.Id, bobSession.SessionId);
        var bobLeftAt = _clock.UtcNow;

        _clock.Advance(TimeSpan.FromMinutes(30));
        await _sessions.StartAsync(_carol.Id, "test");

        var friends = (await _service.GetOverviewAsync(_alice.Id)).Friends;
        Assert.Equal(["Carol", "Bob"], friends.Select(f => f.Username));
        Assert.True(friends[0].IsOnline);
        Assert.False(friends[1].IsOnline);
        Assert.Equal(bobLeftAt, friends[1].LastSeenAt);
        Assert.Equal(DateTimeKind.Utc, friends[1].LastSeenAt!.Value.Kind);
    }

    [Fact]
    public async Task Overview_FriendWithoutSessions_HasNoLastSeen()
    {
        await MakeFriends(_alice, _bob);

        var friend = Assert.Single((await _service.GetOverviewAsync(_alice.Id)).Friends);
        Assert.False(friend.IsOnline);
        Assert.Null(friend.LastSeenAt);
    }

    private async Task MakeFriends(User from, User to)
    {
        await _service.SendRequestAsync(from.Id, to.Username);
        await _service.AcceptRequestAsync(to.Id, from.Id);
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
        return user;
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
