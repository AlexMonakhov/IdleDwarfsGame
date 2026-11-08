using IdleDwarfsGame.Server.Enums;

public interface IQuestRepository
{
    Task<DailyQuestState?> GetStateAsync(Guid playerId);
    Task<List<PlayerQuest>> GetQuestsAsync(Guid playerId, DateTime day);
    Task<PlayerQuest?> GetQuestAsync(Guid playerId, Guid questId);

    /// <summary>
    /// Starts a new day: saves <paramref name="state"/> (it must still have <paramref name="expectedVersion"/>,
    /// null — the row must not exist yet), replaces every quest of the player with <paramref name="quests"/>
    /// and drops activity counters older than <paramref name="keepActivityFrom"/>.
    /// </summary>
    Task<QuestSaveOutcome> StartDayAsync(DailyQuestState state, int? expectedVersion, List<PlayerQuest> quests, DateTime keepActivityFrom);

    /// <summary>
    /// Replaces an unclaimed quest with <paramref name="replacement"/> (same slot). Uses the free reroll of the day
    /// or charges <paramref name="rainbowCost"/> rainbow coins, all in one transaction.
    /// </summary>
    Task<QuestSaveOutcome> RerollAsync(DailyQuestState state, Guid questId, PlayerQuest replacement, bool useFreeReroll, long rainbowCost);

    /// <summary>
    /// Marks the quest claimed and grants its reward. Event currency goes to the wallet of the event
    /// that started at <paramref name="eventStart"/>.
    /// </summary>
    Task<bool> ClaimQuestAsync(PlayerQuest quest, DateTime? eventStart, DateTime now);

    /// <summary>
    /// Marks milestone <paramref name="index"/> of today's board claimed (the state must still have its version) and grants
    /// <paramref name="reward"/>, <paramref name="elderSeals"/> and <paramref name="eventCurrency"/>.
    /// </summary>
    Task<QuestSaveOutcome> ClaimMilestoneAsync(DailyQuestState state, int index, PlayerReward reward, int elderSeals,
        int eventCurrency, DateTime? eventStart);
}

public enum QuestSaveOutcome
{
    Saved,
    // Доску успели изменить параллельным запросом (двойной клик, вторая вкладка)
    Changed,
    NotEnoughResources
}
