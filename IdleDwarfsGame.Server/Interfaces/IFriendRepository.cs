public record FriendEntry(Guid UserId, string Username, DateTime Since);
public record FriendRequestEntry(Guid UserId, string Username, DateTime CreatedAt);

public interface IFriendRepository
{
    Task<bool> AreFriendsAsync(Guid userId, Guid otherUserId);
    Task<bool> RequestExistsAsync(Guid fromUserId, Guid toUserId);
    Task<int> CountFriendsAsync(Guid userId);
    Task<int> CountOutgoingRequestsAsync(Guid userId);
    Task<int> CountIncomingRequestsAsync(Guid userId);

    Task<List<FriendEntry>> GetFriendsAsync(Guid userId);
    /// <summary>
    /// Requests sent to the user; UserId of an entry is the sender.
    /// </summary>
    Task<List<FriendRequestEntry>> GetIncomingRequestsAsync(Guid userId);
    /// <summary>
    /// Requests sent by the user; UserId of an entry is the receiver.
    /// </summary>
    Task<List<FriendRequestEntry>> GetOutgoingRequestsAsync(Guid userId);

    /// <summary>
    /// Returns false if the same request already exists.
    /// </summary>
    Task<bool> AddRequestAsync(FriendRequest request);
    /// <summary>
    /// Returns false if there was no such request.
    /// </summary>
    Task<bool> DeleteRequestAsync(Guid fromUserId, Guid toUserId);
    /// <summary>
    /// Removes the request (and the counter request, if any) and makes the users friends in one transaction.
    /// Returns false if there was no such request.
    /// </summary>
    Task<bool> AcceptRequestAsync(Guid fromUserId, Guid toUserId, DateTime now);
    /// <summary>
    /// Returns false if the users were not friends.
    /// </summary>
    Task<bool> RemoveFriendAsync(Guid userId, Guid friendId);
}
