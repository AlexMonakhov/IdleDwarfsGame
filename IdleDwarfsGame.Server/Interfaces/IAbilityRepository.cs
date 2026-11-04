public interface IAbilityRepository
{
    Task<List<PlayerAbility>> GetByPlayerAsync(Guid playerId);
    Task<PlayerAbility?> GetAsync(Guid playerId, Guid abilityId);
    /// <summary>
    /// In one transaction: removes the hero (its items go back to the inventory) and adds the extracted ability.
    /// Returns false and changes nothing if the hero is gone, became busy or was the player's last one.
    /// </summary>
    Task<bool> ExtractAsync(Hero hero, PlayerAbility ability);
    /// <summary>
    /// In one transaction: removes the ability and saves the changes made to the tracked hero.
    /// Returns false and changes nothing if the ability was used meanwhile.
    /// </summary>
    Task<bool> InsertAsync(PlayerAbility ability, Hero hero);
    /// <summary>
    /// In one transaction: removes both abilities and adds the result. Returns false if one of them was used meanwhile.
    /// </summary>
    Task<bool> CombineAsync(PlayerAbility first, PlayerAbility second, PlayerAbility result);
    /// <summary>
    /// In one transaction: removes the heroes (their items go back to the inventory) and adds the result.
    /// Returns false and changes nothing if one of them is gone or became busy.
    /// </summary>
    // refund — золото и опыт, которые вернутся игроку за прокачку слитых дворфов
    Task<bool> FuseAsync(Guid playerId, IReadOnlyList<Guid> heroIds, Hero result, HeroUpgradeCost refund);
}
