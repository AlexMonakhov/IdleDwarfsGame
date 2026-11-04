using IdleDwarfsGame.Server.Enums;

// Боссы гильдии: по одному каждого уровня. Внешность и умения — шаблон из GuildBossSeedData,
// атака масштабируется уровнем, а HP задаётся отдельно и намного больше, чем у обычных боссов:
// одним отрядом за бой (FightRounds раундов) его не убить, бьёт вся гильдия.
// У каждого уровня несколько вариантов «босс + арена его стихии», они сменяют друг друга по суткам.
// Бой идёт на арене варианта — с её эффектом.
// Запас HP подобран симуляцией (BalanceSimulation.Report): лёгкого босса отряд 1-го ранга 10-го уровня
// снимает за ~30 попыток, среднего отряд 2-го ранга 20-го уровня — за ~75, сложного отряд 3-го ранга 25-го уровня — за ~260
public static class GuildBosses
{
    // Сколько раундов длится один бой с боссом. Обычные бои идут до CombatContext.DefaultMaxRounds,
    // но урон по боссу за попытку рассчитан на этот лимит
    public const int FightRounds = 15;

    // Боссы обновляются каждые сутки в 00:00 UTC
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

    // Как часто один игрок может бить босса одного уровня
    public static readonly TimeSpan AttackCooldown = TimeSpan.FromHours(1);

    public static readonly GuildBossDefinition[] All =
    [
        new(GuildBossTier.Easy,
            Variants:
            [
                new(GuildBossSeedData.MossbackTroll, CampaignSeedData.MarshArenaId),
                new(GuildBossSeedData.RimehornYeti, CampaignSeedData.IceArenaId)
            ],
            Level: 10, MaxHp: 300_000,
            Gold: 2_000, Experience: 1_000,
            Chests: [(ChestType.Gem, 1), (ChestType.Hero, 1)]),
        new(GuildBossTier.Medium,
            Variants:
            [
                new(GuildBossSeedData.AshmawWarlord, CampaignSeedData.LavaArenaId),
                new(GuildBossSeedData.GrukkStormcaller, CampaignSeedData.StormArenaId)
            ],
            Level: 20, MaxHp: 1_800_000,
            Gold: 15_000, Experience: 7_500,
            Chests: [(ChestType.Gem, 2), (ChestType.Hero, 2)]),
        new(GuildBossTier.Hard,
            Variants:
            [
                new(GuildBossSeedData.NyxaraHollowQueen, CampaignSeedData.CryptArenaId),
                new(GuildBossSeedData.SeraphelShatteredLight, CampaignSeedData.SanctumArenaId)
            ],
            Level: 35, MaxHp: 7_500_000,
            Gold: 100_000, Experience: 50_000,
            Chests: [(ChestType.Gem, 3), (ChestType.Hero, 3), (ChestType.Hero, 3)])
    ];

    public static GuildBossDefinition Get(GuildBossTier tier) => All.First(b => b.Tier == tier);

    // Сутки, в которые попадает момент времени (00:00 UTC)
    public static DateTime DayOf(DateTime utc) => new(utc.Ticks - utc.Ticks % Lifetime.Ticks, DateTimeKind.Utc);

    // Арены лежат в сидах вместе с кампанией; в БД — те же строки
    public static Arena ArenaOf(GuildBossVariant variant) => CampaignSeedData.Arenas.First(a => a.Id == variant.ArenaId);

    public static PlayerReward RewardFor(GuildBossTier tier, Guid playerId, DateTime now)
    {
        var definition = Get(tier);
        var rewardId = Guid.NewGuid();
        return new PlayerReward
        {
            Id = rewardId,
            PlayerId = playerId,
            Source = RewardSource.GuildBoss,
            SourceId = (int)tier,
            Gold = definition.Gold,
            Experience = definition.Experience,
            Chests = definition.Chests
                .Select(c => new PlayerRewardChest { Id = Guid.NewGuid(), RewardId = rewardId, ChestType = c.Type, ChestLevel = c.Level })
                .ToList(),
            CreatedAt = now
        };
    }
}

public record GuildBossDefinition(
    GuildBossTier Tier,
    GuildBossVariant[] Variants,
    int Level,
    int MaxHp,
    int Gold,
    int Experience,
    (ChestType Type, int Level)[] Chests
)
{
    // Вариант на сутки day: варианты идут по кругу, каждые сутки — следующий
    public GuildBossVariant VariantOn(DateTime day) =>
        Variants[(int)(GuildBosses.DayOf(day).Ticks / GuildBosses.Lifetime.Ticks % Variants.Length)];
}

// Кто выходит на бой и где: шаблон из GuildBossSeedData и арена (CampaignSeedData.Arenas)
public record GuildBossVariant(Guid TemplateHeroId, int ArenaId);
