// Когда сервер в последний раз получал HTTP-запрос. По этому фоновые задачи понимают, есть ли кто-то в игре.
// Пока игроков нет, задачи не ходят в базу: бесплатная Azure SQL (serverless) успевает заснуть и не тратит лимит,
// а тариф App Service F1 не тратит CPU-квоту
public class RequestActivity(TimeProvider clock)
{
    // DateTimeOffset.UtcTicks последнего запроса; 0 — с момента старта запросов не было
    private long _lastTicks;

    public void Touch() => Interlocked.Exchange(ref _lastTicks, clock.GetUtcNow().UtcTicks);

    public bool WasActiveWithin(TimeSpan window)
    {
        long last = Interlocked.Read(ref _lastTicks);
        return last != 0 && clock.GetUtcNow().UtcTicks - last <= window.Ticks;
    }
}
