// Инвентарь сундуков и гемов. Открытие сундука: бросок его таблицы дропа (ItemDropService),
// гемы ложатся стопками в PlayerItems, герои создаются вне отряда
public class ChestService(
    IInventoryRepository inventory,
    ItemDropService drops,
    IEntityFactory entityFactory,
    Random random)
{
    public async Task<InventoryInfo> GetInventoryAsync(Guid playerId)
    {
        var chests = await inventory.GetClosedChestsAsync(playerId);
        var items = await inventory.GetItemsAsync(playerId);

        return new InventoryInfo
        {
            Chests = chests.Select(ChestInfo.From).ToList(),
            Items = items
                .OrderBy(i => i.ItemType)
                .ThenByDescending(i => i.Rarity)
                .ThenBy(i => i.SubType)
                .Select(ItemInfo.From)
                .ToList()
        };
    }

    public async Task<ChestOpenResult> OpenAsync(Guid playerId, Guid chestId)
    {
        var chest = await inventory.GetClosedChestAsync(playerId, chestId)
            ?? throw new InvalidOperationException("Chest not found or already opened");

        var loot = drops.Roll(chest.ChestDropTable);
        var items = loot.Where(d => d.ItemType is not (ItemType.Hero or ItemType.Equipment)).ToList();
        var heroes = loot
            .Where(d => d.ItemType == ItemType.Hero)
            .SelectMany(d => Enumerable.Range(0, d.Amount).Select(_ => HeroGenerator.Create(playerId, d.Level, d.SubType, random)))
            .ToList();
        // Снаряжение не складывается в стопки: каждая штука — своя строка
        var equipment = loot
            .Where(d => d.ItemType == ItemType.Equipment)
            .SelectMany(d => Enumerable.Range(0, d.Amount).Select(_ => new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = playerId, ItemKey = d.SubType }))
            .ToList();

        if (!await inventory.OpenChestAsync(chest, items, heroes, equipment))
            throw new InvalidOperationException("Chest is already opened");

        return new ChestOpenResult
        {
            Chest = ChestInfo.From(chest),
            Items = items.Select(ItemInfo.From).ToList(),
            // Только что созданный герой не может быть в экспедиции
            Heroes = heroes.Select(h => HeroInfo.From(entityFactory, h, onExpedition: false)).ToList(),
            Equipment = equipment.Select(EquipmentInfo.From).OfType<EquipmentInfo>().ToList()
        };
    }
}
