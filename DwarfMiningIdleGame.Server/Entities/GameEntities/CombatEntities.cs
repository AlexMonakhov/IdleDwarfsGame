public class Squad
{
    public string PlayerId { get; set; }
    public List<Character> Characters { get; set; } = new();
}

public class Character
{
    public string Name { get; set; } // Например: "Ice Dwarf Vanguard" или "Avian Support"
    public int Position { get; set; } // Например: 5 (саппорт)
}

public class EcsBattleResult
{
    public bool IsPlayerWinner { get; set; }
    public Dictionary<string, int> DamageDealt { get; set; } = new();
}

public class RewardEntity
{
    public int Gold { get; set; }
    public int Experience { get; set; }
}

public enum FightType
{
    Campaign,
    InfinityTower
}