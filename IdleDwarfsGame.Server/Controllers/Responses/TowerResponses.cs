public class TowerFloorInfo
{
    public int Floor { get; set; }
    public HeroElement Element { get; set; }
    public StageType Type { get; set; }
    public StageStatus Status { get; set; }
    public ArenaInfo Arena { get; set; } = new();
    // Враги со статами уже под этаж (как они будут в бою)
    public List<CombatUnitInfo> Enemies { get; set; } = new();
    // Мощь отряда игрока против врагов этажа и оценка шанса победы (с учётом арены)
    public BattleForecast Forecast { get; set; } = new();
    // Что даст этаж: награда фиксированная, поэтому её видно заранее
    public RewardInfo Reward { get; set; } = new();
}

// Пройденный этаж в списке башни — без врагов и прогноза (их считать дорого, а бой за него не нужен)
public class ClearedFloorInfo
{
    public int Floor { get; set; }
    public HeroElement Element { get; set; }
    public StageType Type { get; set; }
    public string Background { get; set; } = string.Empty;
    // Этаж прошлой недели: награду за него так и не забрали, а башня с тех пор началась заново
    public bool PreviousWeek { get; set; }
    // Незабранная награда за этаж (null — уже забрана) и что в ней: награда башни фиксированная и видна заранее
    public Guid? RewardId { get; set; }
    public RewardInfo? Reward { get; set; }
}

public class TowerOverview
{
    // Неделя башни: с понедельника 00:00 UTC до следующего
    public DateTime WeekStart { get; set; }
    public DateTime WeekEndsAt { get; set; }
    // Стихии блоков по FloorsPerBlock этажей по порядку: первая — у этажей 1–5
    public List<HeroElement> Rotation { get; set; } = new();
    public int FloorsPerBlock { get; set; }

    // Последний пройденный за неделю этаж (0 — ещё ни одного) и тот, за который сейчас бой
    public int ClearedFloor { get; set; }
    public int CurrentFloor { get; set; }
    // Лучший результат прошлых недель; 0 — его нет
    public int BestFloor { get; set; }
    // До какого этажа можно пропустить разом (null — пропуск недоступен) и что он даст
    public int? SkipToFloor { get; set; }
    public RewardInfo? SkipReward { get; set; }

    // Пройденные этажи: все с незабранной наградой (и прошлых недель) и последний пройденный на этой неделе.
    // Награды не мешают идти дальше — их можно забрать по одной или все разом
    public List<ClearedFloorInfo> ClearedFloors { get; set; } = new();
    // Сколько всего незабранных наград башни — подсветка кнопки «Забрать всё»
    public int UnclaimedRewards { get; set; }
    // Отряд игрока один на все этажи, поэтому отдаётся один раз. Менять его можно между любыми боями
    public List<CombatUnitInfo> Squad { get; set; } = new();
    // Текущий этаж + несколько следующих заблокированных
    public List<TowerFloorInfo> Floors { get; set; } = new();
}

public class TowerFightResult
{
    public int Floor { get; set; }
    // Этаж пройден этим боем — за это выдана награда, её забирают в окне башни
    public bool Cleared { get; set; }
    public int CurrentFloor { get; set; }
    public HeroElement Element { get; set; }
    public ArenaInfo Arena { get; set; } = new();
    public CombatResult Combat { get; set; } = new();
}

public class TowerSkipResult
{
    // Последний пропущенный этаж; бой теперь за следующий
    public int SkippedTo { get; set; }
    public RewardInfo Reward { get; set; } = new();
    public PlayerResources Resources { get; set; } = new();
}
