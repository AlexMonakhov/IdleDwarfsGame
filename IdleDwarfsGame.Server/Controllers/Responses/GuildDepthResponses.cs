public class GuildDepthOverview
{
    public DateTime ServerTime { get; set; }
    public DateTime WeekStart { get; set; }
    // Конец гонки; дальше до NextWeekAt — итоги
    public DateTime EndsAt { get; set; }
    public DateTime NextWeekAt { get; set; }
    public bool Active { get; set; }

    // Подъёмник шахты гильдии — общее улучшение
    public int ShaftLevel { get; set; }
    public int MaxShaftLevel { get; set; }
    public double ShaftFactor { get; set; }
    public double NextShaftFactor { get; set; }
    // null — подъёмник на максимуме
    public long? ShaftCostGold { get; set; }

    // Свой дворф этой недели; null — ещё не ставили
    public GuildDepthDwarfInfo? MyDwarf { get; set; }
    // Дворф этой недели стоит в шахте другой гильдии (игрок сменил гильдию)
    public bool PlacedElsewhere { get; set; }

    // Своя гильдия в рейтинге: 0 — ещё не в рейтинге
    public double GuildScore { get; set; }
    public int GuildPlace { get; set; }
    public int GuildCount { get; set; }

    public List<GuildDepthMemberInfo> Members { get; set; } = new();
    public List<GuildDepthStandingInfo> Leaderboard { get; set; } = new();
    public List<GuildDepthRewardTier> Rewards { get; set; } = new();
    // Награда закончившейся гонки, которую можно забрать
    public GuildDepthPendingInfo? Pending { get; set; }
    public PlayerResources Resources { get; set; } = new();
}

public class GuildDepthDwarfInfo
{
    // null — дворфа отозвали
    public Guid? HeroId { get; set; }
    public string? Name { get; set; }
    public int Rank { get; set; }
    public HeroElement Element { get; set; }
    public HeroProfession? Profession { get; set; }
    public double BaseRate { get; set; }
    // Руда в час со всеми улучшениями
    public double Rate { get; set; }

    public int PickLevel { get; set; }
    public int MaxPickLevel { get; set; }
    public double PickFactor { get; set; }
    public double NextPickFactor { get; set; }
    public long? PickCostGold { get; set; }

    public double Mined { get; set; }
    public double Stored { get; set; }
    public double Storage { get; set; }
    public long StoredGold { get; set; }
    public long GoldPerHour { get; set; }
}

public class GuildDepthMemberInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? HeroName { get; set; }
    public int Rank { get; set; }
    public HeroElement Element { get; set; }
    public int PickLevel { get; set; }
    public double Rate { get; set; }
    public double Mined { get; set; }
    public bool IsMe { get; set; }
}

public class GuildDepthStandingInfo
{
    public Guid GuildId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public int Members { get; set; }
    public int Place { get; set; }
    public int HonorBadges { get; set; }
    public bool IsMine { get; set; }
}

// Награда с места FromPlace (до следующей ступени)
public class GuildDepthRewardTier
{
    public int FromPlace { get; set; }
    public int HonorBadges { get; set; }
}

public class GuildDepthPendingInfo
{
    public DateTime WeekStart { get; set; }
    public int Place { get; set; }
    public int HonorBadges { get; set; }
}
