using IdleDwarfsGame.Server.Enums;

public enum FixedRewardKind { Hero, Item, Chest, Gold, RainbowCoins }

// Ивентовый герой задан целиком: ранг, стихия, имя, иммунитет и пассивка максимального для ранга уровня.
// Профессия, заклинание и пассивка выбираются генератором, но с постоянным Seed — у всех игроков один и тот же герой
public record FixedHero(string Name, int Rank, HeroElement Element, int Seed, string? Immunity);

// Товар лавки. Limit — сколько раз можно купить за период (неделю или ивент).
// Сундук — уровня текущей главы кампании; золото — в «уровнях кампании», как у заданий (DailyQuests.GoldFor)
public record FixedOffer(
    string Key,
    FixedRewardKind Kind,
    long Price,
    int Limit,
    FixedHero? Hero = null,
    string? ItemKey = null,
    ChestType? Chest = null,
    int Amount = 1,
    double GoldStages = 0);

// Лавки с постоянным ассортиментом: товары и цены захардкожены, ничего не выпадает случайно.
//   Лавка старейшин — за печати ежедневных заданий (~350 в неделю), лимиты обновляются с неделей.
//     Всё сразу стоит 540 печатей — за неделю приходится выбирать.
//   Лавка ивента — за валюту текущего ивента, лимиты на ивент. Главные награды — герой ивента, легендарный предмет
//     и артефакт; остальное — сундуки и золото, чтобы потратить остаток. Всё сразу — 8 400: активный игрок
//     за ивент набирает ~3 500–4 000 (задания дают ~600, остальное — активность ивента), то есть героя и ещё что-то одно.
// Период обеих лавок — неделя ивентов (WorldEvents.WeekOf): с её концом лимиты сбрасываются, а валюта ивента сгорает
public static class FixedShops
{
    public static readonly FixedOffer[] Elder =
    [
        new("gem_chest", FixedRewardKind.Chest, Price: 40, Limit: 3, Chest: ChestType.Gem),
        new("hero_chest", FixedRewardKind.Chest, Price: 60, Limit: 2, Chest: ChestType.Hero),
        new("equipment_chest", FixedRewardKind.Chest, Price: 60, Limit: 2, Chest: ChestType.Equipment),
        new("gold_pouch", FixedRewardKind.Gold, Price: 20, Limit: 5, GoldStages: 5),
        new("rainbow_coins", FixedRewardKind.RainbowCoins, Price: 80, Limit: 1, Amount: 10)
    ];

    public static readonly FixedHero MinesHero = new("Brokk Deepvein", 4, HeroElement.Lightning, Seed: 1102, Immunity: "Stun");
    public static readonly FixedHero BossesHero = new("Sigrun Bossbane", 4, HeroElement.Blood, Seed: 1109, Immunity: "Bleed");

    // Общая часть лавок ивентов: сундуки и золото
    private static readonly FixedOffer[] EventFillers =
    [
        new("hero_chest", FixedRewardKind.Chest, Price: 150, Limit: 5, Chest: ChestType.Hero),
        new("equipment_chest", FixedRewardKind.Chest, Price: 150, Limit: 3, Chest: ChestType.Equipment),
        new("gem_chest", FixedRewardKind.Chest, Price: 100, Limit: 5, Chest: ChestType.Gem),
        new("gold_pouch", FixedRewardKind.Gold, Price: 50, Limit: 10, GoldStages: 5)
    ];

    public static readonly FixedOffer[] CommunityMines =
    [
        new("event_hero", FixedRewardKind.Hero, Price: 2_000, Limit: 1, Hero: MinesHero),
        new("artifact", FixedRewardKind.Item, Price: 3_000, Limit: 1, ItemKey: "ArmageddonBlade"),
        new("legendary", FixedRewardKind.Item, Price: 1_200, Limit: 1, ItemKey: "HeartOfTheMountain"),
        .. EventFillers
    ];

    public static readonly FixedOffer[] WorldBosses =
    [
        new("event_hero", FixedRewardKind.Hero, Price: 2_000, Limit: 1, Hero: BossesHero),
        new("artifact", FixedRewardKind.Item, Price: 3_000, Limit: 1, ItemKey: "ArmageddonBlade"),
        new("legendary", FixedRewardKind.Item, Price: 1_200, Limit: 1, ItemKey: "WarlordHelm"),
        .. EventFillers
    ];

    public static FixedOffer[] CatalogOf(FixedShopKind shop, WorldEventWindow window) => shop switch
    {
        FixedShopKind.Elder => Elder,
        FixedShopKind.Event => window.Kind == WorldEventKind.CommunityMines ? CommunityMines : WorldBosses,
        _ => []
    };

    public static DateTime PeriodOf(DateTime utc) => WorldEvents.WeekOf(utc);

    public static Hero CreateHero(FixedHero definition, Guid playerId)
    {
        var hero = HeroGenerator.Create(playerId, definition.Rank, definition.Element.ToString(), new Random(definition.Seed));
        hero.Name = definition.Name;
        if (hero.Passives.Count > 0)
            hero.PassiveLevel = PassiveBook.MaxLevelFor(hero.Level);
        hero.Immunities = definition.Immunity == null ? [] : [definition.Immunity];
        return hero;
    }

    // Редкость товара — для рамки карточки
    public static Rarity RarityOf(FixedOffer offer) => offer.Kind switch
    {
        FixedRewardKind.Hero => ShopRules.RarityOf(offer.Hero!.Rank),
        FixedRewardKind.Item => EquipmentCatalog.Find(offer.ItemKey!)?.Rarity ?? Rarity.Common,
        FixedRewardKind.RainbowCoins => Rarity.Mythic,
        FixedRewardKind.Chest => offer.Chest == ChestType.Gem ? Rarity.Uncommon : Rarity.Rare,
        _ => Rarity.Common
    };
}
