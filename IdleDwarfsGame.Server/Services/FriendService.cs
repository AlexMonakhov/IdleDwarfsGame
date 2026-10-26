// Коды ошибок уходят на фронт строкой — по ним он показывает переведённый текст (friends.errors.<code>)
public enum FriendError
{
    UserNotFound,
    CannotAddYourself,
    AlreadyFriends,
    RequestAlreadySent,
    RequestNotFound,
    NotFriends,
    FriendLimit,
    TargetFriendLimit,
    TooManyRequests
}

public class FriendException(FriendError code, string message) : InvalidOperationException(message)
{
    public FriendError Code { get; } = code;
}

// Друзья между аккаунтами (User), а не игроками: у демо-игроков без аккаунта друзей нет.
// Дружба взаимная: игрок отправляет заявку по имени, второй принимает её. Если оба отправили заявки друг другу —
// вторая заявка сразу делает их друзьями.
public class FriendService(
    IFriendRepository friends,
    IUserRepository users,
    SessionService sessions,
    TimeProvider time
)
{
    public const int MaxFriends = 100;
    // Сколько неотвеченных заявок можно держать одновременно — защита от рассылки всем подряд
    public const int MaxOutgoingRequests = 30;

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<FriendsOverview> GetOverviewAsync(Guid userId)
    {
        var list = await friends.GetFriendsAsync(userId);
        var incoming = await friends.GetIncomingRequestsAsync(userId);
        var outgoing = await friends.GetOutgoingRequestsAsync(userId);
        var presence = await sessions.GetPresenceAsync(list.Select(f => f.UserId).ToList());

        var friendInfos = list
            .Select(f =>
            {
                var seen = presence.GetValueOrDefault(f.UserId);
                return new FriendInfo
                {
                    UserId = f.UserId,
                    Username = f.Username,
                    IsOnline = seen?.IsOnline ?? false,
                    LastSeenAt = seen?.LastSeenAt,
                    FriendsSince = AsUtc(f.Since)
                };
            })
            .OrderByDescending(f => f.IsOnline)
            .ThenByDescending(f => f.LastSeenAt ?? DateTime.MinValue)
            .ThenBy(f => f.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new FriendsOverview
        {
            ServerTime = Now,
            MaxFriends = MaxFriends,
            Friends = friendInfos,
            Incoming = incoming.Select(ToRequestInfo).ToList(),
            Outgoing = outgoing.Select(ToRequestInfo).ToList()
        };
    }

    public async Task<SendFriendRequestResult> SendRequestAsync(Guid userId, string username)
    {
        var target = await users.GetByUsernameAsync(username.Trim())
            ?? throw new FriendException(FriendError.UserNotFound, "Player not found");

        if (target.Id == userId)
            throw new FriendException(FriendError.CannotAddYourself, "You can't add yourself");
        if (await friends.AreFriendsAsync(userId, target.Id))
            throw new FriendException(FriendError.AlreadyFriends, "You are already friends");

        // Игрок сам уже звал нас в друзья — отправка встречной заявки равна согласию
        if (await friends.RequestExistsAsync(target.Id, userId))
        {
            await AcceptRequestAsync(userId, target.Id);
            return new SendFriendRequestResult { Outcome = FriendRequestOutcome.Accepted };
        }

        if (await friends.RequestExistsAsync(userId, target.Id))
            throw new FriendException(FriendError.RequestAlreadySent, "Friend request is already sent");
        if (await friends.CountFriendsAsync(userId) >= MaxFriends)
            throw new FriendException(FriendError.FriendLimit, $"You can't have more than {MaxFriends} friends");
        if (await friends.CountOutgoingRequestsAsync(userId) >= MaxOutgoingRequests)
            throw new FriendException(FriendError.TooManyRequests, $"You can't have more than {MaxOutgoingRequests} pending requests");

        var request = new FriendRequest { FromUserId = userId, ToUserId = target.Id, CreatedAt = Now };
        if (!await friends.AddRequestAsync(request))
            throw new FriendException(FriendError.RequestAlreadySent, "Friend request is already sent");

        return new SendFriendRequestResult { Outcome = FriendRequestOutcome.Sent };
    }

    // userId принимает заявку от fromUserId
    public async Task AcceptRequestAsync(Guid userId, Guid fromUserId)
    {
        if (!await friends.RequestExistsAsync(fromUserId, userId))
            throw new FriendException(FriendError.RequestNotFound, "Friend request not found");
        if (await friends.CountFriendsAsync(userId) >= MaxFriends)
            throw new FriendException(FriendError.FriendLimit, $"You can't have more than {MaxFriends} friends");
        if (await friends.CountFriendsAsync(fromUserId) >= MaxFriends)
            throw new FriendException(FriendError.TargetFriendLimit, "This player's friend list is full");

        if (!await friends.AcceptRequestAsync(fromUserId, userId, Now))
            throw new FriendException(FriendError.RequestNotFound, "Friend request not found");
    }

    public async Task DeclineRequestAsync(Guid userId, Guid fromUserId)
    {
        if (!await friends.DeleteRequestAsync(fromUserId, userId))
            throw new FriendException(FriendError.RequestNotFound, "Friend request not found");
    }

    public async Task CancelRequestAsync(Guid userId, Guid toUserId)
    {
        if (!await friends.DeleteRequestAsync(userId, toUserId))
            throw new FriendException(FriendError.RequestNotFound, "Friend request not found");
    }

    public async Task RemoveFriendAsync(Guid userId, Guid friendId)
    {
        if (!await friends.RemoveFriendAsync(userId, friendId))
            throw new FriendException(FriendError.NotFriends, "This player is not your friend");
    }

    private static FriendRequestInfo ToRequestInfo(FriendRequestEntry entry) => new()
    {
        UserId = entry.UserId,
        Username = entry.Username,
        CreatedAt = AsUtc(entry.CreatedAt)
    };

    // SQLite возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
