public enum StageStatus { Available, Locked }

public class ArenaInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public HeroElement Element { get; set; }
    public string? EffectName { get; set; }
    public int EffectPower { get; set; }
    public int EffectDuration { get; set; }

    public static ArenaInfo From(Arena arena) => new()
    {
        Id = arena.Id,
        Name = arena.Name,
        Description = arena.Description,
        Background = arena.Background,
        Element = arena.Element,
        EffectName = arena.EffectName,
        EffectPower = arena.EffectPower,
        EffectDuration = arena.EffectDuration
    };
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

// Пройденный уровень в списке кампании — без врагов и прогноза (их считать дорого, а бой за него не нужен):
// номер, название, фон арены и награда, если её ещё не забрали
public class ClearedStageInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StageType Type { get; set; }
    public string Background { get; set; } = string.Empty;
    public HeroElement Element { get; set; }
    // Незабранная награда за уровень (null — уже забрана). Что в ней — фронт узнаёт только из ответа на получение
    public Guid? RewardId { get; set; }
}

public class CampaignOverview
{
    // null — кампания пройдена целиком
    public int? CurrentStageId { get; set; }
    // Пройденные уровни по порядку: все с незабранной наградой и последний пройденный (перед текущим).
    // Награды не мешают идти дальше — их можно забрать по одной или все разом
    public List<ClearedStageInfo> ClearedStages { get; set; } = new();
    // Сколько всего незабранных наград кампании — подсветка кнопки «Забрать всё»
    public int UnclaimedRewards { get; set; }
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
