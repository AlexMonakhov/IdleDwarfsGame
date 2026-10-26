using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SessionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc));
    private readonly SessionService _service;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public SessionTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        AddUser(_userId, "Alice");
        AddUser(_otherUserId, "Bob");
        _context.SaveChanges();

        _service = new SessionService(new UserSessionRepository(_context), new FriendRepository(_context), _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Heartbeat_KeepsUserOnline_UntilWindowPasses()
    {
        var session = await _service.StartAsync(_userId, "Mozilla/5.0");
        Assert.Equal((int)SessionService.HeartbeatInterval.TotalSeconds, session.HeartbeatSeconds);
        Assert.True((await Presence()).IsOnline);

        _clock.Advance(SessionService.HeartbeatInterval);
        Assert.NotNull(await _service.HeartbeatAsync(_userId, session.SessionId));
        var seenAt = _clock.UtcNow;

        // Пинги перестали приходить (вкладку закрыли без сигнала) — после окна игрок офлайн, но время захода помним
        _clock.Advance(SessionService.OnlineWindow + TimeSpan.FromSeconds(1));
        var presence = await Presence();
        Assert.False(presence.IsOnline);
        Assert.Equal(seenAt, presence.LastSeenAt);
    }

    [Fact]
    public async Task End_MakesUserOffline_AndStopsHeartbeats()
    {
        var session = await _service.StartAsync(_userId, null);

        Assert.True(await _service.EndAsync(_userId, session.SessionId));
        Assert.False((await Presence()).IsOnline);
        Assert.Null(await _service.HeartbeatAsync(_userId, session.SessionId));
        Assert.False(await _service.EndAsync(_userId, session.SessionId));
    }

    [Fact]
    public async Task ForeignSession_CannotBeTouchedOrEnded()
    {
        var session = await _service.StartAsync(_userId, null);

        Assert.Null(await _service.HeartbeatAsync(_otherUserId, session.SessionId));
        Assert.False(await _service.EndAsync(_otherUserId, session.SessionId));
        Assert.True((await Presence()).IsOnline);
    }

    [Fact]
    public async Task Start_ClosesStaleSessions_ButKeepsLiveOnes()
    {
        var stale = await _service.StartAsync(_userId, null);
        _clock.Advance(TimeSpan.FromHours(1));
        var live = await _service.StartAsync(_userId, null);
        _clock.Advance(TimeSpan.FromSeconds(10));
        await _service.StartAsync(_userId, null);

        var sessions = await _context.UserSessions.AsNoTracking().ToListAsync();
        Assert.Equal(3, sessions.Count);
        // Протухшая сессия закрыта тем моментом, когда её видели последний раз
        var closed = sessions.Single(s => s.Id == stale.SessionId);
        Assert.Equal(closed.LastSeenAt, closed.EndedAt);
        Assert.Null(sessions.Single(s => s.Id == live.SessionId).EndedAt);
    }

    [Fact]
    public async Task Start_TruncatesLongUserAgent()
    {
        var session = await _service.StartAsync(_userId, new string('x', 1000));

        var stored = await _context.UserSessions.AsNoTracking().SingleAsync(s => s.Id == session.SessionId);
        Assert.Equal(SessionService.MaxUserAgentLength, stored.UserAgent!.Length);
    }

    [Fact]
    public async Task Heartbeat_ReportsIncomingFriendRequests()
    {
        var session = await _service.StartAsync(_userId, null);
        _context.FriendRequests.Add(new FriendRequest { FromUserId = _otherUserId, ToUserId = _userId, CreatedAt = _clock.UtcNow });
        await _context.SaveChangesAsync();

        var heartbeat = await _service.HeartbeatAsync(_userId, session.SessionId);
        Assert.Equal(1, heartbeat!.IncomingFriendRequests);
    }

    private async Task<UserPresence> Presence() => (await _service.GetPresenceAsync([_userId]))[_userId];

    private void AddUser(Guid id, string username)
    {
        var player = new Player { Id = Guid.NewGuid(), Name = username };
        _context.Users.Add(new User
        {
            Id = id,
            Username = username,
            NormalizedUsername = UserRepository.Normalize(username),
            Email = $"{username}@test.local",
            NormalizedEmail = UserRepository.Normalize($"{username}@test.local"),
            PlayerId = player.Id,
            Player = player
        });
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
