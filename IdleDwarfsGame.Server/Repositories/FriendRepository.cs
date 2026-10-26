using Microsoft.EntityFrameworkCore;

public class FriendRepository(GameDbContext context) : IFriendRepository
{
    public async Task<bool> AreFriendsAsync(Guid userId, Guid otherUserId)
    {
        return await context.Friendships.AnyAsync(f => f.UserId == userId && f.FriendId == otherUserId);
    }

    public async Task<bool> RequestExistsAsync(Guid fromUserId, Guid toUserId)
    {
        return await context.FriendRequests.AnyAsync(r => r.FromUserId == fromUserId && r.ToUserId == toUserId);
    }

    public async Task<int> CountFriendsAsync(Guid userId)
    {
        return await context.Friendships.CountAsync(f => f.UserId == userId);
    }

    public async Task<int> CountOutgoingRequestsAsync(Guid userId)
    {
        return await context.FriendRequests.CountAsync(r => r.FromUserId == userId);
    }

    public async Task<int> CountIncomingRequestsAsync(Guid userId)
    {
        return await context.FriendRequests.CountAsync(r => r.ToUserId == userId);
    }

    public async Task<List<FriendEntry>> GetFriendsAsync(Guid userId)
    {
        return await context.Friendships
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => new FriendEntry(f.FriendId, f.Friend.Username, f.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<FriendRequestEntry>> GetIncomingRequestsAsync(Guid userId)
    {
        return await context.FriendRequests
            .AsNoTracking()
            .Where(r => r.ToUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new FriendRequestEntry(r.FromUserId, r.From.Username, r.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<FriendRequestEntry>> GetOutgoingRequestsAsync(Guid userId)
    {
        return await context.FriendRequests
            .AsNoTracking()
            .Where(r => r.FromUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new FriendRequestEntry(r.ToUserId, r.To.Username, r.CreatedAt))
            .ToListAsync();
    }

    public async Task<bool> AddRequestAsync(FriendRequest request)
    {
        await context.FriendRequests.AddAsync(request);
        try
        {
            await context.SaveChangesAsync();
            // Удаляются заявки через ExecuteDelete, мимо трекера — отслеживаемая копия помешала бы отправить заявку снова
            context.Entry(request).State = EntityState.Detached;
            return true;
        }
        catch (DbUpdateException)
        {
            // Такую же заявку только что отправил параллельный запрос (ключ FromUserId + ToUserId)
            context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> DeleteRequestAsync(Guid fromUserId, Guid toUserId)
    {
        int deleted = await context.FriendRequests
            .Where(r => r.FromUserId == fromUserId && r.ToUserId == toUserId)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }

    public async Task<bool> AcceptRequestAsync(Guid fromUserId, Guid toUserId, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Удаление заявки и есть проверка: из двух одновременных принятий пройдёт только первое
        int accepted = await context.FriendRequests
            .Where(r => r.FromUserId == fromUserId && r.ToUserId == toUserId)
            .ExecuteDeleteAsync();
        if (accepted == 0)
            return false;

        // Встречная заявка (игроки отправили друг другу) больше не нужна
        await context.FriendRequests
            .Where(r => r.FromUserId == toUserId && r.ToUserId == fromUserId)
            .ExecuteDeleteAsync();

        Friendship[] rows =
        [
            new Friendship { UserId = fromUserId, FriendId = toUserId, CreatedAt = now },
            new Friendship { UserId = toUserId, FriendId = fromUserId, CreatedAt = now }
        ];
        await context.Friendships.AddRangeAsync(rows);
        try
        {
            await context.SaveChangesAsync();
            // Дружба удаляется через ExecuteDelete, мимо трекера — как и с заявками, не держим копии
            foreach (var row in rows)
                context.Entry(row).State = EntityState.Detached;
        }
        catch (DbUpdateException)
        {
            // Уже друзья — заявку всё равно убираем
            context.ChangeTracker.Clear();
        }

        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> RemoveFriendAsync(Guid userId, Guid friendId)
    {
        int removed = await context.Friendships
            .Where(f => (f.UserId == userId && f.FriendId == friendId) || (f.UserId == friendId && f.FriendId == userId))
            .ExecuteDeleteAsync();
        return removed > 0;
    }
}
