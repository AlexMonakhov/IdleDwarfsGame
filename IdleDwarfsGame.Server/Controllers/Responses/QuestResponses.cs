public class QuestInfo
{
    public Guid Id { get; set; }
    public int Slot { get; set; }
    // Шаблон (DailyQuests.Templates): по нему фронт пишет текст задания (quests.templates.<key>)
    public string Key { get; set; } = string.Empty;
    public ActivityKind Kind { get; set; }
    public QuestDifficulty Difficulty { get; set; }
    public int Target { get; set; }
    // Сделано за сегодня, не больше цели
    public int Progress { get; set; }
    public bool Completed { get; set; }
    public bool Claimed { get; set; }

    public int Points { get; set; }
    public int ElderSeals { get; set; }
    public int EventCurrency { get; set; }
    public int Gold { get; set; }
    public int Experience { get; set; }
}

public class QuestMilestoneInfo
{
    public int Index { get; set; }
    public int Points { get; set; }
    public bool Reached { get; set; }
    public bool Claimed { get; set; }
    // Золото, опыт и сундуки — посчитаны по текущему уровню кампании, как их и выдадут
    public RewardInfo Reward { get; set; } = new();
    public int ElderSeals { get; set; }
    // Только пока идёт ивент
    public int EventCurrency { get; set; }
}

// Ивент недели — коротко, для подписи у ивентовой валюты в заданиях
public class QuestEventInfo
{
    public WorldEventKind Kind { get; set; }
    public DateTime EndsAt { get; set; }
}

public class QuestBoard
{
    public DateTime Day { get; set; }
    public DateTime ResetAt { get; set; }
    public DateTime ServerTime { get; set; }

    // Очки забранных заданий и потолок шкалы
    public int Points { get; set; }
    public int MaxPoints { get; set; }

    public bool FreeReroll { get; set; }
    public int RerollCost { get; set; }

    // Сколько наград можно забрать прямо сейчас (задания и пороги) — для значка на кнопке
    public int Claimable { get; set; }

    public List<QuestInfo> Quests { get; set; } = new();
    public List<QuestMilestoneInfo> Milestones { get; set; } = new();
    // null — ивент сейчас не идёт (день лавки)
    public QuestEventInfo? Event { get; set; }
    public PlayerResources Resources { get; set; } = new();
}

public class QuestClaimResult
{
    public QuestBoard Board { get; set; } = new();
    public RewardInfo Reward { get; set; } = new();
    public int ElderSeals { get; set; }
    public int EventCurrency { get; set; }
}
