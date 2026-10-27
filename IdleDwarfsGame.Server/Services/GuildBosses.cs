using IdleDwarfsGame.Server.Enums;

// Боссы гильдии: по одному каждого уровня. Внешность и умения — шаблон врага башни (HeroSeedData),
// атака масштабируется уровнем, а HP задаётся отдельно и намного больше, чем у обычных боссов:
// одним отрядом за бой (CombatContext.MaxRounds раундов) его не убить, бьёт вся гильдия.
// Стартовый отряд наносит за бой ~730 урона лёгкому, ~480 среднему и ~400 сложному боссу,
// то есть стартовым отрядам нужно ~27, ~85 и ~250 ударов
public static class GuildBosses
{
    // Боссы обновляются каждые сутки в 00:00 UTC
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

    // Как часто один игрок может бить босса одного уровня
    public static readonly TimeSpan AttackCooldown = TimeSpan.FromHours(1);

    public static readonly GuildBossDefinition[] All =
    [
        new(GuildBossTier.Easy,
            TemplateHeroId: Guid.Parse("22222222-0000-4000-8000-000000000001"), // Orc Brute
            Background: "ice",
            Level: 2, MaxHp: 20_000,
            Gold: 400, Experience: 200,
            Chests: [(ChestType.Gem, 1)]),
        new(GuildBossTier.Medium,
            TemplateHeroId: Guid.Parse("22222222-0000-4000-8000-000000000004"), // Orc Warlord
            Background: "lava",
            Level: 5, MaxHp: 40_000,
            Gold: 1200, Experience: 600,
            Chests: [(ChestType.Gem, 2), (ChestType.Hero, 1)]),
        new(GuildBossTier.Hard,
            TemplateHeroId: Guid.Parse("22222222-0000-4000-8000-000000000005"), // Skeleton Mage
            Background: "ice",
            Level: 8, MaxHp: 100_000,
            Gold: 3000, Experience: 1500,
            Chests: [(ChestType.Gem, 3), (ChestType.Hero, 2)])
    ];

    public static GuildBossDefinition Get(GuildBossTier tier) => All.First(b => b.Tier == tier);

    // Сутки, в которые попадает момент времени (00:00 UTC)
    public static DateTime DayOf(DateTime utc) => new(utc.Ticks - utc.Ticks % Lifetime.Ticks, DateTimeKind.Utc);

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
    Guid TemplateHeroId,
    // Фон арены, на которой идёт бой (ключ как у Arena.Background: картинку по нему выбирает фронт)
    string Background,
    int Level,
    int MaxHp,
    int Gold,
    int Experience,
    (ChestType Type, int Level)[] Chests
);
