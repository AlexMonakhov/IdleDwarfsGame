// Прогресс игрока в бесконечной башне за одну неделю (InfinityTower.WeekOf). Одна строка на (игрок, неделя):
// в новую неделю башня начинается с первого этажа, а лучший этаж прошлых недель открывает пропуск
public class PlayerTowerProgress
{
    public Guid PlayerId { get; set; }
    // Понедельник 00:00 UTC
    public DateTime WeekStart { get; set; }

    // Последний пройденный за неделю этаж: этажи проходятся строго по порядку
    public int Floor { get; set; }
    public DateTime UpdatedAt { get; set; }
}
