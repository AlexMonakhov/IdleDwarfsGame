using IdleDwarfsGame.Server.Enums;

public enum RewardSource { Campaign, GuildBoss, Tower }

// Награда, выданная на сервере, но ещё не забранная игроком.
// Создаётся вместе с результатом боя; золото, опыт, сундуки и предметы попадают на аккаунт только при ClaimedAt
public class PlayerReward
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }

    public RewardSource Source { get; set; }
    // Откуда награда: для кампании — номер уровня, для босса гильдии — GuildBossTier, для башни — этаж
    // (у пропуска этажей — последний пропущенный)
    public int SourceId { get; set; }

    public int Gold { get; set; }
    public int Experience { get; set; }
    public List<PlayerRewardChest> Chests { get; set; } = new();
    public List<PlayerRewardItem> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    // null — награду ещё не забрали
    public DateTime? ClaimedAt { get; set; }
}

// Сундук в награде. В инвентарь (PlayerChests) он попадает закрытым при получении награды
public class PlayerRewardChest
{
    public Guid Id { get; set; }
    public Guid RewardId { get; set; }

    public ChestType ChestType { get; set; }
    public int ChestLevel { get; set; }
    // Basic — сундук без стихии
    public HeroElement Element { get; set; } = HeroElement.Basic;
}

// Стопка предметов в награде (пока — гемы башни). При получении прибавляется к стопке в PlayerItems
public class PlayerRewardItem
{
    public Guid Id { get; set; }
    public Guid RewardId { get; set; }

    public ItemType ItemType { get; set; }
    public string SubType { get; set; } = string.Empty;
    public Rarity Rarity { get; set; }
    public int Level { get; set; } = 1;
    public int Quantity { get; set; }
}
