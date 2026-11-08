using IdleDwarfsGame.Server.Enums;

public enum QuestDifficulty { Easy, Medium, Hard }

// Разделы игры, которые нужны заданиям. Задание выпадает, только когда его раздел открыт:
// пока в игре всё открыто сразу, раздел «открывается» по числу пройденных уровней кампании (MinClearedStages),
// гильдия — членством. Те же пороги потом возьмёт обучение
public enum GameFeature { Campaign, Upgrade, Inventory, Mining, Expeditions, Forge, Tower, Market, Pvp, Guild }

public record QuestTemplate(string Key, ActivityKind Kind, int Target, QuestDifficulty Difficulty, GameFeature Feature);

// Порог шкалы очков: сколько очков нужно и что за него дают. Золото и опыт — в «уровнях кампании»,
// как у заданий (DailyQuests.GoldFor); сундуки — уровня текущей главы кампании
public record QuestMilestone(int Points, int ElderSeals, int EventCurrency, double GoldStages, ChestType[] Chests);

// Что известно об игроке при сборке доски: от этого зависят доступные задания и размер наград
public record PlayerProgress(int ClearedStages, CampaignStage? CurrentStage, bool InGuild);

// Ежедневные задания. Каждые сутки (00:00 UTC) игрок получает SlotCount заданий: места 0–1 лёгкие, 2–3 средние, 4 — сложное.
// Прогресс — счётчик действия за сегодня (PlayerDailyActivity), поэтому сделанное до захода в задания тоже засчитано.
// Задание даёт очки, печати старейшин и немного золота с опытом; во время ивента — ещё и его валюту (WorldEvents).
// Очки всех пяти заданий — ровно 100: шкала из пяти порогов (Milestones) заполняется, если сделать всё.
//
// Доход печатей: 20 за задания + 30 на шкале = 50 в сутки, ~350 в неделю (цены — ElderShop)
public static class DailyQuests
{
    public const int SlotCount = 5;

    // Одна замена задания в сутки бесплатно, дальше — за радужные монеты
    public const int RerollCost = 5;

    // Сколько суток хранить счётчики действий (для отладки и будущих недельных заданий)
    public const int KeepActivityDays = 7;

    public static DateTime DayOf(DateTime utc) => utc.Date;

    public static QuestDifficulty DifficultyOf(int slot) => slot switch
    {
        < 2 => QuestDifficulty.Easy,
        < 4 => QuestDifficulty.Medium,
        _ => QuestDifficulty.Hard
    };

    public static int PointsFor(QuestDifficulty difficulty) => difficulty switch
    {
        QuestDifficulty.Easy => 15,
        QuestDifficulty.Medium => 20,
        _ => 30
    };

    public static int ElderSealsFor(QuestDifficulty difficulty) => difficulty switch
    {
        QuestDifficulty.Easy => 3,
        QuestDifficulty.Medium => 4,
        _ => 6
    };

    // Ивентовая валюта за задание, пока идёт ивент: ~50 в сутки с заданий и ещё 50 на шкале.
    // Основной доход ивента — его собственная активность, задания лишь не дают отстать тем, кто заходит ненадолго
    public static int EventCurrencyFor(QuestDifficulty difficulty) => difficulty switch
    {
        QuestDifficulty.Easy => 8,
        QuestDifficulty.Medium => 10,
        _ => 14
    };

    // Золото и опыт задания — столько «уровней кампании», сколько стоит первое прохождение текущего уровня
    // (CampaignRewards.FixedFor): награда растёт вместе с игроком и не ломает экономику ни в начале, ни в конце
    public static double GoldStagesFor(QuestDifficulty difficulty) => difficulty switch
    {
        QuestDifficulty.Easy => 1.5,
        QuestDifficulty.Medium => 2,
        _ => 3
    };

    public static readonly QuestMilestone[] Milestones =
    [
        new(20, ElderSeals: 0, EventCurrency: 0, GoldStages: 2, Chests: []),
        new(40, ElderSeals: 5, EventCurrency: 10, GoldStages: 0, Chests: []),
        new(60, ElderSeals: 0, EventCurrency: 0, GoldStages: 0, Chests: [ChestType.Gem]),
        new(80, ElderSeals: 10, EventCurrency: 15, GoldStages: 0, Chests: []),
        new(100, ElderSeals: 15, EventCurrency: 25, GoldStages: 0, Chests: [ChestType.Hero])
    ];

    public static int MaxPoints => Milestones[^1].Points;

    // С какого числа пройденных уровней кампании раздел попадает в задания
    public static int MinClearedStages(GameFeature feature) => feature switch
    {
        GameFeature.Campaign or GameFeature.Upgrade => 0,
        GameFeature.Inventory => 1,
        GameFeature.Mining => 3,
        GameFeature.Expeditions => 5,
        GameFeature.Forge => 8,
        GameFeature.Tower => 12,
        GameFeature.Market => 15,
        GameFeature.Pvp => 20,
        GameFeature.Guild => 25,
        _ => 0
    };

    // Цели подобраны так, чтобы лёгкое задание делалось за пару минут, а все пять — за 10–15 минут игры за сутки
    public static readonly QuestTemplate[] Templates =
    [
        // Лёгкие
        new("campaign_fight_3", ActivityKind.CampaignFight, 3, QuestDifficulty.Easy, GameFeature.Campaign),
        new("hero_upgrade_2", ActivityKind.HeroUpgrade, 2, QuestDifficulty.Easy, GameFeature.Upgrade),
        new("mine_dig_100", ActivityKind.MineDig, 100, QuestDifficulty.Easy, GameFeature.Mining),
        new("mine_collect_2", ActivityKind.MineCollect, 2, QuestDifficulty.Easy, GameFeature.Mining),
        new("expedition_start_1", ActivityKind.ExpeditionStart, 1, QuestDifficulty.Easy, GameFeature.Expeditions),

        // Средние
        new("campaign_win_5", ActivityKind.CampaignWin, 5, QuestDifficulty.Medium, GameFeature.Campaign),
        new("hero_upgrade_5", ActivityKind.HeroUpgrade, 5, QuestDifficulty.Medium, GameFeature.Upgrade),
        new("chest_open_3", ActivityKind.ChestOpen, 3, QuestDifficulty.Medium, GameFeature.Inventory),
        new("mine_dig_300", ActivityKind.MineDig, 300, QuestDifficulty.Medium, GameFeature.Mining),
        new("expedition_claim_2", ActivityKind.ExpeditionClaim, 2, QuestDifficulty.Medium, GameFeature.Expeditions),
        new("tower_fight_2", ActivityKind.TowerFight, 2, QuestDifficulty.Medium, GameFeature.Tower),
        new("shop_buy_1", ActivityKind.ShopBuy, 1, QuestDifficulty.Medium, GameFeature.Market),
        new("pvp_fight_3", ActivityKind.PvpFight, 3, QuestDifficulty.Medium, GameFeature.Pvp),
        new("guild_boss_2", ActivityKind.GuildBossAttack, 2, QuestDifficulty.Medium, GameFeature.Guild),

        // Сложные
        new("campaign_win_10", ActivityKind.CampaignWin, 10, QuestDifficulty.Hard, GameFeature.Campaign),
        new("mine_collect_5", ActivityKind.MineCollect, 5, QuestDifficulty.Hard, GameFeature.Mining),
        new("forge_fuse_1", ActivityKind.ForgeFuse, 1, QuestDifficulty.Hard, GameFeature.Forge),
        new("tower_fight_4", ActivityKind.TowerFight, 4, QuestDifficulty.Hard, GameFeature.Tower),
        new("pvp_win_3", ActivityKind.PvpWin, 3, QuestDifficulty.Hard, GameFeature.Pvp),
        new("guild_boss_4", ActivityKind.GuildBossAttack, 4, QuestDifficulty.Hard, GameFeature.Guild)
    ];

    public static QuestTemplate? Find(string key) => Templates.FirstOrDefault(t => t.Key == key);

    public static bool IsOpen(GameFeature feature, PlayerProgress player) =>
        player.ClearedStages >= MinClearedStages(feature) && (feature != GameFeature.Guild || player.InGuild);

    // Случайное задание нужной сложности из открытых разделов. На одной доске действие по возможности не повторяется:
    // два задания на одно и то же выполнялись бы одним делом. Новичку открыто мало разделов — тогда хотя бы
    // не тот же шаблон. taken — шаблоны, уже занятые на доске
    public static QuestTemplate Pick(QuestDifficulty difficulty, PlayerProgress player, IReadOnlyCollection<string> taken, Random random)
    {
        var takenTemplates = taken.Select(Find).OfType<QuestTemplate>().ToList();
        var open = Templates.Where(t => t.Difficulty == difficulty && IsOpen(t.Feature, player)).ToList();
        var freshKind = open.Where(t => takenTemplates.All(x => x.Kind != t.Kind)).ToList();
        var freshKey = open.Where(t => !taken.Contains(t.Key)).ToList();
        // Кампания открыта всегда, так что хотя бы одно задание каждой сложности найдётся
        var pool = freshKind.Count > 0 ? freshKind : freshKey.Count > 0 ? freshKey : open;
        return pool[random.Next(pool.Count)];
    }

    public static int GoldFor(double stages, PlayerProgress player) =>
        player.CurrentStage == null ? 0 : (int)Math.Round(CampaignRewards.FixedFor(player.CurrentStage).Gold * stages);

    public static int ExperienceFor(double stages, PlayerProgress player) =>
        player.CurrentStage == null ? 0 : (int)Math.Round(CampaignRewards.FixedFor(player.CurrentStage).Experience * stages);

    // Сундуки шкалы — уровня главы, в которой сейчас игрок
    public static int ChestLevelFor(PlayerProgress player) => CampaignRewards.ChestLevelFor(Math.Max(1, player.CurrentStage?.Id ?? 1));

    public static PlayerQuest Create(Guid playerId, DateTime day, int slot, QuestTemplate template, PlayerProgress player, bool eventActive)
    {
        var difficulty = DifficultyOf(slot);
        double stages = GoldStagesFor(difficulty);
        return new PlayerQuest
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Day = day,
            Slot = slot,
            Key = template.Key,
            Kind = template.Kind,
            Target = template.Target,
            Points = PointsFor(difficulty),
            ElderSeals = ElderSealsFor(difficulty),
            EventCurrency = eventActive ? EventCurrencyFor(difficulty) : 0,
            Gold = GoldFor(stages, player),
            Experience = ExperienceFor(stages, player)
        };
    }
}
