using IdleDwarfsGame.Server.Entities.GameEntities;
using IdleDwarfsGame.Server.Enums;

public class ChestInfo
{
    public Guid Id { get; set; }
    public ChestType Type { get; set; }
    public int Level { get; set; }

    public static ChestInfo From(PlayerChest chest) => new()
    {
        Id = chest.Id,
        Type = chest.ChestDropTable.ChestType,
        Level = chest.ChestDropTable.ChestLevel
    };
}

// Стопка предметов: в инвентаре Quantity — сколько всего, в результате открытия сундука — сколько выпало
public class ItemInfo
{
    public ItemType ItemType { get; set; }
    // Для гемов — GemType ("Ruby")
    public string SubType { get; set; } = string.Empty;
    public Rarity Rarity { get; set; }
    public int Level { get; set; }
    public int Quantity { get; set; }

    public static ItemInfo From(PlayerItem item) => new()
    {
        ItemType = item.ItemType,
        SubType = item.SubType,
        Rarity = item.Rarity,
        Level = item.Level,
        Quantity = item.Quantity
    };

    public static ItemInfo From(LootDrop drop) => new()
    {
        ItemType = drop.ItemType,
        SubType = drop.SubType,
        Rarity = drop.Rarity,
        Level = drop.Level,
        Quantity = drop.Amount
    };
}

public class InventoryInfo
{
    // Закрытые сундуки
    public List<ChestInfo> Chests { get; set; } = new();
    public List<ItemInfo> Items { get; set; } = new();
}

public class ChestOpenResult
{
    public ChestInfo Chest { get; set; } = new();
    public List<ItemInfo> Items { get; set; } = new();
    public List<HeroInfo> Heroes { get; set; } = new();
    public List<EquipmentInfo> Equipment { get; set; } = new();
}
