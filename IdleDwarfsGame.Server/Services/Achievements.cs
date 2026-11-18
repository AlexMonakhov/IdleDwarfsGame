using IdleDwarfsGame.Server.Enums;

// Что считает достижение. Activity — счётчик действия за всё время (PlayerLifetimeActivity, вид — AchievementDefinition.Kind),
// остальное — текущее состояние аккаунта, его читают при каждом запросе (AchievementStats)
public enum AchievementMetric
{
    Activity,
    CampaignStages,
    TowerBestFloor,
    HeroesOwned,
    Friends,
    LoginDays,
    LoginBestStreak
}

// Награда ступени. Золото и опыт — в «уровнях кампании» (DailyQuests.GoldFor), сундуки — уровня текущей главы.
// Frame — рамка аватара (фронт: models/types/Account.ts FRAME_EFFECTS), открывается, когда ступень забрана
public record AchievementReward(double GoldStages, int RainbowCoins, int ElderSeals, ChestType[] Chests, string? Frame = null);

// Достижение со ступенями: Targets — цели по возрастанию, награда ступени — Achievements.TierReward
// (у ступени FrameTier — ещё и рамка Frame)
public record AchievementDefinition(string Key, AchievementMetric Metric, long[] Targets, ActivityKind? Kind = null)
{
    public string? Frame { get; init; }
    public int FrameTier { get; init; } = -1;
}

// Значения метрик игрока на сейчас
public record AchievementStats(
    IReadOnlyDictionary<ActivityKind, long> Lifetime,
    int CampaignStages,
    int TowerBestFloor,
    int HeroesOwned,
    int Friends,
    int LoginDays,
    int LoginBestStreak)
{
    public long ValueOf(AchievementDefinition achievement) => achievement.Metric switch
    {
        AchievementMetric.Activity => achievement.Kind is { } kind ? Lifetime.GetValueOrDefault(kind) : 0,
        AchievementMetric.CampaignStages => CampaignStages,
        AchievementMetric.TowerBestFloor => TowerBestFloor,
        AchievementMetric.HeroesOwned => HeroesOwned,
        AchievementMetric.Friends => Friends,
        AchievementMetric.LoginDays => LoginDays,
        AchievementMetric.LoginBestStreak => LoginBestStreak,
        _ => 0
    };
}

// Достижения: постоянные цели со ступенями. Ступень забирается кнопкой, когда цель достигнута; ступени — по порядку.
// Награда растёт со ступенью и одинакова у всех достижений (TierReward), чтобы баланс правился в одном месте.
// Три рамки аватара открываются достижениями: снег — серия входов, угли — победы на арене, руны — кампания.
//
// Радужных монет за всё: ~3+5+10+15+25 на достижение с пятью ступенями — около 600 на все достижения
public static class Achievements
{
    private static readonly ChestType[] None = [];

    // Награда ступени по её номеру; ступеней дальше пятой нет
    public static readonly AchievementReward[] Tiers =
    [
        new(GoldStages: 3, RainbowCoins: 3, ElderSeals: 0, None),
        new(GoldStages: 5, RainbowCoins: 5, ElderSeals: 10, [ChestType.Gem]),
        new(GoldStages: 0, RainbowCoins: 10, ElderSeals: 15, [ChestType.Hero]),
        new(GoldStages: 0, RainbowCoins: 15, ElderSeals: 20, [ChestType.Hero, ChestType.Equipment]),
        new(GoldStages: 0, RainbowCoins: 25, ElderSeals: 30, [ChestType.Hero, ChestType.Hero, ChestType.Equipment])
    ];

    public static readonly AchievementDefinition[] All =
    [
        // Прогресс
        new("campaign_stages", AchievementMetric.CampaignStages, [10, 50, 100, 250, 500]) { Frame = "runes", FrameTier = 3 },
        new("campaign_wins", AchievementMetric.Activity, [25, 100, 500, 2000, 5000], ActivityKind.CampaignWin),
        new("tower_floor", AchievementMetric.TowerBestFloor, [10, 25, 50, 100]),
        new("heroes_owned", AchievementMetric.HeroesOwned, [30, 60, 100, 200]),
        new("hero_upgrades", AchievementMetric.Activity, [10, 100, 500, 2000], ActivityKind.HeroUpgrade),

        // Арена и гильдия
        new("pvp_wins", AchievementMetric.Activity, [5, 25, 100, 500], ActivityKind.PvpWin) { Frame = "embers", FrameTier = 2 },
        new("guild_boss", AchievementMetric.Activity, [10, 100, 500], ActivityKind.GuildBossAttack),
        new("friends", AchievementMetric.Friends, [1, 5, 10]),

        // Хозяйство
        new("mine_digs", AchievementMetric.Activity, [1000, 10000, 50000, 200000], ActivityKind.MineDig),
        new("mine_collects", AchievementMetric.Activity, [10, 100, 500, 2000], ActivityKind.MineCollect),
        new("expeditions", AchievementMetric.Activity, [5, 50, 250, 1000], ActivityKind.ExpeditionClaim),
        new("chests_opened", AchievementMetric.Activity, [10, 100, 500, 2000], ActivityKind.ChestOpen),
        new("forge_fuses", AchievementMetric.Activity, [5, 50, 250], ActivityKind.ForgeFuse),
        new("shop_buys", AchievementMetric.Activity, [5, 50, 200], ActivityKind.ShopBuy),

        // Верность
        new("login_days", AchievementMetric.LoginDays, [7, 30, 100, 365]),
        new("login_streak", AchievementMetric.LoginBestStreak, [7, 14, 28]) { Frame = "snow", FrameTier = 2 }
    ];

    public static AchievementDefinition? Find(string key) => All.FirstOrDefault(a => a.Key == key);

    public static AchievementReward RewardOf(AchievementDefinition achievement, int tier)
    {
        var reward = Tiers[Math.Min(tier, Tiers.Length - 1)];
        return tier == achievement.FrameTier ? reward with { Frame = achievement.Frame } : reward;
    }

    // Сколько ступеней достигнуто при таком значении метрики
    public static int ReachedTiers(AchievementDefinition achievement, long value) =>
        achievement.Targets.Count(target => value >= target);

    // Рамки, открытые забранными ступенями
    public static IEnumerable<string> FramesOf(IReadOnlyDictionary<string, int> claimedTiers) =>
        All.Where(a => a.Frame != null && claimedTiers.GetValueOrDefault(a.Key) > a.FrameTier).Select(a => a.Frame!);
}
