public class PlayerResources
{
    public long Gold { get; set; }
    public long Experience { get; set; }
}

public class ExpeditionReward
{
    public int Gold { get; set; }
    public int Experience { get; set; }
}

// Награда задания как функция мощи: Gold = BaseGold + GoldPerPower × мощь (округление до целого).
// Длительность уже учтена — фронт по этим числам показывает награду, пока игрок подбирает героев
public class ExpeditionRates
{
    public double BaseGold { get; set; }
    public double GoldPerPower { get; set; }
    public double BaseExperience { get; set; }
    public double ExperiencePerPower { get; set; }
}

public class ExpeditionInfo
{
    public Guid Id { get; set; }
    public int Slot { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public ExpeditionStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public ExpeditionRates Rates { get; set; } = new();
    // Мощь отправленных героев и зафиксированная награда; у доступных заданий — 0
    public int Power { get; set; }
    public ExpeditionReward Reward { get; set; } = new();
    // Герои на задании по ячейкам
    public List<Guid> HeroIds { get; set; } = new();
}

public class ExpeditionBoard
{
    // По ним фронт поправляет свои часы для таймеров
    public DateTime ServerTime { get; set; }
    // Когда доска сменится новыми заданиями
    public DateTime RefreshAt { get; set; }
    public int MaxHeroes { get; set; }
    // Задания текущей доски по местам + запущенные со старых досок
    public List<ExpeditionInfo> Expeditions { get; set; } = new();
}

public class ExpeditionClaimResult
{
    public ExpeditionReward Reward { get; set; } = new();
    // Ресурсы игрока после начисления
    public PlayerResources Resources { get; set; } = new();
}
