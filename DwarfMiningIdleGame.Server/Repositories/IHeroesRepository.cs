using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DwarfMiningIdleGame.Server.Entities.GameEntities;

public interface IHeroesRepository
{
    Task<Hero?> GetByIdAsync(Guid id);
    Task<List<Hero>> GetByPlayerAsync(Guid playerId);
    Task CreateAsync(Hero hero);
    Task UpdateAsync(Hero hero);
    Task DeleteAsync(Guid id);
    /// <summary>
    /// Assigns a hero to a squad slot (or removes from squad if placeInSquad is null).
    /// Ensures slot uniqueness per player.
    /// </summary>
    Task AssignToSquadAsync(Guid heroId, int? placeInSquad);
}
