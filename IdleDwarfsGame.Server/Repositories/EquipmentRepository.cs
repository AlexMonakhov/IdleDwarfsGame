using Microsoft.EntityFrameworkCore;

public class EquipmentRepository(GameDbContext context) : IEquipmentRepository
{
    public async Task<List<PlayerEquipment>> GetByPlayerAsync(Guid playerId)
    {
        return await context.PlayerEquipment
            .Where(e => e.PlayerId == playerId)
            .ToListAsync();
    }

    public async Task<PlayerEquipment?> GetAsync(Guid playerId, Guid itemId)
    {
        return await context.PlayerEquipment
            .FirstOrDefaultAsync(e => e.Id == itemId && e.PlayerId == playerId);
    }

    public async Task EquipAsync(PlayerEquipment item, Guid heroId, EquipmentSlot slot)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Двумя сохранениями: уникальный индекс (герой, слот) не должен видеть два предмета в одном слоте
        var occupant = await context.PlayerEquipment
            .FirstOrDefaultAsync(e => e.HeroId == heroId && e.Slot == slot && e.Id != item.Id);
        if (occupant != null)
        {
            occupant.HeroId = null;
            occupant.Slot = null;
            await context.SaveChangesAsync();
        }

        item.HeroId = heroId;
        item.Slot = slot;
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task UnequipAsync(PlayerEquipment item)
    {
        item.HeroId = null;
        item.Slot = null;
        await context.SaveChangesAsync();
    }
}
