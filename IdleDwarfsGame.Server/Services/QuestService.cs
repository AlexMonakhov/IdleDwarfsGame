using IdleDwarfsGame.Server.Enums;

// Причина отказа — фронт показывает по коду переведённый текст (quests.errors.<code>)
public enum QuestError
{
    QuestNotFound,
    NotCompleted,
    AlreadyClaimed,
    UnknownMilestone,
    MilestoneNotReached,
    NotEnoughRainbowCoins,
    // Доску изменили параллельным запросом или наступили новые сутки — фронт перечитает её
    Changed
}

public class QuestException(QuestError code, string message) : InvalidOperationException(message)
{
    public QuestError Code { get; } = code;
}

// Ежедневные задания (правила — DailyQuests). Доска собирается при первом заходе в сутки,
// прогресс берётся из счётчиков действий (ActivityTracker), награды выдаются сразу при получении
public class QuestService(
    IQuestRepository quests,
    IActivityRepository activity,
    IPlayerRepository players,
    Random random,
    TimeProvider clock)
{
    // Доску могли собрать параллельно (две вкладки) — тогда перечитываем и смотрим ещё раз
    private const int StartAttempts = 3;

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<QuestBoard> GetAsync(Guid playerId)
    {
        await EnsureTodayAsync(playerId);
        return await DescribeAsync(playerId);
    }

    public async Task<QuestClaimResult> ClaimAsync(Guid playerId, Guid questId)
    {
        var state = await EnsureTodayAsync(playerId);
        var quest = await quests.GetQuestAsync(playerId, questId);
        if (quest == null || quest.Day != state.Day)
            throw new QuestException(QuestError.QuestNotFound, "Quest not found");
        if (quest.ClaimedAt != null)
            throw new QuestException(QuestError.AlreadyClaimed, "The quest reward is already claimed");

        var counters = await activity.GetDayAsync(playerId, state.Day);
        if (ProgressOf(quest, counters) < quest.Target)
            throw new QuestException(QuestError.NotCompleted, "The quest is not completed yet");

        // Валюта идёт в кошелёк ивента, который шёл в сутки задания
        DateTime? eventStart = quest.EventCurrency > 0 ? WorldEvents.Of(quest.Day).StartsAt : null;
        if (!await quests.ClaimQuestAsync(quest, eventStart, Now))
            throw new QuestException(QuestError.AlreadyClaimed, "The quest reward is already claimed");

        return new QuestClaimResult
        {
            Board = await DescribeAsync(playerId),
            Reward = new RewardInfo { Gold = quest.Gold, Experience = quest.Experience },
            ElderSeals = quest.ElderSeals,
            EventCurrency = quest.EventCurrency
        };
    }

    public async Task<QuestClaimResult> ClaimMilestoneAsync(Guid playerId, int index)
    {
        if (index < 0 || index >= DailyQuests.Milestones.Length)
            throw new QuestException(QuestError.UnknownMilestone, $"Unknown milestone {index}");

        var now = Now;
        var state = await EnsureTodayAsync(playerId);
        if ((state.MilestonesClaimed & (1 << index)) != 0)
            throw new QuestException(QuestError.AlreadyClaimed, "The milestone reward is already claimed");

        var milestone = DailyQuests.Milestones[index];
        var board = await quests.GetQuestsAsync(playerId, state.Day);
        if (PointsOf(board) < milestone.Points)
            throw new QuestException(QuestError.MilestoneNotReached, $"Collect {milestone.Points} points first");

        var player = await players.GetProgressAsync(playerId);
        var reward = MilestoneReward(playerId, milestone, player, now);
        var activeEvent = WorldEvents.ActiveAt(now);
        int eventCurrency = activeEvent == null ? 0 : milestone.EventCurrency;

        switch (await quests.ClaimMilestoneAsync(state, index, reward, milestone.ElderSeals, eventCurrency, activeEvent?.StartsAt))
        {
            case QuestSaveOutcome.Changed:
                throw new QuestException(QuestError.Changed, "The quest board has just changed, try again");
        }

        return new QuestClaimResult
        {
            Board = await DescribeAsync(playerId),
            Reward = RewardInfo.From(reward),
            ElderSeals = milestone.ElderSeals,
            EventCurrency = eventCurrency
        };
    }

    // Замена задания на другое той же сложности: раз в сутки бесплатно, дальше за радужные монеты
    public async Task<QuestBoard> RerollAsync(Guid playerId, Guid questId)
    {
        var now = Now;
        var state = await EnsureTodayAsync(playerId);
        var board = await quests.GetQuestsAsync(playerId, state.Day);
        var quest = board.FirstOrDefault(q => q.Id == questId)
            ?? throw new QuestException(QuestError.QuestNotFound, "Quest not found");
        if (quest.ClaimedAt != null)
            throw new QuestException(QuestError.AlreadyClaimed, "The quest reward is already claimed");

        var player = await players.GetProgressAsync(playerId);
        // Новое задание — другое: не то же и не те, что уже на доске
        var taken = board.Select(q => q.Key).ToList();
        var template = DailyQuests.Pick(DailyQuests.DifficultyOf(quest.Slot), player, taken, random);
        var replacement = DailyQuests.Create(playerId, state.Day, quest.Slot, template, player, quest.EventCurrency > 0 || WorldEvents.ActiveAt(now) != null);

        bool free = !state.FreeRerollUsed;
        switch (await quests.RerollAsync(state, questId, replacement, free, free ? 0 : DailyQuests.RerollCost))
        {
            case QuestSaveOutcome.Changed:
                throw new QuestException(QuestError.Changed, "The quest board has just changed, try again");
            case QuestSaveOutcome.NotEnoughResources:
                throw new QuestException(QuestError.NotEnoughRainbowCoins, $"A reroll costs {DailyQuests.RerollCost} rainbow coins");
        }
        return await DescribeAsync(playerId);
    }

    /* ===== Доска ===== */

    // Доска без состояния или со вчерашними заданиями — собираем свежую
    private async Task<DailyQuestState> EnsureTodayAsync(Guid playerId)
    {
        for (int attempt = 0; attempt < StartAttempts; attempt++)
        {
            var now = Now;
            var today = DailyQuests.DayOf(now);
            var state = await quests.GetStateAsync(playerId);
            if (state != null && state.Day >= today)
                return state;

            var player = await players.GetProgressAsync(playerId);
            bool eventActive = WorldEvents.ActiveAt(now) != null;
            var board = Generate(playerId, today, player, eventActive);
            var next = new DailyQuestState { PlayerId = playerId, Day = today };
            if (await quests.StartDayAsync(next, state?.Version, board, today.AddDays(-DailyQuests.KeepActivityDays)) == QuestSaveOutcome.Saved)
                return await quests.GetStateAsync(playerId) ?? next;
        }
        throw new QuestException(QuestError.Changed, "The quest board has just changed, try again");
    }

    private List<PlayerQuest> Generate(Guid playerId, DateTime day, PlayerProgress player, bool eventActive)
    {
        var used = new List<string>();
        var board = new List<PlayerQuest>();
        for (int slot = 0; slot < DailyQuests.SlotCount; slot++)
        {
            var template = DailyQuests.Pick(DailyQuests.DifficultyOf(slot), player, used, random);
            used.Add(template.Key);
            board.Add(DailyQuests.Create(playerId, day, slot, template, player, eventActive));
        }
        return board;
    }

    private static PlayerReward MilestoneReward(Guid playerId, QuestMilestone milestone, PlayerProgress player, DateTime now)
    {
        var rewardId = Guid.NewGuid();
        int chestLevel = DailyQuests.ChestLevelFor(player);
        return new PlayerReward
        {
            Id = rewardId,
            PlayerId = playerId,
            Gold = DailyQuests.GoldFor(milestone.GoldStages, player),
            Experience = DailyQuests.ExperienceFor(milestone.GoldStages, player),
            Chests = milestone.Chests
                .Select(type => new PlayerRewardChest { Id = Guid.NewGuid(), RewardId = rewardId, ChestType = type, ChestLevel = chestLevel })
                .ToList(),
            CreatedAt = now
        };
    }

    private static int ProgressOf(PlayerQuest quest, IReadOnlyDictionary<ActivityKind, int> counters) =>
        Math.Min(quest.Target, counters.GetValueOrDefault(quest.Kind));

    // Очки шкалы — за забранные задания
    private static int PointsOf(IEnumerable<PlayerQuest> board) => board.Where(q => q.ClaimedAt != null).Sum(q => q.Points);

    /* ===== Ответ ===== */

    private async Task<QuestBoard> DescribeAsync(Guid playerId)
    {
        var now = Now;
        var today = DailyQuests.DayOf(now);
        var state = await quests.GetStateAsync(playerId) ?? new DailyQuestState { PlayerId = playerId, Day = today };
        var board = await quests.GetQuestsAsync(playerId, state.Day);
        var counters = await activity.GetDayAsync(playerId, state.Day);
        var player = await players.GetProgressAsync(playerId);
        var activeEvent = WorldEvents.ActiveAt(now);
        int points = PointsOf(board);

        var questInfos = board.Select(q =>
        {
            int progress = ProgressOf(q, counters);
            return new QuestInfo
            {
                Id = q.Id,
                Slot = q.Slot,
                Key = q.Key,
                Kind = q.Kind,
                Difficulty = DailyQuests.DifficultyOf(q.Slot),
                Target = q.Target,
                Progress = progress,
                Completed = progress >= q.Target,
                Claimed = q.ClaimedAt != null,
                Points = q.Points,
                ElderSeals = q.ElderSeals,
                EventCurrency = q.EventCurrency,
                Gold = q.Gold,
                Experience = q.Experience
            };
        }).ToList();

        var milestones = DailyQuests.Milestones.Select((m, index) => new QuestMilestoneInfo
        {
            Index = index,
            Points = m.Points,
            Reached = points >= m.Points,
            Claimed = (state.MilestonesClaimed & (1 << index)) != 0,
            Reward = RewardInfo.From(MilestoneReward(playerId, m, player, now)),
            ElderSeals = m.ElderSeals,
            EventCurrency = activeEvent == null ? 0 : m.EventCurrency
        }).ToList();

        return new QuestBoard
        {
            Day = state.Day,
            ResetAt = today.AddDays(1),
            ServerTime = now,
            Points = points,
            MaxPoints = DailyQuests.MaxPoints,
            FreeReroll = !state.FreeRerollUsed,
            RerollCost = DailyQuests.RerollCost,
            Claimable = questInfos.Count(q => q.Completed && !q.Claimed) + milestones.Count(m => m.Reached && !m.Claimed),
            Quests = questInfos,
            Milestones = milestones,
            Event = activeEvent == null ? null : new QuestEventInfo { Kind = activeEvent.Kind, EndsAt = activeEvent.EndsAt },
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }
}
