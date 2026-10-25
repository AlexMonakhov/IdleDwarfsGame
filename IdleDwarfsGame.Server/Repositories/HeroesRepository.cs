using IdleDwarfsGame.Server.Entities.GameEntities;
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

    public async Task<List<Hero>> GetSquadAsync(Guid playerId, int maxSize = 5)
    {
        var heroes = await _context.Heroes
            .Where(h => h.PlayerId == playerId)
            .ToListAsync();

        var squad = heroes
            .Where(h => h.PlaceInSquad != null)
            .OrderBy(h => h.PlaceInSquad)
            .Take(maxSize)
            .ToList();
        if (squad.Count > 0)
            return squad;

        // Герои в экспедиции в запасной отряд не попадают (в настоящий отряд их не пускает SaveSquadAsync)
        var busy = await BusyHeroIds(heroes.Select(h => h.Id).ToList()).ToListAsync();
        return heroes.Where(h => !busy.Contains(h.Id)).OrderBy(h => h.Name).Take(maxSize).ToList();
    }

    private IQueryable<Guid> BusyHeroIds(List<Guid> heroIds) => _context.ExpeditionHeroes
        .Where(h => heroIds.Contains(h.HeroId))
        .Select(h => h.HeroId);

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

        if (await BusyHeroIds([heroId]).AnyAsync())
            throw new InvalidOperationException($"Hero {heroId} is on an expedition");

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

    public async Task<List<Hero>> SaveSquadAsync(Guid playerId, IReadOnlyList<Guid?> slots)
    {
        if (slots.Count > StarterSquad.Size)
            throw new InvalidOperationException($"Squad can't have more than {StarterSquad.Size} places");

        var squadIds = slots.Where(id => id != null).Select(id => id!.Value).ToList();
        if (squadIds.Count == 0)
            throw new InvalidOperationException("Squad must contain at least one hero");
        if (squadIds.Distinct().Count() != squadIds.Count)
            throw new InvalidOperationException("The same hero can't take several places");

        var heroes = await GetByPlayerAsync(playerId);
        var foreign = squadIds.Except(heroes.Select(h => h.Id)).ToList();
        if (foreign.Count > 0)
            throw new InvalidOperationException($"Heroes not found: {string.Join(", ", foreign)}");

        var busy = await BusyHeroIds(squadIds).ToListAsync();
        if (busy.Count > 0)
        {
            var names = heroes.Where(h => busy.Contains(h.Id)).Select(h => h.Name);
            throw new InvalidOperationException($"Heroes on an expedition can't join the squad: {string.Join(", ", names)}");
        }

        var places = slots
            .Select((id, index) => (id, place: index + 1))
            .Where(s => s.id != null)
            .ToDictionary(s => s.id!.Value, s => s.place);

        foreach (var hero in heroes)
            hero.PlaceInSquad = places.TryGetValue(hero.Id, out var place) ? place : null;

        await _context.SaveChangesAsync();
        return heroes;
    }
}
