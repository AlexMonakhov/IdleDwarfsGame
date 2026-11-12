public class DelveOverview
{
    public DateTime ServerTime { get; set; }
    public DateTime WeekStart { get; set; }
    // Когда можно начать новый забег
    public DateTime NextDayAt { get; set; }
    public int Floors { get; set; }
    public int Columns { get; set; }
    public List<DelveNodeInfo> Map { get; set; } = new();
    // Последний забег недели (активный или законченный); null — на этой неделе не спускались
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
    public List<string> Offer { get; set; } = new();
    public List<string> Relics { get; set; } = new();
    // Живые герои с HP и реликвиями — как они выйдут в следующий бой
    public List<CombatUnitInfo> Units { get; set; } = new();
    public List<DelveFallenInfo> Fallen { get; set; } = new();
    public long GoldEarned { get; set; }
    public bool IsToday { get; set; }
}

public class DelveFallenInfo
{
    public Guid HeroId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public HeroElement Element { get; set; }
    public int Level { get; set; }
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
