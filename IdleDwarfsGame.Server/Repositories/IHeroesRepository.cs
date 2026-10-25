using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdleDwarfsGame.Server.Entities.GameEntities;

public interface IHeroesRepository
{
    Task<Hero?> GetByIdAsync(Guid id);
    Task<List<Hero>> GetByPlayerAsync(Guid playerId);
    /// <summary>
    /// Heroes placed in the squad, ordered by slot. Falls back to the first heroes of the player when nobody is placed.
    /// </summary>
    Task<List<Hero>> GetSquadAsync(Guid playerId, int maxSize = 5);
    Task CreateAsync(Hero hero);
    Task UpdateAsync(Hero hero);
    Task DeleteAsync(Guid id);
    /// <summary>
    /// Assigns a hero to a squad slot (or removes from squad if placeInSquad is null).
    /// Ensures slot uniqueness per player.
    /// </summary>
    Task AssignToSquadAsync(Guid heroId, int? placeInSquad);
    /// <summary>
    /// Replaces the whole squad of a player: slots[i] goes to place i + 1, null leaves the slot empty,
    /// everyone else is removed from the squad. Throws InvalidOperationException on invalid input.
    /// </summary>
    Task<List<Hero>> SaveSquadAsync(Guid playerId, IReadOnlyList<Guid?> slots);
}
