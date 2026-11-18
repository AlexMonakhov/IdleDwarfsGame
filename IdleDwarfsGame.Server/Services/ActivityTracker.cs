// Учёт действий игрока: сервисы сообщают о каждом успешном действии, а задания (и дальше — обучение, ивенты)
// читают счётчики. Вызывается после того, как действие уже сохранено
public interface IActivityTracker
{
    Task RecordAsync(Guid playerId, ActivityKind kind, int amount = 1);
}

// Счётчики за сутки (PlayerDailyActivity) и за всё время (PlayerLifetimeActivity — достижения),
// а также отметка обучения «активность попробована» (PlayerTutorial.DoneAt).
// Счётчик — не деньги: если записать его не удалось, действие игрока уже прошло, поэтому ошибку только логируем,
// а не роняем ответ
public class ActivityTracker(
    IActivityRepository activity,
    TimeProvider time,
    ILogger<ActivityTracker> logger,
    ITutorialRepository? tutorials = null) : IActivityTracker
{
    // Отметку «попробовал» достаточно записать один раз: дальше те же действия (клики по шахте) в БД за ней не ходят
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(Guid, string), byte> MarkedDone = new();

    public async Task RecordAsync(Guid playerId, ActivityKind kind, int amount = 1)
    {
        if (amount <= 0)
            return;

        var now = time.GetUtcNow().UtcDateTime;
        try
        {
            await activity.AddAsync(playerId, DailyQuests.DayOf(now), kind, amount);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to record activity {Kind} x{Amount} for player {PlayerId}", kind, amount, playerId);
        }

        if (tutorials == null || TutorialActivities.KeyFor(kind) is not { } key || MarkedDone.ContainsKey((playerId, key)))
            return;
        try
        {
            await tutorials.MarkDoneAsync(playerId, key, now);
            MarkedDone.TryAdd((playerId, key), 0);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to mark tutorial activity {Key} done for player {PlayerId}", key, playerId);
        }
    }
}
