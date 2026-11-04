using IdleDwarfsGame.Server.Enums;

// Награда за первое прохождение уровня кампании (повторные победы ничего не дают).
// Золото и опыт: фиксированная часть по силе врагов уровня + случайная прибавка.
// Сундуки: после любой победы каждый тип из RandomChests бросается отдельно, босс сверху даёт один гарантированный.
// Что лежит в сундуке — таблицы дропа в БД (LootSeedData)
public static class CampaignRewards
{
    // Фиксированная часть: Base × (множитель статов врагов уровня)^RewardExponent — почти как растёт цена прокачки
    // дворфа (HeroUpgradeService, квадрат): чем сильнее враги, тем дороже прокачка, нужная, чтобы их победить.
    // За фазу ранга кампания окупает примерно половину прокачки отряда (EconomyBalanceTests), остальное — шахты и экспедиции.
    // Уровень 1 → 40 золота, 50 → ~230, 150 → ~3.4 тыс., 300 → ~21 тыс., 500 → ~130 тыс. (у босса ×BossMultiplier)
    public const double RewardExponent = 1.9;
    public const double BaseGold = 40;
    public const double BaseExperience = 20;

    // Случайная прибавка к золоту и опыту: от 0 до +50% фиксированной части
    public const double RandomBonus = 0.5;

    public const double BossMultiplier = 3;

    // Шанс случайного сундука каждого типа за победу
    // Дворфов нужно много: пятеро одного ранга сливаются в одного рангом выше, а 1-й ранг идёт в шахты
    public static readonly (ChestType Type, double Chance)[] RandomChests =
    [
        (ChestType.Gem, 0.15),
        (ChestType.Hero, 0.15),
        (ChestType.Equipment, 0.10)
    ];

    // Гарантированный сундук босса: тип выбирается по весам
    public static readonly (ChestType Type, int Weight)[] BossChestWeights =
    [
        (ChestType.Gem, 30),
        (ChestType.Hero, 40),
        (ChestType.Equipment, 30)
    ];

    // Уровень сундука по главам кампании: под ранг дворфов, которым её проходят (CampaignGenerator)
    private static readonly int[] ChestLevelFrom = [1, 51, 151, 251, 376];

    public static int ChestLevelFor(int stageId) =>
        Math.Clamp(ChestLevelFrom.Count(from => stageId >= from), 1, LootSeedData.MaxChestLevel);

    public static RewardEntity FixedFor(CampaignStage stage)
    {
        double enemyFactor = BalanceCalculator.Factor(stage.EnemyLevel);
        double scale = Math.Pow(enemyFactor, RewardExponent) * (stage.Type == StageType.Boss ? BossMultiplier : 1);
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
