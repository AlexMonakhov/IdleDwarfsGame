using System.Text.RegularExpressions;

public class TutorialException(string message) : InvalidOperationException(message);

// Прогресс обучения (PlayerTutorial): какие сценарии пройдены или пропущены и какие активности игрок уже пробовал.
// Сценарии описаны на фронте (src/tutorial), сервер хранит только их статусы по ключу
public partial class TutorialService(ITutorialRepository tutorials, ICampaignRepository campaign, TimeProvider time)
{
    // Ключ — как у сценариев на фронте: 'warehouse', 'altar.abilities'
    [GeneratedRegex("^[a-z][a-z0-9.-]{0,63}$")]
    private static partial Regex KeyPattern();

    // Сценариев на фронте несколько десятков; предел не даёт забить таблицу выдуманными ключами
    public const int MaxKeysPerPlayer = 200;

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<TutorialProgress> GetAsync(Guid playerId)
    {
        var rows = await tutorials.GetAsync(playerId);
        var cleared = await campaign.GetClearedStageIdsAsync(playerId);
        var onboarding = rows.FirstOrDefault(r => r.Key == Onboarding.Key);
        return new TutorialProgress
        {
            Onboarding = new OnboardingState { Active = onboarding is { Status: null }, Step = onboarding?.Step ?? 0 },
            Scenarios = rows.Where(r => r.Status != null).ToDictionary(r => r.Key, r => r.Status!.Value),
            Done = rows.Where(r => r.DoneAt != null).Select(r => r.Key).Order().ToList(),
            CampaignCleared = cleared.Count
        };
    }

    public async Task SetStatusAsync(Guid playerId, string key, TutorialStatus status)
    {
        if (!KeyPattern().IsMatch(key))
            throw new TutorialException($"Invalid tutorial key '{key}'");

        var rows = await tutorials.GetAsync(playerId);
        if (rows.All(r => r.Key != key) && rows.Count >= MaxKeysPerPlayer)
            throw new TutorialException("Too many tutorial keys");

        await tutorials.SetStatusAsync(playerId, key, status, Now);
    }

    // Несколько ключей одним статусом (пропуск стартового обучения: все окна знакомы). Неверный ключ — ошибка до записи
    public async Task SetStatusesAsync(Guid playerId, IReadOnlyCollection<string> keys, TutorialStatus status)
    {
        var distinct = keys.Distinct().ToList();
        if (distinct.FirstOrDefault(k => !KeyPattern().IsMatch(k)) is { } invalid)
            throw new TutorialException($"Invalid tutorial key '{invalid}'");

        var rows = await tutorials.GetAsync(playerId);
        if (rows.Count + distinct.Count(k => rows.All(r => r.Key != k)) > MaxKeysPerPlayer)
            throw new TutorialException("Too many tutorial keys");

        foreach (var key in distinct)
            await tutorials.SetStatusAsync(playerId, key, status, Now);
    }

    // Стартовое обучение сбросом не начинается заново: его выдачи уже сделаны
    public Task ResetAsync(Guid playerId) => tutorials.ResetStatusesAsync(playerId, Now, [Onboarding.Key]);
}

public class TutorialProgress
{
    // Стартовое обучение: идёт ли и на каком шаге (Onboarding)
    public OnboardingState Onboarding { get; set; } = new();
    public Dictionary<string, TutorialStatus> Scenarios { get; set; } = new();
    // Активности, которые игрок уже пробовал (TutorialActivities)
    public List<string> Done { get; set; } = new();
    // Пройдено уровней кампании: по нему фронт решает, куда звать дальше (не в гильдию на второй минуте)
    public int CampaignCleared { get; set; }
}
