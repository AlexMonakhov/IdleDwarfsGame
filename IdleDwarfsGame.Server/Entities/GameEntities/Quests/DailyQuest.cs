// Сколько раз игрок сделал действие за сутки (00:00 UTC). Ключ (PlayerId, Day, Kind).
// Прогресс ежедневного задания — это счётчик его действия за сегодня, поэтому действия до первого захода
// в задания тоже засчитываются. Старые сутки удаляет QuestRepository при генерации новой доски
public class PlayerDailyActivity
{
    public Guid PlayerId { get; set; }
    public DateTime Day { get; set; }
    public ActivityKind Kind { get; set; }
    public int Count { get; set; }
}

// Доска ежедневных заданий игрока: на какие сутки она собрана, потрачена ли сегодня бесплатная замена,
// какие пороги шкалы очков уже забраны (битовая маска по индексу DailyQuests.Milestones).
// Version — условные апдейты: из двух параллельных изменений проходит только первое
public class DailyQuestState
{
    public Guid PlayerId { get; set; }
    public DateTime Day { get; set; }
    public bool FreeRerollUsed { get; set; }
    public int MilestonesClaimed { get; set; }
    public int Version { get; set; }
}

// Задание на сутки. Цель, очки и награда фиксируются при генерации: золото и опыт зависят от прогресса кампании
// на тот момент, ивентовая валюта — от ивента, который шёл в эти сутки
public class PlayerQuest
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime Day { get; set; }
    // Место на доске 0..DailyQuests.SlotCount-1; сложность задаётся местом (DailyQuests.DifficultyOf)
    public int Slot { get; set; }

    // Шаблон из DailyQuests.Templates — по нему фронт пишет текст задания
    public string Key { get; set; } = string.Empty;
    public ActivityKind Kind { get; set; }
    public int Target { get; set; }

    public int Points { get; set; }
    public int ElderSeals { get; set; }
    public int EventCurrency { get; set; }
    public int Gold { get; set; }
    public int Experience { get; set; }

    // null — награду ещё не забрали
    public DateTime? ClaimedAt { get; set; }
}
