/* ===== Помощь в шахте друга ===== */

public class FriendMinesOverview
{
    public Guid FriendId { get; set; }
    public string FriendName { get; set; } = string.Empty;
    public double HelpHours { get; set; }
    public bool HelpedToday { get; set; }
    public int HelpsGiven { get; set; }
    public int MaxHelpsGiven { get; set; }
    public int FriendHelpsReceived { get; set; }
    public int MaxHelpsReceived { get; set; }
    public List<FriendMineInfo> Mines { get; set; } = new();
}

public class FriendMineInfo
{
    public Guid MineId { get; set; }
    public int Plot { get; set; }
    public MineType Type { get; set; }
    public int Level { get; set; }
    public int Dwarves { get; set; }
    public double Stored { get; set; }
    public int Capacity { get; set; }
    // Сколько добавит помощь сейчас (0 — склад полон или нет дворфов)
    public double HelpAmount { get; set; }
}

public class FriendHelpResult
{
    public double Added { get; set; }
    public long Gold { get; set; }
    public PlayerResources Resources { get; set; } = new();
    public FriendMinesOverview Mines { get; set; } = new();
}

/* ===== Наёмники ===== */

public class MercenaryOverview
{
    public int HiresLeft { get; set; }
    public int MaxHires { get; set; }
    public int LenderElderSeals { get; set; }
    public List<MercenaryFriendInfo> Friends { get; set; } = new();
}

public class MercenaryFriendInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool HiredToday { get; set; }
    public List<MercenaryHeroInfo> Heroes { get; set; } = new();
}

public class MercenaryHeroInfo
{
    public Guid HeroId { get; set; }
    public CombatUnitInfo Unit { get; set; } = new();
}

public class MercenaryFightRequest
{
    public Guid LenderUserId { get; set; }
    public Guid HeroId { get; set; }
    // Место в отряде 1..5, которое займёт наёмник
    public int Slot { get; set; }
}

public class MercenaryFightResult
{
    public CampaignFightResult Fight { get; set; } = new();
    public string MercenaryName { get; set; } = string.Empty;
    public int Slot { get; set; }
}

/* ===== Ставки на «Бой дня» ===== */

public class ArenaBetOverview
{
    public DateTime ServerTime { get; set; }
    // null — на сервере меньше двух отрядов
    public ArenaBetMatchInfo? Current { get; set; }
    // Последний проведённый бой
    public ArenaBetMatchInfo? Previous { get; set; }
    // Сколько монет ждёт выплаты за проведённые бои
    public long Claimable { get; set; }
    public long MinBet { get; set; }
    public long MaxBet { get; set; }
    public double HouseCut { get; set; }
    public PlayerResources Resources { get; set; } = new();
}

public class ArenaBetMatchInfo
{
    public DateTime WindowStart { get; set; }
    public DateTime ClosesAt { get; set; }
    public bool Open { get; set; }
    public ArenaBetSideInfo A { get; set; } = new();
    public ArenaBetSideInfo B { get; set; } = new();
    // 1 / 2; null — бой ещё не проведён
    public int? Winner { get; set; }
    public int Rounds { get; set; }
    public ArenaMyBetInfo? MyBet { get; set; }
}

public class ArenaBetSideInfo
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Power { get; set; }
    public long Pool { get; set; }
    public int Bettors { get; set; }
    // Во сколько раз вырастет ставка при нынешнем банке
    public double Multiplier { get; set; }
    public List<CombatUnitInfo> Units { get; set; } = new();
}

public class ArenaMyBetInfo
{
    public int Side { get; set; }
    public long Amount { get; set; }
    public long? Payout { get; set; }
    public bool Claimed { get; set; }
}

public class ArenaBetRequest
{
    public int Side { get; set; }
    public long Amount { get; set; }
}

public class ArenaBetReplay
{
    public string AName { get; set; } = string.Empty;
    public string BName { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public CombatResult Combat { get; set; } = new();
}
