using Microsoft.EntityFrameworkCore;

public class UserSessionRepository(GameDbContext context) : IUserSessionRepository
{
    public async Task StartAsync(UserSession session, DateTime staleBefore)
    {
        await context.UserSessions
            .Where(s => s.UserId == session.UserId && s.EndedAt == null && s.LastSeenAt < staleBefore)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.EndedAt, x => x.LastSeenAt));

        await context.UserSessions.AddAsync(session);
        await context.SaveChangesAsync();
    }

    public async Task<bool> TouchAsync(Guid userId, Guid sessionId, DateTime now)
    {
        int touched = await context.UserSessions
            .Where(s => s.Id == sessionId && s.UserId == userId && s.EndedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastSeenAt, now));
        return touched > 0;
    }

    public async Task<bool> EndAsync(Guid userId, Guid sessionId, DateTime now)
    {
        int ended = await context.UserSessions
            .Where(s => s.Id == sessionId && s.UserId == userId && s.EndedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LastSeenAt, now)
                .SetProperty(x => x.EndedAt, now));
        return ended > 0;
    }

    public async Task<Dictionary<Guid, UserPresence>> GetPresenceAsync(IReadOnlyCollection<Guid> userIds, DateTime onlineSince)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, UserPresence>();

        var rows = await context.UserSessions
            .Where(s => userIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                LastSeenAt = g.Max(s => s.LastSeenAt),
                OpenSessions = g.Count(s => s.EndedAt == null && s.LastSeenAt >= onlineSince)
            })
            .ToListAsync();

        return rows.ToDictionary(r => r.UserId, r => new UserPresence(r.LastSeenAt, r.OpenSessions > 0));
    }
}
