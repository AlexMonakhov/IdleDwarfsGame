// Причина отказа — фронт показывает по коду переведённый текст (forge.errors.<code>)
public enum ForgeError
{
    WrongItemCount,
    // Предмета нет или он надет на героя — в кузницу идут только снятые
    ItemNotFound,
    DifferentRarities,
    // Из легендарных артефакт не куётся, артефакты дальше не куются
    MaxRarity,
    WrongGemCount,
    // Такого камня или ступени нет (число вместо имени в запросе)
    UnknownGem,
    NotEnoughGems,
    // Предметы или камни изменились параллельным запросом (двойной клик)
    Changed
}

public class ForgeException(ForgeError code, string message) : InvalidOperationException(message)
{
    public ForgeError Code { get; } = code;
}

// Кузница (по образцу слияния дворфов на алтаре, AbilityService.FuseAsync):
//   предметы — ItemFusionSize снятых предметов одной редкости переплавляются в случайный предмет редкостью выше.
//     Вид нового предмета — как у случайного из переплавленных: 5 оружий — точно оружие, 1 оружие и 4 кольца — кольцо в 80%;
//   камни — GemFusionSize камней одной ступени сплавляются в один камень того же вида ступенью выше.
// Выше MaxForgedRarity не куётся: артефакт из легендарных не получить — артефакты только из сундуков и магазина
public class ForgeService(IForgeRepository forge, Random random)
{
    public const int ItemFusionSize = 5;
    public const int GemFusionSize = 5;
    public const Rarity MaxForgedRarity = Rarity.Legendary;
    // Сколько слияний камней за раз — защита от опечатки в запросе
    public const int MaxGemFusions = 1000;

    public static bool CanForgeFrom(Rarity rarity) => rarity < MaxForgedRarity;

    public async Task<ForgeOverview> GetAsync(Guid playerId)
    {
        var items = await forge.GetFreeEquipmentAsync(playerId);
        var gems = await forge.GetGemsAsync(playerId);

        return new ForgeOverview
        {
            Items = items
                .Select(EquipmentInfo.From)
                .OfType<EquipmentInfo>()
                .OrderBy(i => i.Rarity)
                .ThenBy(i => i.Type)
                .ThenBy(i => i.Key)
                .ToList(),
            Gems = gems.OrderBy(g => g.Gem).ThenBy(g => g.Rarity).ToList(),
            ItemFusionSize = ItemFusionSize,
            GemFusionSize = GemFusionSize,
            MaxRarity = MaxForgedRarity
        };
    }

    public async Task<ForgeItemResult> FuseItemsAsync(Guid playerId, IReadOnlyList<Guid>? equipmentIds)
    {
        if (equipmentIds == null || equipmentIds.Count != ItemFusionSize || equipmentIds.Distinct().Count() != ItemFusionSize)
            throw new ForgeException(ForgeError.WrongItemCount, $"Forging needs {ItemFusionSize} different items");

        var free = (await forge.GetFreeEquipmentAsync(playerId)).ToDictionary(e => e.Id);
        var definitions = new List<EquipmentDefinition>();
        foreach (var id in equipmentIds)
        {
            // Надетые в список свободных не попадают — отличаем «надет» от «нет такого» уже по сообщению
            if (!free.TryGetValue(id, out var item))
                throw new ForgeException(ForgeError.ItemNotFound, "Item not found or worn by a hero");
            definitions.Add(EquipmentCatalog.Find(item.ItemKey)
                ?? throw new ForgeException(ForgeError.ItemNotFound, $"Unknown item '{item.ItemKey}'"));
        }

        var rarity = definitions[0].Rarity;
        if (definitions.Any(d => d.Rarity != rarity))
            throw new ForgeException(ForgeError.DifferentRarities, "All items must be of the same rarity");
        if (!CanForgeFrom(rarity))
            throw new ForgeException(ForgeError.MaxRarity, $"{rarity} items cannot be forged into anything higher");

        var forged = RollResult(RollType(definitions, random), rarity + 1, random);
        var result = new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = playerId, ItemKey = forged.Key };
        if (!await forge.FuseItemsAsync(playerId, equipmentIds, result))
            throw new ForgeException(ForgeError.Changed, "One of the items has just changed, try again");

        return new ForgeItemResult
        {
            Item = EquipmentInfo.From(result)!,
            Forge = await GetAsync(playerId)
        };
    }

    public async Task<ForgeGemResult> FuseGemsAsync(Guid playerId, GemType gem, Rarity rarity, int times)
    {
        // JSON принимает енамы и числом: (GemType)99 или (Rarity)-1 — не камни
        if (!Enum.IsDefined(gem) || !Enum.IsDefined(rarity))
            throw new ForgeException(ForgeError.UnknownGem, "Unknown gem or tier");
        if (times < 1 || times > MaxGemFusions)
            throw new ForgeException(ForgeError.WrongGemCount, $"Fuse from 1 to {MaxGemFusions} times");
        if (!CanForgeFrom(rarity))
            throw new ForgeException(ForgeError.MaxRarity, $"{rarity} gems cannot be fused into anything higher");

        int cost = GemFusionSize * times;
        if (!await forge.FuseGemsAsync(playerId, gem, rarity, cost, rarity + 1, times))
            throw new ForgeException(ForgeError.NotEnoughGems, $"Not enough {rarity} {gem}: {cost} needed");

        return new ForgeGemResult
        {
            Gem = gem,
            Rarity = rarity + 1,
            Created = times,
            Forge = await GetAsync(playerId)
        };
    }

    // Вид случайного из переплавленных: шанс вида — его доля среди них
    public static EquipmentType RollType(IReadOnlyList<EquipmentDefinition> items, Random random) =>
        items[random.Next(items.Count)].Type;

    // Случайный предмет этого вида и редкости. Если такого вида на этой редкости в каталоге нет — любой этой редкости
    public static EquipmentDefinition RollResult(EquipmentType type, Rarity rarity, Random random)
    {
        var pool = EquipmentCatalog.All.Where(i => i.Rarity == rarity && i.Type == type).ToList();
        if (pool.Count == 0)
            pool = EquipmentCatalog.All.Where(i => i.Rarity == rarity).ToList();
        if (pool.Count == 0)
            throw new ForgeException(ForgeError.MaxRarity, $"No {rarity} items to forge");
        return pool[random.Next(pool.Count)];
    }
}
