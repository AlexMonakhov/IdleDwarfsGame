using Microsoft.EntityFrameworkCore;

// Причина отказа — фронт показывает по коду переведённый текст (equipment.errors.<code>)
public enum EquipmentError
{
    ItemNotFound,
    HeroNotFound,
    WrongSlot,
    ElementRestricted,
    // Параллельный запрос успел занять слот
    SlotChanged
}

public class EquipmentException(EquipmentError code, string message) : InvalidOperationException(message)
{
    public EquipmentError Code { get; } = code;
}

// Снаряжение игрока: надеть и снять. Что предметы дают в бою — EquipmentCatalog и EntityFactory
public class EquipmentService(
    IEquipmentRepository equipment,
    IHeroesRepository heroes,
    IExpeditionRepository expeditions,
    IEntityFactory entityFactory)
{
    public async Task<List<EquipmentInfo>> GetMyAsync(Guid playerId) =>
        ToInfo(await equipment.GetByPlayerAsync(playerId));

    // slot == null — подходящий слот выбираем сами: для колец свободный, а если оба заняты — первый
    public async Task<EquipmentChangeResult> EquipAsync(Guid playerId, Guid itemId, Guid heroId, EquipmentSlot? slot)
    {
        var item = await equipment.GetAsync(playerId, itemId)
            ?? throw new EquipmentException(EquipmentError.ItemNotFound, "Item not found");
        var definition = EquipmentCatalog.Find(item.ItemKey)
            ?? throw new EquipmentException(EquipmentError.ItemNotFound, $"Unknown item '{item.ItemKey}'");

        var hero = await heroes.GetByIdAsync(heroId);
        if (hero == null || hero.PlayerId != playerId)
            throw new EquipmentException(EquipmentError.HeroNotFound, "Hero not found");

        if (!definition.Fits(hero.Element))
            throw new EquipmentException(EquipmentError.ElementRestricted, $"{hero.Element} hero can't wear {definition.Key}");

        var slots = EquipmentCatalog.SlotsFor(definition.Type);
        if (slot is { } requested && !slots.Contains(requested))
            throw new EquipmentException(EquipmentError.WrongSlot, $"{definition.Key} doesn't fit slot {requested}");

        var target = slot ?? PickSlot(hero, item, slots);

        // Предмет переезжает с другого героя — его характеристики тоже изменятся
        var previousOwner = item.HeroId is { } ownerId && ownerId != heroId
            ? await heroes.GetByIdAsync(ownerId)
            : null;

        try
        {
            await equipment.EquipAsync(item, hero.Id, target);
        }
        catch (DbUpdateException)
        {
            throw new EquipmentException(EquipmentError.SlotChanged, "The slot has just changed, try again");
        }

        return await ChangeResultAsync(playerId, previousOwner == null ? [hero] : [hero, previousOwner]);
    }

    public async Task<EquipmentChangeResult> UnequipAsync(Guid playerId, Guid itemId)
    {
        var item = await equipment.GetAsync(playerId, itemId)
            ?? throw new EquipmentException(EquipmentError.ItemNotFound, "Item not found");

        var owner = item.HeroId is { } ownerId ? await heroes.GetByIdAsync(ownerId) : null;
        await equipment.UnequipAsync(item);

        return await ChangeResultAsync(playerId, owner == null ? [] : [owner]);
    }

    private static EquipmentSlot PickSlot(Hero hero, PlayerEquipment item, EquipmentSlot[] slots)
    {
        // Уже надет на этого героя — остаётся на месте
        if (item.HeroId == hero.Id && item.Slot is { } current)
            return current;

        var taken = hero.Equipment.Select(e => e.Slot).ToHashSet();
        return slots.Where(s => !taken.Contains(s)).DefaultIfEmpty(slots[0]).First();
    }

    private async Task<EquipmentChangeResult> ChangeResultAsync(Guid playerId, List<Hero> changed)
    {
        var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
        return new EquipmentChangeResult
        {
            Heroes = changed.Select(h => HeroInfo.From(entityFactory, h, busy.Contains(h.Id))).ToList(),
            Items = await GetMyAsync(playerId)
        };
    }

    // Предмет с неизвестным ключом (убрали из каталога) в список не попадает: показать его нечем
    private static List<EquipmentInfo> ToInfo(IEnumerable<PlayerEquipment> items) => items
        .Select(EquipmentInfo.From)
        .OfType<EquipmentInfo>()
        .OrderByDescending(i => i.Rarity)
        .ThenBy(i => i.Type)
        .ThenBy(i => i.Key)
        .ToList();
}
