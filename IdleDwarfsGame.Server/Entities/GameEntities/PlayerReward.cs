using IdleDwarfsGame.Server.Enums;

public enum RewardSource { Campaign }

// Награда, выданная на сервере, но ещё не забранная игроком.
// Создаётся вместе с результатом боя; золото, опыт и сундуки попадают на аккаунт только при ClaimedAt
public class PlayerReward
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }

    public RewardSource Source { get; set; }
    // Откуда награда: для кампании — номер уровня
    public int SourceId { get; set; }

    public int Gold { get; set; }
    public int Experience { get; set; }
    public List<PlayerRewardChest> Chests { get; set; } = new();

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
}
