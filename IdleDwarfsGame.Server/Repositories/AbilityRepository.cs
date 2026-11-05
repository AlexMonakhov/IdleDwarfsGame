using Microsoft.EntityFrameworkCore;

public class AbilityRepository(GameDbContext context) : IAbilityRepository
{
    public async Task<List<PlayerAbility>> GetByPlayerAsync(Guid playerId)
    {
        return await context.PlayerAbilities
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId)
            .ToListAsync();
    }

    public async Task<PlayerAbility?> GetAsync(Guid playerId, Guid abilityId)
    {
        return await context.PlayerAbilities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == abilityId && a.PlayerId == playerId);
    }

    public async Task<bool> ExtractAsync(Hero hero, PlayerAbility ability)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // UPDATE без изменений блокирует строку игрока до коммита — параллельные извлечения идут по очереди
        // и проверка «остался ли кто-то» ниже видит результат предыдущего
        await context.Players
            .Where(p => p.Id == hero.PlayerId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, p => p.Gold));

        // Без коммита транзакция откатывается при выходе: дворф остаётся на месте
        if (!await DissolveAsync(hero.PlayerId, [hero.Id]))
            return false;
        // Два параллельных извлечения из двух последних дворфов: второе не проходит
        if (!await context.Heroes.AnyAsync(h => h.PlayerId == hero.PlayerId))
            return false;

        context.PlayerAbilities.Add(ability);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        context.ChangeTracker.Clear();
        return true;
    }

    public async Task<bool> InsertAsync(PlayerAbility ability, Hero hero)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условное удаление: из двух одновременных вставок одной способности пройдёт только первая
        if (!await UseAsync(ability.PlayerId, [ability.Id]))
        {
            context.ChangeTracker.Clear();
            return false;
        }

        if (context.Entry(hero).State == EntityState.Detached)
            context.Heroes.Update(hero);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> CombineAsync(PlayerAbility first, PlayerAbility second, PlayerAbility result)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if (!await UseAsync(first.PlayerId, [first.Id, second.Id]))
            return false;

        context.PlayerAbilities.Add(result);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        context.Entry(result).State = EntityState.Detached;
        return true;
    }

    public async Task<bool> FuseAsync(Guid playerId, IReadOnlyList<Guid> heroIds, Hero result, HeroUpgradeCost refund)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if (!await DissolveAsync(playerId, heroIds))
            return false;

        if (refund.Gold > 0 || refund.Experience > 0)
        {
            await context.Players
                .Where(p => p.Id == playerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Gold, p => p.Gold + refund.Gold)
                    .SetProperty(p => p.Experience, p => p.Experience + refund.Experience));
        }

        context.Heroes.Add(result);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        context.ChangeTracker.Clear();
        return true;
    }

    // Удаляет способности, только если на месте все до одной
    private async Task<bool> UseAsync(Guid playerId, IReadOnlyList<Guid> abilityIds)
    {
        int used = await context.PlayerAbilities
            .Where(a => a.PlayerId == playerId && abilityIds.Contains(a.Id))
            .ExecuteDeleteAsync();
        return used == abilityIds.Count;
    }

    // Распыляет дворфов, только если все они у игрока и свободны (не в экспедиции и не в шахте).
    // Надетые на них предметы возвращаются в инвентарь — до удаления: внешний ключ обнулил бы HeroId, но не Slot.
    // При false вызывающий не коммитит транзакцию, и снятие предметов тоже откатывается
    private async Task<bool> DissolveAsync(Guid playerId, IReadOnlyList<Guid> heroIds)
    {
        await context.PlayerEquipment
            .Where(e => e.PlayerId == playerId && e.HeroId != null && heroIds.Contains(e.HeroId.Value))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.HeroId, (Guid?)null)
                .SetProperty(e => e.Slot, (EquipmentSlot?)null));

        int dissolved = await context.Heroes
            .Where(h => h.PlayerId == playerId && heroIds.Contains(h.Id)
                && !context.ExpeditionHeroes.Any(e => e.HeroId == h.Id)
                && !context.Mines.Any(m => m.HeroId == h.Id))
            .ExecuteDeleteAsync();
        return dissolved == heroIds.Count;
    }
}
