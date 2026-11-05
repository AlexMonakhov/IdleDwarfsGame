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

    // Несколько наград одной: золото и опыт складываются, одинаковые предметы — в одну стопку
    public static RewardInfo Sum(IEnumerable<PlayerReward> rewards)
    {
        var list = rewards.ToList();
        return new RewardInfo
        {
            Gold = list.Sum(r => r.Gold),
            Experience = list.Sum(r => r.Experience),
            Chests = list
                .SelectMany(r => r.Chests)
                .Select(c => new RewardChestInfo { Type = c.ChestType, Level = c.ChestLevel, Element = c.Element })
                .ToList(),
            Items = list
                .SelectMany(r => r.Items)
                .GroupBy(i => (i.ItemType, i.SubType, i.Rarity, i.Level))
                .OrderBy(g => g.Key.Rarity)
                .Select(g => new ItemInfo
                {
                    ItemType = g.Key.ItemType, SubType = g.Key.SubType, Rarity = g.Key.Rarity, Level = g.Key.Level,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .ToList()
        };
    }
}

public class RewardClaimResult
{
    public RewardInfo Reward { get; set; } = new();
    // Золото и опыт аккаунта уже с наградой — для шапки интерфейса
    public PlayerResources Resources { get; set; } = new();
}

public class RewardClaimAllResult : RewardClaimResult
{
    // Какие награды забраны: их карточки в списке гаснут без перезагрузки обзора
    public List<Guid> RewardIds { get; set; } = new();
}
