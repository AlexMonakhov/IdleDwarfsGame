// Ready в базе не хранится: это InProgress, у которого уже прошло EndsAt (см. ExpeditionInfo)
public enum ExpeditionStatus { Available, InProgress, Ready, Claimed }

// Задание с доски экспедиций. Доска игрока обновляется раз в ExpeditionService.BoardLifetime:
// невзятые задания старой доски удаляются, запущенные доживают до получения награды.
public class Expedition
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }

    // Начало окна доски, к которому относится задание, и его место на доске
    public DateTime BoardStartsAt { get; set; }
    public int Slot { get; set; }

    // Ключ шаблона (название и описание переводятся на фронте) и длительность
    public string TemplateKey { get; set; } = string.Empty;
    public int DurationHours { get; set; }

    public ExpeditionStatus Status { get; set; } = ExpeditionStatus.Available;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime? ClaimedAt { get; set; }

    // Фиксируются при старте: мощь отправленных героев и награда за задание
    public int Power { get; set; }
    public int Gold { get; set; }
    public int Experience { get; set; }

    // Строки есть только пока задание идёт — после получения награды герои свободны
    public List<ExpeditionHero> Heroes { get; set; } = new();
}

// Герой на задании. HeroId уникален: один герой не может быть сразу в двух заданиях
public class ExpeditionHero
{
    public Guid ExpeditionId { get; set; }
    public Guid HeroId { get; set; }
    // Ячейка в задании (1..ExpeditionService.MaxHeroes)
    public int Slot { get; set; }
}
