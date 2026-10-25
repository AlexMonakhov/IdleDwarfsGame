public interface IExpeditionRepository
{
    /// <summary>
    /// Expeditions of the board that starts at boardStartsAt plus the ones still in progress from older boards.
    /// </summary>
    Task<List<Expedition>> GetBoardAsync(Guid playerId, DateTime boardStartsAt);
    /// <summary>
    /// Removes not started offers of older boards and adds the new board. Returns false if the board
    /// was already created by a parallel request.
    /// </summary>
    Task<bool> ReplaceBoardAsync(Guid playerId, DateTime boardStartsAt, IReadOnlyList<Expedition> offers);
    Task<Expedition?> GetAsync(Guid playerId, Guid expeditionId);
    /// <summary>
    /// Ids of player's heroes that are on an expedition right now (not claimed yet).
    /// </summary>
    Task<HashSet<Guid>> GetBusyHeroIdsAsync(Guid playerId);
    /// <summary>
    /// Moves an available offer into progress with the given heroes. Throws InvalidOperationException
    /// if the offer was taken or one of the heroes went on another expedition meanwhile.
    /// </summary>
    Task StartAsync(Expedition expedition);
    /// <summary>
    /// Marks a finished expedition as claimed, frees its heroes and credits the reward to the player.
    /// Returns false if the expedition is not finished or was claimed before.
    /// </summary>
    Task<bool> ClaimAsync(Expedition expedition, DateTime now);
}
