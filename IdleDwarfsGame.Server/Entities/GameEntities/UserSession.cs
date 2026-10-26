// Один заход в игру — одна открытая вкладка. Пока вкладка открыта, клиент раз в SessionService.HeartbeatInterval
// продлевает LastSeenAt; при закрытии вкладки или выходе ставится EndedAt.
// По этим строкам видно, когда игрок был в игре и в сети ли он сейчас, а позже — сколько у него открыто вкладок и устройств
public class UserSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    // null — сессия открыта (или вкладку закрыли без сигнала, тогда она протухает по LastSeenAt)
    public DateTime? EndedAt { get; set; }

    // Браузер/устройство, с которого зашли
    public string? UserAgent { get; set; }
}
