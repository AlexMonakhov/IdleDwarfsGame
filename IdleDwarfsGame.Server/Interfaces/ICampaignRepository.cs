public interface ICampaignRepository
{
    /// <summary>
    /// Ids of all stages in the order they are played.
    /// </summary>
    Task<List<int>> GetStageIdsAsync();
    /// <summary>
    /// Stages with arena and enemy templates loaded, ordered by Id.
    /// </summary>
    Task<List<CampaignStage>> GetStagesAsync(IReadOnlyCollection<int> stageIds);
    Task<CampaignStage?> GetStageAsync(int stageId);
    Task<HashSet<int>> GetClearedStageIdsAsync(Guid playerId);
    /// <summary>
    /// Saves the stage as cleared together with its first-clear reward (unclaimed).
    /// Returns false if it was cleared before — then the reward is not saved.
    /// </summary>
    Task<bool> MarkClearedAsync(Guid playerId, int stageId, PlayerReward? reward = null);
}
