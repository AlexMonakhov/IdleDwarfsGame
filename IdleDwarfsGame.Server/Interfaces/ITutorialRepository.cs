public interface ITutorialRepository
{
    Task<List<PlayerTutorial>> GetAsync(Guid playerId);
    Task<PlayerTutorial?> GetOneAsync(Guid playerId, string key);
    Task<int> CountAsync(Guid playerId);
    Task SetStatusAsync(Guid playerId, string key, TutorialStatus status, DateTime now);
    // Отметить, что активность попробована. Уже отмеченную не трогает
    Task MarkDoneAsync(Guid playerId, string key, DateTime now);
    // Отметить ключ, если ещё не отмечен: true — отметили сейчас (выдача «один раз», Onboarding)
    Task<bool> TryMarkDoneAsync(Guid playerId, string key, DateTime now);
    Task SetStepAsync(Guid playerId, string key, int step, DateTime now);
    // Сбросить статусы сценариев; отметки «попробовал» остаются — это факт, а не прогресс обучения.
    // Ключи keep не трогаются (стартовое обучение сбросом не начинается заново)
    Task ResetStatusesAsync(Guid playerId, DateTime now, IReadOnlyCollection<string> keep);
}
