using Microsoft.EntityFrameworkCore;

// Только для Development (Program.cs): настоящий игрок без единого предмета получает по одной штуке всего каталога,
// чтобы снаряжение можно было проверить, не дожидаясь дропа из сундуков. Тестовые аккаунты PvP не трогаем
public static class EquipmentDevSeed
{
    public static async Task SeedAsync(GameDbContext context)
    {
        var players = await context.Users
            .Where(u => !u.Username.StartsWith(TestAccountsSeed.UsernamePrefix))
            .Select(u => u.PlayerId)
            .ToListAsync();
        var withItems = await context.PlayerEquipment
            .Select(e => e.PlayerId)
            .Distinct()
            .ToListAsync();

        var items = players
            .Except(withItems)
            .SelectMany(playerId => EquipmentCatalog.All.Select(definition => new PlayerEquipment
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                ItemKey = definition.Key
            }))
            .ToList();
        if (items.Count == 0)
            return;

        context.PlayerEquipment.AddRange(items);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }
}
