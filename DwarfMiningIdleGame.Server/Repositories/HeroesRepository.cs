using DwarfMiningIdleGame.Server.Entities.GameEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HeroesRepository : IHeroesRepository
{
    private readonly GameDbContext _context;

    public HeroesRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task<Hero?> GetByIdAsync(Guid id)
    {
        return await _context.Heroes.FindAsync(id);
    }

    public async Task<List<Hero>> GetByPlayerAsync(Guid playerId)
    {
        return await _context.Heroes
            .Where(h => h.PlayerId == playerId)
            .ToListAsync();
    }

    public async Task CreateAsync(Hero hero)
    {
        // Ensure Id is set
        if (hero.Id == Guid.Empty)
            hero.Id = Guid.NewGuid();

        await _context.Heroes.AddAsync(hero);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Hero hero)
    {
        _context.Heroes.Update(hero);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var hero = await _context.Heroes.FindAsync(id);
        if (hero != null)
        {
            _context.Heroes.Remove(hero);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AssignToSquadAsync(Guid heroId, int? placeInSquad)
    {
        var hero = await _context.Heroes.FindAsync(heroId);
        if (hero == null)
            throw new InvalidOperationException($"Hero with id {heroId} not found");

        // If removing from squad, just null the place
        if (placeInSquad == null)
        {
            hero.PlaceInSquad = null;
            _context.Heroes.Update(hero);
            await _context.SaveChangesAsync();
            return;
        }

        // Ensure no other hero of the same player occupies that slot
        var existing = await _context.Heroes
            .FirstOrDefaultAsync(h => h.PlayerId == hero.PlayerId && h.PlaceInSquad == placeInSquad && h.Id != heroId);

        if (existing != null)
        {
            // Remove existing hero from slot
            existing.PlaceInSquad = null;
            _context.Heroes.Update(existing);
        }

        hero.PlaceInSquad = placeInSquad;
        _context.Heroes.Update(hero);

        await _context.SaveChangesAsync();
    }
}
