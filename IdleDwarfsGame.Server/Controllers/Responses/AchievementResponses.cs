// Что выдано одним получением: золото, опыт, сундуки (RewardInfo), валюты и открытые рамки
public class GrantedReward
{
    public RewardInfo Reward { get; set; } = new();
    public int RainbowCoins { get; set; }
    public int ElderSeals { get; set; }
    public List<string> Frames { get; set; } = new();
}

/* ===== Достижения ===== */

public class AchievementTierInfo
{
    public long Target { get; set; }
    public bool Reached { get; set; }
    public bool Claimed { get; set; }
    // Золото, опыт и сундуки — посчитаны по текущему уровню кампании, как их и выдадут
    public RewardInfo Reward { get; set; } = new();
    public int RainbowCoins { get; set; }
    public int ElderSeals { get; set; }
    // Рамка аватара за эту ступень
    public string? Frame { get; set; }
}

public class AchievementInfo
{
    // Ключ (Achievements.All) — по нему фронт пишет название и текст (achievements.list.<key>)
    public string Key { get; set; } = string.Empty;
    public AchievementMetric Metric { get; set; }
    public long Progress { get; set; }
    public int ClaimedTiers { get; set; }
    public int ReachedTiers { get; set; }
    public List<AchievementTierInfo> Tiers { get; set; } = new();
}

public class AchievementBoard
{
    public List<AchievementInfo> Achievements { get; set; } = new();
    // Ступеней, которые можно забрать прямо сейчас — значок на кнопке
    public int Claimable { get; set; }
    public int ClaimedTiers { get; set; }
    public int TotalTiers { get; set; }
    // Рамки аватара, открытые достижениями
    public List<string> UnlockedFrames { get; set; } = new();
    public PlayerResources Resources { get; set; } = new();
}

public class AchievementClaimResult
{
    public AchievementBoard Board { get; set; } = new();
    public GrantedReward Granted { get; set; } = new();
}

/* ===== Календарь входов ===== */

public class LoginDayInfo
{
    // 1..CycleLength
    public int Day { get; set; }
    public bool Claimed { get; set; }
    // Награда, которую можно забрать (или уже забрали) сегодня
    public bool Today { get; set; }
    // Последний день недели — крупная награда
    public bool Big { get; set; }
    public RewardInfo Reward { get; set; } = new();
    public int RainbowCoins { get; set; }
    public int ElderSeals { get; set; }
}

public class LoginCalendarInfo
{
    public int Cycle { get; set; }
    public int CycleLength { get; set; }
    // День цикла на сегодня
    public int Day { get; set; }
    public bool ClaimedToday { get; set; }
    public int ClaimedDays { get; set; }
    // Серия дней подряд по уже забранным дням (после пропуска — 0)
    public int Streak { get; set; }
    public int BestStreak { get; set; }
    public int TotalDays { get; set; }
    // Пропуск оборвал серию — календарь начался с первого дня
    public bool Broken { get; set; }
    public int MissedDaysAllowed { get; set; }

    public DateTime ResetAt { get; set; }
    public DateTime ServerTime { get; set; }
    public List<LoginDayInfo> Days { get; set; } = new();
    public PlayerResources Resources { get; set; } = new();
}

public class LoginClaimResult
{
    public LoginCalendarInfo Calendar { get; set; } = new();
    public GrantedReward Granted { get; set; } = new();
}
