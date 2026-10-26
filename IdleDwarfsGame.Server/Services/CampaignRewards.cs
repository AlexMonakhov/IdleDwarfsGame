using IdleDwarfsGame.Server.Enums;

// Награда за первое прохождение уровня кампании (повторные победы ничего не дают).
// Золото и опыт: фиксированная часть по номеру уровня + случайная прибавка.
// Сундуки: после любой победы каждый тип из RandomChests бросается отдельно, босс сверху даёт один гарантированный.
// Что лежит в сундуке — таблицы дропа в БД (LootSeedData)
public static class CampaignRewards
{
    // Фиксированная часть: Base × уровень^LevelExponent.
    // Уровень 1 → 50 золота и 25 опыта, 5 → 345 и 173, 10 → 792 и 396 (у босса ×BossMultiplier)
    public const double BaseGold = 50;
    public const double BaseExperience = 25;
    public const double LevelExponent = 1.2;

    // Случайная прибавка к золоту и опыту: от 0 до +50% фиксированной части
    public const double RandomBonus = 0.5;

    public const double BossMultiplier = 2;

    // Шанс случайного сундука каждого типа за победу
    public static readonly (ChestType Type, double Chance)[] RandomChests =
    [
        (ChestType.Gem, 0.15),
        (ChestType.Hero, 0.05)
    ];

    // Гарантированный сундук босса: тип выбирается по весам
    public static readonly (ChestType Type, int Weight)[] BossChestWeights =
    [
        (ChestType.Gem, 70),
        (ChestType.Hero, 30)
    ];

    // Уровень сундука растёт каждые 10 уровней кампании: 1–9 → 1, 10–19 → 2, дальше 3
    public static int ChestLevelFor(int stageId) => Math.Clamp(1 + stageId / 10, 1, LootSeedData.MaxChestLevel);

    public static RewardEntity FixedFor(CampaignStage stage)
    {
        double scale = Math.Pow(Math.Max(1, stage.Id), LevelExponent) * (stage.Type == StageType.Boss ? BossMultiplier : 1);
        return new RewardEntity
        {
            Gold = (int)Math.Round(BaseGold * scale, MidpointRounding.AwayFromZero),
            Experience = (int)Math.Round(BaseExperience * scale, MidpointRounding.AwayFromZero)
        };
    }

    public static PlayerReward Roll(Guid playerId, CampaignStage stage, Random random, DateTime now)
    {
        var fixedPart = FixedFor(stage);
        int chestLevel = ChestLevelFor(stage.Id);

        var chests = new List<ChestType>();
        if (stage.Type == StageType.Boss)
            chests.Add(PickBossChest(random));
        chests.AddRange(RandomChests.Where(c => random.NextDouble() < c.Chance).Select(c => c.Type));

        var rewardId = Guid.NewGuid();
        return new PlayerReward
        {
            Id = rewardId,
            PlayerId = playerId,
            Source = RewardSource.Campaign,
            SourceId = stage.Id,
            Gold = fixedPart.Gold + Bonus(fixedPart.Gold, random),
            Experience = fixedPart.Experience + Bonus(fixedPart.Experience, random),
            Chests = chests
                .Select(type => new PlayerRewardChest { Id = Guid.NewGuid(), RewardId = rewardId, ChestType = type, ChestLevel = chestLevel })
                .ToList(),
            CreatedAt = now
        };
    }

    private static int Bonus(int amount, Random random) =>
        (int)Math.Round(amount * RandomBonus * random.NextDouble(), MidpointRounding.AwayFromZero);

    private static ChestType PickBossChest(Random random)
    {
        int roll = random.Next(BossChestWeights.Sum(c => c.Weight));
        foreach (var (type, weight) in BossChestWeights)
        {
            if (roll < weight)
                return type;
            roll -= weight;
        }
        return BossChestWeights[^1].Type;
    }
}
