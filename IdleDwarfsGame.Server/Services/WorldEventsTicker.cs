// Фоновая задача ивентов: раз в Interval создаёт месторождения общих шахт по расписанию и закрывает выработанные.
// Без неё всё то же самое делает первый заход игрока, но тогда дворфы с выработанного месторождения висели бы занятыми,
// пока кто-нибудь не откроет карту. Мировым боссам задача не нужна: следующий этап создаёт первый же запрос.
//
// Работает только пока в игре кто-то есть (запросы за последние ActiveWindow): без игроков догонять некому,
// а первый же заход сделает тик сам. Иначе запрос раз в минуту не даёт serverless-базе заснуть
public class WorldEventsTicker(IServiceScopeFactory scopes, RequestActivity activity, ILogger<WorldEventsTicker> logger) : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!activity.WasActiveWithin(ActiveWindow))
                continue;
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<CommunityMineService>().TickAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "World events tick failed");
            }
        }
    }
}
