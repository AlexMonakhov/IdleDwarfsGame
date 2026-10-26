public class SessionStarted
{
    public Guid SessionId { get; set; }
    // Как часто клиенту продлевать сессию
    public int HeartbeatSeconds { get; set; }
}

public class SessionHeartbeat
{
    // Для значка на кнопке друзей — без отдельного опроса
    public int IncomingFriendRequests { get; set; }
}
