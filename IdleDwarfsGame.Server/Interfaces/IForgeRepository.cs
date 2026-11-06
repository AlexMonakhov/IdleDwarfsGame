public interface IForgeRepository
{
    /// <summary>
    /// Items in the player's inventory (not worn by any hero).
    /// </summary>
    Task<List<PlayerEquipment>> GetFreeEquipmentAsync(Guid playerId);

    /// <summary>
    /// Level-1 gem stacks of the player.
    /// </summary>
    Task<List<ShopGemStack>> GetGemsAsync(Guid playerId);

    /// <summary>
    /// Removes the fused items and adds the forged one in one transaction.
    /// False — one of the items is gone or has just been put on a hero.
    /// </summary>
    Task<bool> FuseItemsAsync(Guid playerId, IReadOnlyList<Guid> equipmentIds, PlayerEquipment result);

    /// <summary>
    /// Takes <paramref name="cost"/> gems of <paramref name="from"/> rarity and adds <paramref name="created"/> gems of
    /// <paramref name="to"/> rarity of the same kind. False — not enough gems.
    /// </summary>
    Task<bool> FuseGemsAsync(Guid playerId, GemType gem, Rarity from, int cost, Rarity to, int created);
}
