using IdleDwarfsGame.Server.Enums;
// Инвентарь сундуков и гемов. Открытие сундука: бросок его таблицы дропа (ItemDropService),
// гемы ложатся стопками в PlayerItems, герои создаются вне отряда.
// До ChestBonuses.CardCount карт добиваются доп. наградами (ChestBonuses): золото и опыт — по этапу кампании игрока
// (players), валюта ивента — только пока ивент идёт (clock)
public class ChestService(
    IInventoryRepository inventory,
    ItemDropService drops,
    IEntityFactory entityFactory,
    Random random,
    IActivityTracker activity,
    IPlayerRepository players,
    TimeProvider clock,
    OnboardingService? onboarding = null)
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

        // Стартовый сундук обучения не бросается: в нём отряд и запас новичка (один раз на аккаунт)
        bool starter = chest.ChestDropTable.ChestType == ChestType.Starter;
        var loot = starter ? [] : drops.Roll(chest.ChestDropTable);
        var items = loot.Where(d => d.ItemType is not (ItemType.Hero or ItemType.Equipment)).ToList();
        var heroes = starter
            ? onboarding != null ? await onboarding.StarterChestHeroesAsync(playerId) : []
            : loot
                .Where(d => d.ItemType == ItemType.Hero)
                .SelectMany(d => Enumerable.Range(0, d.Amount).Select(_ => HeroGenerator.Create(playerId, d.Level, d.SubType, random)))
                .ToList();
        // Снаряжение не складывается в стопки: каждая штука — своя строка
        var equipment = loot
            .Where(d => d.ItemType == ItemType.Equipment)
            .SelectMany(d => Enumerable.Range(0, d.Amount).Select(_ => new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = playerId, ItemKey = d.SubType }))
            .ToList();

        // Карта на каждую стопку, героя и предмет; остальное до пяти — бонусы
        int mainCards = items.Count + heroes.Count + equipment.Count;
        var activeEvent = WorldEvents.ActiveAt(clock.GetUtcNow().UtcDateTime);
        var bonuses = ChestBonuses.Roll(
            ChestBonuses.CountFor(mainCards),
            chest.ChestDropTable.ChestLevel,
            await players.GetProgressAsync(playerId),
            activeEvent != null,
            random);

        // Бонусные камни — в те же стопки инвентаря, что и камни сундука
        var bonusGems = bonuses
            .Where(b => b.Kind == ChestBonusKind.Gems)
            .Select(b => new LootDrop(ItemType.Gem, b.SubType, b.Rarity, 1, b.Amount));
        var currency = new ChestCurrencyGrant(
            Gold: bonuses.Where(b => b.Kind == ChestBonusKind.Gold).Sum(b => (long)b.Amount),
            Experience: bonuses.Where(b => b.Kind == ChestBonusKind.Experience).Sum(b => (long)b.Amount),
            RainbowCoins: bonuses.Where(b => b.Kind == ChestBonusKind.RainbowCoins).Sum(b => (long)b.Amount),
            EventCurrency: bonuses.Where(b => b.Kind == ChestBonusKind.EventCurrency).Sum(b => (long)b.Amount),
            EventStart: activeEvent?.StartsAt);

        // Одинаковые камни из сундука и бонуса — одной стопкой: две новые строки одной стопки в одной транзакции не сохранить
        var stacks = items.Concat(bonusGems)
            .GroupBy(d => d with { Amount = 0 })
            .Select(g => g.Key with { Amount = g.Sum(d => d.Amount) })
            .ToList();

        if (!await inventory.OpenChestAsync(chest, stacks, heroes, equipment, currency))
            throw new InvalidOperationException("Chest is already opened");
        await activity.RecordAsync(playerId, ActivityKind.ChestOpen);
        // Стартовый сундук поставил пятерых в отряд — мощь отряда изменилась
        if (heroes.Any(h => h.PlaceInSquad != null) && onboarding != null)
            await onboarding.RefreshSquadPowerAsync(playerId);

        return new ChestOpenResult
        {
            Chest = ChestInfo.From(chest),
            Items = items.Select(ItemInfo.From).ToList(),
            // Только что созданный герой не может быть в экспедиции
            Heroes = heroes.Select(h => HeroInfo.From(entityFactory, h, onExpedition: false)).ToList(),
            Equipment = equipment.Select(EquipmentInfo.From).OfType<EquipmentInfo>().ToList(),
            Bonuses = bonuses.Select(ChestBonusInfo.From).ToList()
        };
    }
}
