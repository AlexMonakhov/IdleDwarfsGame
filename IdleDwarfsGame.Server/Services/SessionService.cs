// Сессии — заходы в игру. Каждая вкладка открывает свою сессию и раз в HeartbeatInterval продлевает её;
// по ним считается, в сети ли игрок и когда он был в игре.
public class SessionService(IUserSessionRepository sessions, IFriendRepository friends, TimeProvider time)
{
    public static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(60);
    // Фоновые вкладки браузер будит не чаще раза в минуту — один пропущенный пинг ещё не делает игрока офлайн
    public static readonly TimeSpan OnlineWindow = TimeSpan.FromSeconds(150);
    public const int MaxUserAgentLength = 256;

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<SessionStarted> StartAsync(Guid userId, string? userAgent)
    {
        var now = Now;
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartedAt = now,
            LastSeenAt = now,
            UserAgent = string.IsNullOrWhiteSpace(userAgent)
                ? null
                : userAgent.Length > MaxUserAgentLength ? userAgent[..MaxUserAgentLength] : userAgent
        };

        await sessions.StartAsync(session, now - OnlineWindow);
        return new SessionStarted { SessionId = session.Id, HeartbeatSeconds = (int)HeartbeatInterval.TotalSeconds };
    }

    // null — сессия закрыта или чужая: клиенту нужно открыть новую
    public async Task<SessionHeartbeat?> HeartbeatAsync(Guid userId, Guid sessionId)
    {
        if (!await sessions.TouchAsync(userId, sessionId, Now))
            return null;

        return new SessionHeartbeat { IncomingFriendRequests = await friends.CountIncomingRequestsAsync(userId) };
    }

    public Task<bool> EndAsync(Guid userId, Guid sessionId) => sessions.EndAsync(userId, sessionId, Now);

    public async Task<Dictionary<Guid, UserPresence>> GetPresenceAsync(IReadOnlyCollection<Guid> userIds)
    {
        var presence = await sessions.GetPresenceAsync(userIds, Now - OnlineWindow);
        // SQLite возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
        return presence.ToDictionary(
            p => p.Key,
            p => p.Value with { LastSeenAt = DateTime.SpecifyKind(p.Value.LastSeenAt, DateTimeKind.Utc) });
    }
}
