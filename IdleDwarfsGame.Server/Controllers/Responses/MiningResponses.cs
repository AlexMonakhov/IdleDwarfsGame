// Цена постройки или улучшения шахты. Кристаллы — гемы стихии Gem (обычные, 1-го уровня)
public class MineCost
{
    public int Gold { get; set; }
    public int Experience { get; set; }
    public GemType? Gem { get; set; }
    public int Crystals { get; set; }

    public MiningChange AsDebit() => new(-Gold, -Experience, Gem, -Crystals);
}

// Ресурсы, которыми платят за шахты и которые они добывают
public class MiningWallet
{
    public long Gold { get; set; }
    public long Experience { get; set; }
    // Обычные гемы 1-го уровня по камням; камней, которых нет, в словаре нет
    public Dictionary<GemType, int> Crystals { get; set; } = new();
    // Сила кирки на момент DigEnergyAt (время сервера в ответе); дальше фронт растит её по MiningBoard.DigEnergyPerHour
    public double DigEnergy { get; set; }
    public DateTime? DigEnergyAt { get; set; }
    // Уровни улучшений кирки (MiningService.PickaxeMaxLevel)
    public int PickaxeCapacityLevel { get; set; } = 1;
    public int PickaxeRecoveryLevel { get; set; } = 1;
    public int PickaxeStrengthLevel { get; set; } = 1;
}

// Улучшение кирки: Value — что даёт сейчас (запас кликов, кликов в час или процент добычи клика), Next — после улучшения
public class PickaxeUpgradeInfo
{
    public PickaxeUpgrade Upgrade { get; set; }
    public int Level { get; set; }
    public int MaxLevel { get; set; }
    // null — уровень максимальный
    public MineCost? Cost { get; set; }
    public double Value { get; set; }
    public double? Next { get; set; }
}

public class UpgradePickaxeRequest
{
    public PickaxeUpgrade Upgrade { get; set; }
}

public class MineUpgradeInfo
{
    public MineUpgrade Upgrade { get; set; }
    public int Level { get; set; }
    // Сейчас доступный максимум: у скорости, добычи и склада он растёт с уровнем шахты
    public int MaxLevel { get; set; }
    // null — дальше пока нельзя
    public MineCost? Cost { get; set; }
    // Что даёт улучшение сейчас и после следующего уровня: скорость — добыча дворфа в час,
    // добыча — ресурс за клик, склад — вместимость, уровень — сам уровень
    public double Value { get; set; }
    public double? Next { get; set; }
}

public class MineInfo
{
    public Guid Id { get; set; }
    public int Plot { get; set; }
    public MineType Type { get; set; }
    // Камень, который добывает шахта кристаллов; у золота и опыта — null
    public GemType? Gem { get; set; }
    public int Level { get; set; }
    public int MaxLevel { get; set; }
    // Сколько приносит клик — дробное (у кристаллов меньше единицы); накопленная кликами доля — DigProgress
    public double PerClick { get; set; }
    public double DigProgress { get; set; }
    // Добыча всех дворфов шахты в час (без дворфов — сколько даст один; сама шахта не копает) и одного дворфа
    public double RatePerHour { get; set; }
    public double RatePerDwarf { get; set; }
    // Склад растёт вместе с числом дворфов
    public int Capacity { get; set; }
    // Дворфы по местам; мест открыто DwarfSlots из MaxDwarves, следующее — на уровне NextSlotLevel
    public List<Guid> HeroIds { get; set; } = new();
    public int DwarfSlots { get; set; }
    public int MaxDwarves { get; set; }
    public int? NextSlotLevel { get; set; }
    // Накоплено дворфом на момент StoredAt (дробное — забирается целая часть)
    public double Stored { get; set; }
    public DateTime StoredAt { get; set; }
    public List<MineUpgradeInfo> Upgrades { get; set; } = new();
}

public class MiningBoard
{
    // По ним фронт поправляет свои часы для склада
    public DateTime ServerTime { get; set; }
    public int PlotCount { get; set; }
    public int DwarfUnlockLevel { get; set; }
    public int MaxClicksPerSecond { get; set; }
    // Сила кирки: запас кликов на все шахты и сколько восстанавливается в час
    public int DigEnergyMax { get; set; }
    public double DigEnergyPerHour { get; set; }
    public List<PickaxeUpgradeInfo> Pickaxe { get; set; } = new();
    // Цена следующей шахты
    public MineCost BuildCost { get; set; } = new();
    public List<MineInfo> Mines { get; set; } = new();
    public MiningWallet Wallet { get; set; } = new();
}

// Ответ на действие с шахтой: она сама и ресурсы игрока после него
public class MiningActionResult
{
    public MineInfo Mine { get; set; } = new();
    public MiningWallet Wallet { get; set; } = new();
}

public class MiningDigResult : MiningActionResult
{
    // Засчитанные клики (лишние сверх лимита отбрасываются) и сколько они принесли
    public int Clicks { get; set; }
    public int Mined { get; set; }
}

public class MiningCollectResult : MiningActionResult
{
    public int Collected { get; set; }
}
