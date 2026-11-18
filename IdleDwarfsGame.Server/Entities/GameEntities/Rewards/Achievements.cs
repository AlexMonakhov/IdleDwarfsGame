// Сколько раз игрок сделал действие за всё время. Ключ (PlayerId, Kind). Пишется вместе со счётчиком за сутки
// (ActivityRepository.AddAsync); по нему идут достижения на действия (Achievements)
public class PlayerLifetimeActivity
{
    public Guid PlayerId { get; set; }
    public ActivityKind Kind { get; set; }
    public long Count { get; set; }
}

// Сколько ступеней достижения игрок уже забрал. Ключ (PlayerId, Key); строки нет — не забрано ни одной.
// Ступени забираются строго по порядку, поэтому хватает числа
public class PlayerAchievement
{
    public Guid PlayerId { get; set; }
    // Ключ из Achievements.All
    public string Key { get; set; } = string.Empty;
    public int ClaimedTiers { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Календарь входов («месячный пропуск»): награда за каждый день подряд, к концу цикла всё лучше (LoginCalendar).
// Day — сколько дней текущего цикла уже забрано; LastClaimDay — сутки (00:00 UTC) последней награды.
// Пропуск суток обнуляет и цикл, и серию. Version — условные апдейты: из двух параллельных получений проходит одно
public class PlayerLoginCalendar
{
    public Guid PlayerId { get; set; }
    // Номер текущего цикла, с 1
    public int Cycle { get; set; }
    public int Day { get; set; }
    public DateTime? LastClaimDay { get; set; }

    // Серия дней подряд (не обрывается между циклами), лучшая серия и всего дней с наградой — для достижений
    public int Streak { get; set; }
    public int BestStreak { get; set; }
    public int TotalDays { get; set; }

    public int Version { get; set; }
}
