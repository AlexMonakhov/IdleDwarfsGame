// Вид месторождения ивента «Общие шахты»: размер (сколько руды и награды) — CommunityMineRules.SizeOf.
// Хранится строкой — значения не переименовывать без миграции
public enum DepositKind { Copper, Iron, Silver, Gold, Mithril }

// Месторождение на карте ивента — общее на сервер. Копают его дворфы всех игроков сразу.
// Добыча считается лениво, как у личных шахт: OreMined посчитано на момент UpdatedAt, дальше растёт по общей скорости
// (сумма скоростей дворфов × множитель улучшения). Каждое изменение состава или улучшение сначала засчитывает
// добытое по старой скорости (CommunityMineRules.Settle) — так у каждого игрока копится его доля.
// Выработано или ивент кончился — месторождение закрывается (ClosedAt): дворфы освобождаются, участники забирают награду
public class CommunityDeposit
{
    public Guid Id { get; set; }
    public DateTime EventStart { get; set; }
    // Номер месторождения в ивенте, с 0 — по нему место на карте и вид
    public int Index { get; set; }
    public DepositKind Kind { get; set; }

    public double OreTotal { get; set; }
    public double OreMined { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Улучшение скорости для всех (CommunityMineRules.SpeedFactor)
    public int SpeedLevel { get; set; } = 1;

    public DateTime SpawnedAt { get; set; }
    // Когда добыли последнюю руду; null — ещё не выработано
    public DateTime? ExhaustedAt { get; set; }
    // Когда закрыто (выработано или кончился ивент); null — открыто
    public DateTime? ClosedAt { get; set; }

    // Растёт с каждым изменением: из двух параллельных изменений проходит только первое
    public int Version { get; set; }
}

// Вклад игрока в месторождение: сколько руды накопали его дворфы (на момент CommunityDeposit.UpdatedAt)
// и сколько засчитано за улучшения. Ключ (DepositId, PlayerId)
public class DepositWorker
{
    public Guid DepositId { get; set; }
    public Guid PlayerId { get; set; }
    public double Mined { get; set; }
    public double UpgradePoints { get; set; }
    // Сколько уровней скорости этого месторождения купил игрок (не больше CommunityMineRules.MaxUpgradesPerPlayer)
    public int Upgrades { get; set; }
}

// Дворф на месторождении. HeroId уникален: дворф работает в одном месте — здесь, в личной шахте или в экспедиции.
// Rate — его руда в час без улучшения месторождения, считается при отправке (CommunityMineRules.DwarfRate)
public class DepositDwarf
{
    public Guid HeroId { get; set; }
    public Guid DepositId { get; set; }
    public Guid PlayerId { get; set; }
    public double Rate { get; set; }
}

// Полученная награда за закрытое месторождение. Ключ (DepositId, PlayerId)
public class DepositRewardClaim
{
    public Guid DepositId { get; set; }
    public Guid PlayerId { get; set; }
    public int Currency { get; set; }
    public DateTime ClaimedAt { get; set; }
}
