// Что добывает шахта. Кристаллы — это гемы стихий (GemType), см. MiningService.GemOf.
// Хранится строкой: новые виды (валюта, кровавые кристаллы) просто добавляются в конец
public enum MineType
{
    Gold,
    Experience,
    Ruby,
    Sapphire,
    Topaz,
    Emerald,
    Amethyst,
    Diamond,
    Garnet
}

// Улучшения шахты: Level — уровень самой шахты (открывает дворфа и ограничивает остальные улучшения),
// Speed — скорость добычи дворфом, Yield — сколько приносит клик и дворф, Storage — склад, куда копит дворф
public enum MineUpgrade
{
    Level,
    Speed,
    Yield,
    Storage
}

// Улучшения кирки — общие на все шахты (хранятся у игрока): Capacity — запас кликов, Recovery — сколько кликов
// восстанавливается в час, Strength — сколько приносит клик
public enum PickaxeUpgrade
{
    Capacity,
    Recovery,
    Strength
}

// Шахта игрока на месте Plot (1..MiningService.PlotCount).
// Клик по шахте сразу начисляет ресурс игроку; дворф копит добычу в Stored, пока склад не заполнится.
// Добыча дворфа считается лениво: Stored посчитан на момент UpdatedAt, остальное досчитывается при чтении
public class Mine
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public int Plot { get; set; }
    public MineType Type { get; set; }

    public int Level { get; set; } = 1;
    public int SpeedLevel { get; set; } = 1;
    public int YieldLevel { get; set; } = 1;
    public int StorageLevel { get; set; } = 1;

    // Дворфы-добытчики (не больше MiningService.DwarfSlots(Level)); пусто — шахту копают только кликами
    public List<MineDwarf> Dwarves { get; set; } = new();

    // Накоплено дворфом и ещё не забрано (дробная часть копится между сборами)
    public double Stored { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Последняя пачка кликов: по ней ограничиваем частоту кликов
    public DateTime? LastDigAt { get; set; }

    // Накопленная кликами доля ресурса, ещё не дотянувшая до целой штуки: клик приносит дробную часть
    // (MiningService.ClickValue), целые зачисляются сразу, остаток ждёт следующих кликов
    public double DigProgress { get; set; }

    // Растёт с каждым изменением: из двух параллельных изменений шахты проходит только первое
    public int Version { get; set; }
}

// Дворф в шахте. HeroId уникален: один дворф работает только в одной шахте, даже при параллельных запросах
public class MineDwarf
{
    public Guid MineId { get; set; }
    public Guid HeroId { get; set; }
    // Место в шахте 1..MiningService.DwarfSlots(уровень шахты)
    public int Slot { get; set; }
}
