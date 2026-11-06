// Кузница (ForgeService): снятые предметы и камни, из которых можно ковать, и правила
public class ForgeOverview
{
    public List<EquipmentInfo> Items { get; set; } = new();
    public List<ShopGemStack> Gems { get; set; } = new();
    public int ItemFusionSize { get; set; }
    public int GemFusionSize { get; set; }
    // Выше этой редкости не куётся (из легендарных артефакт не получить)
    public Rarity MaxRarity { get; set; }
}

public class ForgeItemResult
{
    public EquipmentInfo Item { get; set; } = new();
    public ForgeOverview Forge { get; set; } = new();
}

public class ForgeGemResult
{
    public GemType Gem { get; set; }
    public Rarity Rarity { get; set; }
    public int Created { get; set; }
    public ForgeOverview Forge { get; set; } = new();
}

public class ForgeItemsRequest
{
    // Может прийти null ("equipmentIds": null) — ForgeService это проверяет
    public List<Guid>? EquipmentIds { get; set; } = new();
}

public class ForgeGemsRequest
{
    public GemType Gem { get; set; }
    public Rarity Rarity { get; set; }
    public int Times { get; set; } = 1;
}
