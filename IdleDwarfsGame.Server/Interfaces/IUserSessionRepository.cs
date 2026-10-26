// Когда игрок последний раз был в игре и есть ли у него сейчас живая сессия
public record UserPresence(DateTime LastSeenAt, bool IsOnline);

public interface IUserSessionRepository
{
    /// <summary>
    /// Adds a new session. Open sessions of the same user not seen since staleBefore are closed
    /// at their LastSeenAt (the tab was closed without a signal).
    /// </summary>
    Task StartAsync(UserSession session, DateTime staleBefore);
    /// <summary>
    /// Extends an open session of the user. Returns false if there is no such open session.
    /// </summary>
    Task<bool> TouchAsync(Guid userId, Guid sessionId, DateTime now);
    /// <summary>
    /// Closes an open session of the user. Returns false if there is no such open session.
    /// </summary>
    Task<bool> EndAsync(Guid userId, Guid sessionId, DateTime now);
    /// <summary>
    /// Presence of the given users; a user is online if one of their open sessions was seen since onlineSince.
    /// Users that have never started a session are absent from the result.
    /// </summary>
    Task<Dictionary<Guid, UserPresence>> GetPresenceAsync(IReadOnlyCollection<Guid> userIds, DateTime onlineSince);
}
