public class DelveOverview
{
    public DateTime ServerTime { get; set; }
    public DateTime PeriodStart { get; set; }
    // Когда можно начать новый забег (начало следующего периода)
    public DateTime NextRunAt { get; set; }
    public int Columns { get; set; }
    public int BossEvery { get; set; }
    public int ReviveCost { get; set; }
    // Последний забег периода (активный или законченный); null — в этом периоде не спускались
    public DelveRunInfo? Run { get; set; }
    public bool CanStart { get; set; }
    public int BestFloor { get; set; }
    public List<DelveLeaderInfo> Leaderboard { get; set; } = new();
    public List<DelveRelicInfo> Relics { get; set; } = new();
}

public class DelveNodeInfo
{
    public int Floor { get; set; }
    public int Column { get; set; }
    public DelveNodeType Type { get; set; }
}

public class DelveRunInfo
{
    public Guid Id { get; set; }
    public DelveRunStatus Status { get; set; }
    public int Floor { get; set; }
    public int Column { get; set; }
    public DelvePending Pending { get; set; }
    // Реликвии на выбор (Pending == Relic)
    public List<string> Offer { get; set; } = new();
    // Награды на выбор (Pending == Reward)
    public List<DelveRewardInfo> Rewards { get; set; } = new();
    public List<string> Relics { get; set; } = new();
    // Отряд по местам, павшие — с нулевым HP
    public List<DelveDwarfInfo> Dwarves { get; set; } = new();
    // Весь следующий этаж и узел через этаж под текущим столбцом; дальше карта скрыта
    public List<DelveNodeInfo> Next { get; set; } = new();
    public DelveNodeInfo? Peek { get; set; }
    public long GoldEarned { get; set; }
    public long ExperienceEarned { get; set; }
    public int RainbowCoinsEarned { get; set; }
    public bool Revived { get; set; }
    public bool CanRevive { get; set; }
}

public class DelveDwarfInfo
{
    public Guid HeroId { get; set; }
    public string Name { get; set; } = string.Empty;
    public HeroElement Element { get; set; }
    public int Level { get; set; }
    public int Position { get; set; }
    // Доля HP 0..1; 0 — пал
    public double Hp { get; set; }
    // Как дворф выйдет в бой (статы с предметами и реликвиями) — для портрета и подсказки
    public CombatUnitInfo Unit { get; set; } = new();
    public int? SpellId { get; set; }
    public string? SpellName { get; set; }
    public int SpellLevel { get; set; }
    public string? ItemKey { get; set; }
    public HeroPassive? Passive { get; set; }
    public int PassiveLevel { get; set; }
    public PassiveValues? PassiveValues { get; set; }
    public string? AuraEffect { get; set; }
    public int AuraLevel { get; set; }
}

public class DelveRewardInfo
{
    public DelveRewardKind Kind { get; set; }
    public string? Key { get; set; }
    public int Level { get; set; }
    public int? SpellId { get; set; }
    public string? SpellName { get; set; }
    public HeroElement? Element { get; set; }
    public Rarity? Rarity { get; set; }
    // У пассивки — её числа на этом уровне (для описания)
    public PassiveValues? PassiveValues { get; set; }
    // Награду надевают на выбранного дворфа; у предмета — только на подходящего по стихии (Fits)
    public bool NeedsTarget { get; set; }
    public List<Guid> Fits { get; set; } = new();
}

public class DelveLeaderInfo
{
    public int Place { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Floor { get; set; }
    public bool IsMe { get; set; }
}

public class DelveRelicInfo
{
    public string Key { get; set; } = string.Empty;
    public bool Rare { get; set; }
}

public class DelveMoveResult
{
    public DelveNodeType Node { get; set; }
    // Бой — только у узлов-боёв
    public CombatResult? Combat { get; set; }
    public string? Background { get; set; }
    public bool? Won { get; set; }
    public long Gold { get; set; }
    public long Experience { get; set; }
    public int RainbowCoins { get; set; }
    public DelveOverview Overview { get; set; } = new();
}

// Отказ от награды: золото и опыт на аккаунт
public class DelveDeclineResult
{
    public long Gold { get; set; }
    public long Experience { get; set; }
    public DelveOverview Overview { get; set; } = new();
}

public class DelveMoveRequest
{
    public int Column { get; set; }
}

public class DelveRelicRequest
{
    public string Key { get; set; } = string.Empty;
}

public class DelveCampRequest
{
    // rest | revive
    public string Action { get; set; } = string.Empty;
    public Guid? HeroId { get; set; }
}

public class DelveRewardRequest
{
    public int Index { get; set; }
    // Дворф для наград с NeedsTarget
    public Guid? HeroId { get; set; }
}

public class DelveOrderRequest
{
    // Все дворфы отряда в новом порядке мест
    public List<Guid> HeroIds { get; set; } = new();
}
