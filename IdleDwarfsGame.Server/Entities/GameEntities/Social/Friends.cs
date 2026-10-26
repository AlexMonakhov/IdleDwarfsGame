// Заявка в друзья. Живёт, пока получатель её не примет или не отклонит, либо отправитель не отзовёт.
// Ключ (FromUserId, ToUserId): повторно отправить заявку тому же игроку нельзя
public class FriendRequest
{
    public Guid FromUserId { get; set; }
    public User From { get; set; } = null!;

    public Guid ToUserId { get; set; }
    public User To { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

// Дружба хранится двумя строками — (A, B) и (B, A): друзья любого игрока читаются одним условием UserId == me,
// а ключ (UserId, FriendId) не даёт подружиться дважды даже при параллельных запросах
public class Friendship
{
    public Guid UserId { get; set; }

    public Guid FriendId { get; set; }
    public User Friend { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
