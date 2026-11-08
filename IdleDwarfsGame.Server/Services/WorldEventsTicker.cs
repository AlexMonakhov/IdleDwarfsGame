// Фоновая задача ивентов: раз в Interval создаёт месторождения общих шахт по расписанию и закрывает выработанные.
// Без неё всё то же самое делает первый заход игрока, но тогда дворфы с выработанного месторождения висели бы занятыми,
// пока кто-нибудь не откроет карту. Мировым боссам задача не нужна: следующий этап создаёт первый же запрос
public class WorldEventsTicker(IServiceScopeFactory scopes, ILogger<WorldEventsTicker> logger) : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
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
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
