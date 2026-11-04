public interface IMiningRepository
{
    Task<List<Mine>> GetMinesAsync(Guid playerId);
    Task<Mine?> GetAsync(Guid playerId, Guid mineId);
    /// <summary>
    /// Ids of player's heroes that work in mines right now.
    /// </summary>
    Task<HashSet<Guid>> GetHeroIdsAsync(Guid playerId);
    /// <summary>
    /// Player's gold, experience and crystals (common level 1 gems) — what mines cost and produce.
    /// </summary>
    Task<MiningWallet> GetWalletAsync(Guid playerId);
    /// <summary>
    /// In one transaction: adds the mine and applies the change to the owner's resources.
    /// Changed — the plot is already taken; nothing is saved unless the outcome is Saved.
    /// </summary>
    Task<MiningSaveOutcome> BuildAsync(Mine mine, MiningChange change);
    /// <summary>
    /// In one transaction: writes the mine (only if its Version is still expectedVersion) and applies
    /// the change to the owner's resources (only if there is enough to pay). On success mine.Version grows by one.
    /// </summary>
    Task<MiningSaveOutcome> SaveAsync(Mine mine, int expectedVersion, MiningChange change);
}
