using IdleDwarfsGame.Server.Enums;

public class RewardChestInfo
{
    public ChestType Type { get; set; }
    public int Level { get; set; }
    // Basic — сундук без стихии
    public HeroElement Element { get; set; }
}

public class RewardInfo
{
    public int Gold { get; set; }
    public int Experience { get; set; }
    public List<RewardChestInfo> Chests { get; set; } = new();
    // Предметы сразу в инвентарь (камни башни)
    public List<ItemInfo> Items { get; set; } = new();

    public static RewardInfo From(PlayerReward reward) => new()
    {
        Gold = reward.Gold,
        Experience = reward.Experience,
        Chests = reward.Chests
            .Select(c => new RewardChestInfo { Type = c.ChestType, Level = c.ChestLevel, Element = c.Element })
            .ToList(),
        Items = reward.Items
            .Select(i => new ItemInfo { ItemType = i.ItemType, SubType = i.SubType, Rarity = i.Rarity, Level = i.Level, Quantity = i.Quantity })
            .ToList()
    };
}

public class RewardClaimResult
{
    public RewardInfo Reward { get; set; } = new();
    // Золото и опыт аккаунта уже с наградой — для шапки интерфейса
    public PlayerResources Resources { get; set; } = new();
}
