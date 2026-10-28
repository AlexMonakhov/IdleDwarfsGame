// Арена PvP (PvpArena): фон и правило нарастающего урона
public class PvpArenaInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    // Каждые IntervalRounds полных раундов весь урон растёт ещё на DamageBonusPercent
    public int IntervalRounds { get; set; }
    public int DamageBonusPercent { get; set; }
    public int MaxRounds { get; set; }

    public static PvpArenaInfo Current => new()
    {
        Name = PvpArena.Name,
        Description = PvpArena.Description,
        Background = PvpArena.Background,
        IntervalRounds = PvpArena.IntervalRounds,
        DamageBonusPercent = PvpArena.BonusPercent,
        MaxRounds = PvpArena.FightRounds
    };
}

public class PvpOpponentInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Power { get; set; }
    // Насколько соперник сильнее (+) или слабее (−) нас, в процентах от нашей мощи
    public double PowerDiffPercent { get; set; }
    // Отряд соперника как участники боя (команда 2)
    public List<CombatUnitInfo> Squad { get; set; } = new();
    // Прогноз боя на арене PvP
    public BattleForecast Forecast { get; set; } = new();
    // Сколько очков сезона даст победа и снимет поражение
    public int WinPoints { get; set; }
    public int LossPoints { get; set; }
}

public class PvpLeaderboardEntryInfo
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public bool IsMe { get; set; }
}

public class PvpSeasonInfo
{
    public PvpSeasonType Type { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int Points { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    // null — в этом сезоне игрок ещё не дрался
    public int? Rank { get; set; }
    public List<PvpLeaderboardEntryInfo> Top { get; set; } = new();
}

// Бой из истории игрока — с его стороны: нападал он или защищался
public class PvpBattleInfo
{
    public Guid Id { get; set; }
    public DateTime FoughtAt { get; set; }
    public bool IsAttack { get; set; }
    public Guid OpponentUserId { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public bool Won { get; set; }
    // Изменение своих очков: у защитника очки не меняются (0)
    public int Points { get; set; }
    public int MyPower { get; set; }
    public int OpponentPower { get; set; }
}

public class PvpOverview
{
    public DateTime ServerTime { get; set; }
    // Когда обнулится список побеждённых (и дневной сезон): 00:00 UTC
    public DateTime DayResetAt { get; set; }
    public int MyPower { get; set; }
    public List<CombatUnitInfo> Squad { get; set; } = new();
    // Окно мощи, в котором подобраны соперники: 10%, а если игроков мало — шире
    public int PowerSpreadPercent { get; set; }
    public int OpponentCount { get; set; }
    public List<PvpOpponentInfo> Opponents { get; set; } = new();
    public int BeatenToday { get; set; }
    public List<PvpSeasonInfo> Seasons { get; set; } = new();
    public List<PvpBattleInfo> History { get; set; } = new();
    public PvpArenaInfo Arena { get; set; } = new();
}

public class PvpSeasonPointsInfo
{
    public PvpSeasonType Type { get; set; }
    public int Points { get; set; }
}

public class PvpFightResult
{
    public Guid OpponentUserId { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public bool Won { get; set; }
    // Изменение очков во всех сезонах
    public int Points { get; set; }
    // Очки после боя в каждом сезоне
    public List<PvpSeasonPointsInfo> Seasons { get; set; } = new();
    public PvpArenaInfo Arena { get; set; } = new();
    public CombatResult Combat { get; set; } = new();
}
