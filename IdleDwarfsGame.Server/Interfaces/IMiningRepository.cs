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
    /// In one transaction: writes the mine (only if its Version is still expectedVersion), applies
    /// the change to the owner's resources (only if there is enough to pay) and puts a dwarf in or takes one out.
    /// HeroTaken — the dwarf has just been put into another mine. On success mine.Version grows by one.
    /// </summary>
    Task<MiningSaveOutcome> SaveAsync(Mine mine, int expectedVersion, MiningChange change, MineDwarfChange? dwarf = null);
    /// <summary>
    /// Raises one pickaxe upgrade by a level, charges its cost and stores the pickaxe energy settled at the old levels —
    /// only if the level is still expectedLevel, the energy was not spent since it was read and there is enough to pay.
    /// </summary>
    Task<bool> UpgradePickaxeAsync(Guid playerId, PickaxeUpgrade upgrade, int expectedLevel, MineCost cost, DigEnergySpend settle);
}

// Дворф ставится в шахту на место Slot (Add) или уходит из неё (Remove)
public record MineDwarfChange(Guid HeroId, int Slot, bool Add)
{
    public static MineDwarfChange Put(Guid heroId, int slot) => new(heroId, slot, true);
    public static MineDwarfChange Take(Guid heroId) => new(heroId, 0, false);
}
