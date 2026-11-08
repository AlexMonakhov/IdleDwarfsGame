// Учёт действий игрока: сервисы сообщают о каждом успешном действии, а задания (и дальше — обучение, ивенты)
// читают счётчики. Вызывается после того, как действие уже сохранено
public interface IActivityTracker
{
    Task RecordAsync(Guid playerId, ActivityKind kind, int amount = 1);
}

// Счётчики за сутки (PlayerDailyActivity). Счётчик — не деньги: если записать его не удалось, действие игрока
// уже прошло, поэтому ошибку только логируем, а не роняем ответ
public class ActivityTracker(IActivityRepository activity, TimeProvider time, ILogger<ActivityTracker> logger) : IActivityTracker
{
    public async Task RecordAsync(Guid playerId, ActivityKind kind, int amount = 1)
    {
        if (amount <= 0)
            return;

        var day = DailyQuests.DayOf(time.GetUtcNow().UtcDateTime);
        try
        {
            await activity.AddAsync(playerId, day, kind, amount);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to record activity {Kind} x{Amount} for player {PlayerId}", kind, amount, playerId);
        }
    }
}
