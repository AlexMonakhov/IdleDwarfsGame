using IdleDwarfsGame.Server.Enums;

// Причина отказа — фронт показывает по коду переведённый текст (achievements.errors.<code>, loginCalendar.errors.<code>)
public enum AchievementError
{
    UnknownAchievement,
    NothingToClaim,
    AlreadyClaimed,
    // Изменили параллельным запросом (двойной клик, вторая вкладка) — фронт перечитает
    Changed
}

public class AchievementException(AchievementError code, string message) : InvalidOperationException(message)
{
    public AchievementError Code { get; } = code;
}

// Награды вне боёв (достижения, календарь входов): золото и опыт в «уровнях кампании», сундуки уровня текущей главы —
// так же, как пороги ежедневных заданий
public static class BonusRewards
{
    public static PlayerReward Build(Guid playerId, double goldStages, IEnumerable<ChestType> chests, PlayerProgress player, DateTime now)
    {
        var rewardId = Guid.NewGuid();
        int chestLevel = DailyQuests.ChestLevelFor(player);
        return new PlayerReward
        {
            Id = rewardId,
            PlayerId = playerId,
            Gold = DailyQuests.GoldFor(goldStages, player),
            Experience = DailyQuests.ExperienceFor(goldStages, player),
            Chests = chests
                .Select(type => new PlayerRewardChest { Id = Guid.NewGuid(), RewardId = rewardId, ChestType = type, ChestLevel = chestLevel })
                .ToList(),
            CreatedAt = now
        };
    }
}

// Достижения (правила — Achievements). Прогресс не хранится отдельно: счётчики действий за всё время пишет ActivityTracker,
// остальное читается из состояния аккаунта. Хранится только, сколько ступеней забрано
public class AchievementService(IAchievementRepository achievements, IPlayerRepository players, TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AchievementBoard> GetAsync(Guid playerId) => await DescribeAsync(playerId);

    // Забирает все достигнутые ступени достижения сразу
    public async Task<AchievementClaimResult> ClaimAsync(Guid playerId, string key)
    {
        var achievement = Achievements.Find(key)
            ?? throw new AchievementException(AchievementError.UnknownAchievement, $"Unknown achievement {key}");

        var stats = await achievements.GetStatsAsync(playerId);
        var claimed = await achievements.GetClaimedAsync(playerId);
        var player = await players.GetProgressAsync(playerId);

        var granted = await ClaimOneAsync(playerId, achievement, stats, claimed, player)
            ?? throw new AchievementException(AchievementError.NothingToClaim, "Nothing to claim yet");

        return new AchievementClaimResult { Board = await DescribeAsync(playerId), Granted = granted };
    }

    // Все достигнутые ступени всех достижений; каждое достижение — своей транзакцией
    public async Task<AchievementClaimResult> ClaimAllAsync(Guid playerId)
    {
        var stats = await achievements.GetStatsAsync(playerId);
        var claimed = await achievements.GetClaimedAsync(playerId);
        var player = await players.GetProgressAsync(playerId);

        var parts = new List<GrantedReward>();
        foreach (var achievement in Achievements.All)
        {
            if (await ClaimOneAsync(playerId, achievement, stats, claimed, player) is { } granted)
                parts.Add(granted);
        }
        if (parts.Count == 0)
            throw new AchievementException(AchievementError.NothingToClaim, "Nothing to claim yet");

        return new AchievementClaimResult { Board = await DescribeAsync(playerId), Granted = Merge(parts) };
    }

    // null — забирать нечего
    private async Task<GrantedReward?> ClaimOneAsync(Guid playerId, AchievementDefinition achievement, AchievementStats stats,
        IReadOnlyDictionary<string, int> claimed, PlayerProgress player)
    {
        int from = claimed.GetValueOrDefault(achievement.Key);
        int to = Achievements.ReachedTiers(achievement, stats.ValueOf(achievement));
        if (to <= from)
            return null;

        var tiers = Enumerable.Range(from, to - from).Select(tier => Achievements.RewardOf(achievement, tier)).ToList();
        var reward = BonusRewards.Build(playerId, tiers.Sum(t => t.GoldStages), tiers.SelectMany(t => t.Chests), player, Now);
        int rainbow = tiers.Sum(t => t.RainbowCoins);
        int seals = tiers.Sum(t => t.ElderSeals);

        if (!await achievements.ClaimAsync(playerId, achievement.Key, from, to, reward, rainbow, seals, Now))
            throw new AchievementException(AchievementError.Changed, "Achievements have just changed, try again");

        return new GrantedReward
        {
            Reward = RewardInfo.From(reward),
            RainbowCoins = rainbow,
            ElderSeals = seals,
            Frames = tiers.Where(t => t.Frame != null).Select(t => t.Frame!).ToList()
        };
    }

    private static GrantedReward Merge(List<GrantedReward> parts) => new()
    {
        Reward = new RewardInfo
        {
            Gold = parts.Sum(p => p.Reward.Gold),
            Experience = parts.Sum(p => p.Reward.Experience),
            Chests = parts.SelectMany(p => p.Reward.Chests).ToList()
        },
        RainbowCoins = parts.Sum(p => p.RainbowCoins),
        ElderSeals = parts.Sum(p => p.ElderSeals),
        Frames = parts.SelectMany(p => p.Frames).ToList()
    };

    /* ===== Ответ ===== */

    private async Task<AchievementBoard> DescribeAsync(Guid playerId)
    {
        var now = Now;
        var stats = await achievements.GetStatsAsync(playerId);
        var claimed = await achievements.GetClaimedAsync(playerId);
        var player = await players.GetProgressAsync(playerId);

        var list = Achievements.All.Select(a =>
        {
            long progress = stats.ValueOf(a);
            int claimedTiers = Math.Min(claimed.GetValueOrDefault(a.Key), a.Targets.Length);
            return new AchievementInfo
            {
                Key = a.Key,
                Metric = a.Metric,
                Progress = progress,
                ClaimedTiers = claimedTiers,
                ReachedTiers = Achievements.ReachedTiers(a, progress),
                Tiers = a.Targets.Select((target, tier) =>
                {
                    var reward = Achievements.RewardOf(a, tier);
                    return new AchievementTierInfo
                    {
                        Target = target,
                        Reached = progress >= target,
                        Claimed = tier < claimedTiers,
                        Reward = RewardInfo.From(BonusRewards.Build(playerId, reward.GoldStages, reward.Chests, player, now)),
                        RainbowCoins = reward.RainbowCoins,
                        ElderSeals = reward.ElderSeals,
                        Frame = reward.Frame
                    };
                }).ToList()
            };
        }).ToList();

        return new AchievementBoard
        {
            Achievements = list,
            Claimable = list.Sum(a => Math.Max(0, a.ReachedTiers - a.ClaimedTiers)),
            ClaimedTiers = list.Sum(a => a.ClaimedTiers),
            TotalTiers = list.Sum(a => a.Tiers.Count),
            UnlockedFrames = Achievements.FramesOf(claimed).ToList(),
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }
}

// Календарь входов (правила — LoginCalendar): раз в сутки забирается награда следующего дня цикла
public class LoginCalendarService(ILoginCalendarRepository calendars, IPlayerRepository players, TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<LoginCalendarInfo> GetAsync(Guid playerId) => await DescribeAsync(playerId);

    public async Task<LoginClaimResult> ClaimAsync(Guid playerId)
    {
        var now = Now;
        var today = DailyQuests.DayOf(now);
        var state = await calendars.GetAsync(playerId);
        var position = LoginCalendar.PositionOf(state, today);
        if (position.ClaimedToday)
            throw new AchievementException(AchievementError.AlreadyClaimed, "Today's reward is already claimed");

        var day = LoginCalendar.RewardOf(position.Day);
        var player = await players.GetProgressAsync(playerId);
        var reward = BonusRewards.Build(playerId, day.GoldStages, day.Chests, player, now);
        var next = LoginCalendar.AfterClaim(playerId, state, position, today);

        if (!await calendars.ClaimAsync(next, state?.Version, reward, day.RainbowCoins, day.ElderSeals))
            throw new AchievementException(AchievementError.AlreadyClaimed, "Today's reward is already claimed");

        return new LoginClaimResult
        {
            Calendar = await DescribeAsync(playerId),
            Granted = new GrantedReward { Reward = RewardInfo.From(reward), RainbowCoins = day.RainbowCoins, ElderSeals = day.ElderSeals }
        };
    }

    private async Task<LoginCalendarInfo> DescribeAsync(Guid playerId)
    {
        var now = Now;
        var today = DailyQuests.DayOf(now);
        var state = await calendars.GetAsync(playerId);
        var position = LoginCalendar.PositionOf(state, today);
        var player = await players.GetProgressAsync(playerId);

        var days = LoginCalendar.Days.Select((day, index) => new LoginDayInfo
        {
            Day = index + 1,
            Claimed = index < position.ClaimedDays,
            Today = index + 1 == position.Day,
            Big = day.Big,
            Reward = RewardInfo.From(BonusRewards.Build(playerId, day.GoldStages, day.Chests, player, now)),
            RainbowCoins = day.RainbowCoins,
            ElderSeals = day.ElderSeals
        }).ToList();

        return new LoginCalendarInfo
        {
            Cycle = position.Cycle,
            CycleLength = LoginCalendar.CycleLength,
            Day = position.Day,
            ClaimedToday = position.ClaimedToday,
            ClaimedDays = position.ClaimedDays,
            // Без сегодняшнего дня: после пропуска — 0
            Streak = position.ClaimedToday ? position.Streak : position.Streak - 1,
            BestStreak = state?.BestStreak ?? 0,
            TotalDays = state?.TotalDays ?? 0,
            Broken = position.Broken,
            MissedDaysAllowed = LoginCalendar.MissedDaysAllowed,
            ResetAt = today.AddDays(1),
            ServerTime = now,
            Days = days,
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }
}
