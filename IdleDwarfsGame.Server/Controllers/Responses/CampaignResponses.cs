public enum StageStatus { Available, Locked }

public class ArenaInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public string? EffectName { get; set; }
    public int EffectPower { get; set; }
    public int EffectDuration { get; set; }
}

public class CampaignStageInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StageType Type { get; set; }
    public int EnemyLevel { get; set; }
    public StageStatus Status { get; set; }
    public ArenaInfo Arena { get; set; } = new();
    // Враги со статами уже под уровень (как они будут в бою)
    public List<CombatUnitInfo> Enemies { get; set; } = new();
    // Мощь отряда игрока против врагов уровня и оценка шанса победы (с учётом арены)
    public BattleForecast Forecast { get; set; } = new();
}

public class CampaignOverview
{
    // null — кампания пройдена целиком
    public int? CurrentStageId { get; set; }
    // Незабранная награда за пройденный уровень: пока она есть, вместо боя — кнопка «Забрать награды».
    // Что в ней — фронт узнаёт только из ответа на получение
    public Guid? PendingRewardId { get; set; }
    public int ClearedCount { get; set; }
    public int TotalStages { get; set; }
    // Отряд игрока один на все уровни, поэтому отдаётся один раз
    public List<CombatUnitInfo> Squad { get; set; } = new();
    // Текущий уровень + несколько следующих заблокированных
    public List<CampaignStageInfo> Stages { get; set; } = new();
}

public class CampaignFightResult
{
    public int StageId { get; set; }
    // Уровень пройден впервые именно этим боем — за это выдана награда, её забирают в кампании
    public bool FirstClear { get; set; }
    // Уровень, доступный после боя (null — кампания пройдена)
    public int? CurrentStageId { get; set; }
    public ArenaInfo Arena { get; set; } = new();
    public CombatResult Combat { get; set; } = new();
}
