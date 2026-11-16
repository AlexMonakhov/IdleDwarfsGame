// Прогресс обучения игрока по ключу: сценарий (знакомство с окном, стартовое обучение) и активность города.
// Ключи задаёт фронт (src/tutorial): 'warehouse', 'altar.abilities', 'onboarding'… Ключ (PlayerId, Key).
// Status — сценарий пройден или пропущен (null — ещё нет). DoneAt — когда игрок впервые сделал главное действие
// активности (ActivityTracker → TutorialActivities); по нему фронт подсказывает, куда ещё не заглядывали
public class PlayerTutorial
{
    public Guid PlayerId { get; set; }
    public string Key { get; set; } = string.Empty;
    public TutorialStatus? Status { get; set; }
    public DateTime? DoneAt { get; set; }
    // Шаг, на котором игрок остановился, — только у стартового обучения (Onboarding): после перезагрузки оно
    // продолжается с этого шага
    public int Step { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Хранится строкой — значения не переименовывать без миграции
public enum TutorialStatus
{
    Completed,
    Skipped
}
