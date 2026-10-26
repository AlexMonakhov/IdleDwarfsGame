public class FriendInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    // Есть открытая вкладка с игрой
    public bool IsOnline { get; set; }
    // Когда последний раз был в игре; null — ни разу с тех пор, как появились сессии
    public DateTime? LastSeenAt { get; set; }
    public DateTime FriendsSince { get; set; }
}

public class FriendRequestInfo
{
    // Входящая заявка — отправитель, исходящая — получатель
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FriendsOverview
{
    // По ним фронт считает «был в сети 5 минут назад» без расхождения часов
    public DateTime ServerTime { get; set; }
    public int MaxFriends { get; set; }
    // Сначала те, кто в сети, затем по давности последнего захода
    public List<FriendInfo> Friends { get; set; } = new();
    public List<FriendRequestInfo> Incoming { get; set; } = new();
    public List<FriendRequestInfo> Outgoing { get; set; } = new();
}

// Accepted — у получателя уже была встречная заявка, и игроки сразу стали друзьями
public enum FriendRequestOutcome { Sent, Accepted }

public class SendFriendRequestResult
{
    public FriendRequestOutcome Outcome { get; set; }
}
