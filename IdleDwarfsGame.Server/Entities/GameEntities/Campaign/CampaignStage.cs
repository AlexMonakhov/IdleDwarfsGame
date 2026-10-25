public enum StageType { Mobs, Boss }

// Уровень кампании. Id — номер уровня, уровни проходятся строго по порядку Id.
public class CampaignStage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StageType Type { get; set; }

    // Уровень, до которого масштабируются статы врагов (BalanceCalculator)
    public int EnemyLevel { get; set; } = 1;

    public int ArenaId { get; set; }
    public Arena Arena { get; set; } = null!;

    public List<CampaignStageEnemy> Enemies { get; set; } = new();
}

// Враг на уровне. Сам враг (или босс) — шаблон из таблицы Heroes со всеми его компонентами:
// спеллы, иммунитеты, аура и т.д. Здесь только то, что зависит от уровня.
public class CampaignStageEnemy
{
    public int Id { get; set; }
    public int StageId { get; set; }

    public Guid HeroId { get; set; }
    public Hero Hero { get; set; } = null!;

    // Позиция в отряде врагов (1 — передняя линия)
    public int Position { get; set; }

    public bool IsBoss { get; set; }

    // Боссы толще обычных врагов того же уровня
    public double HpMultiplier { get; set; } = 1;
}
