// Правила магазинов: что и с каким шансом встаёт на полку, сколько стоит и сколько валюты дают бои.
//
// Полка: SlotCount предметов и SlotCount дворфов, обновляется сама раз в сутки (00:00 UTC).
// Раз в сутки — бесплатное ручное обновление, дальше за RerollCost радужных монет. Один предмет и одного дворфа
// можно закрепить — их не трогает ни одно обновление.
//
// Баланс цен рынка (золото) — от доходов и прокачки (HeroUpgradeService, CampaignRewards):
//   кампания на главах 2-го ранга даёт ~1–3 тыс. золота за бой, 3-го — ~5–15 тыс., 4-го — ~20–130 тыс.;
//   прокачка одного дворфа до потолка: 2-й ранг ~5 тыс., 3-й ~126 тыс., 4-й ~7.8 млн.
// Дворф стоит несколько прокачек своего ранга и десятки боёв кампании той главы, где он нужен:
// купить можно, но не каждый день и не всю полку. Камни стихии — по ступени ранга (как в башне:
// обычные много, необычные реже, редкие — с боссов).
public static class ShopRules
{
    public const int SlotCount = 8;

    // Обновления сверх бесплатного — за радужные монеты; новый игрок получает несколько на пробу
    public const int RerollCost = 10;
    public const int StarterRainbowCoins = 50;

    // ===== Шансы (на 10 000) =====

    // Легендарное — 1%, артефакт — 0.1%
    private static readonly (Rarity Rarity, int Weight)[] ItemWeights =
    [
        (Rarity.Common, 4000),
        (Rarity.Uncommon, 3000),
        (Rarity.Rare, 1800),
        (Rarity.Mythic, 1090),
        (Rarity.Legendary, 100),
        (Rarity.Artifact, 10)
    ];

    // Дворф 4-го ранга — 1%
    private static readonly (int Rank, int Weight)[] HeroWeights =
    [
        (1, 4500),
        (2, 3800),
        (3, 1600),
        (4, 100)
    ];

    public static double ItemChance(Rarity rarity) =>
        ItemWeights.Where(w => w.Rarity == rarity).Sum(w => w.Weight) / (double)ItemWeights.Sum(w => w.Weight);

    public static double HeroChance(int rank) =>
        HeroWeights.Where(w => w.Rank == rank).Sum(w => w.Weight) / (double)HeroWeights.Sum(w => w.Weight);

    public static Rarity RollItemRarity(Random random) => Roll(ItemWeights, random);

    public static int RollHeroRank(Random random) => Roll(HeroWeights, random);

    // Предмет выпавшей редкости; в каталоге её может не оказаться — тогда ближайшая ниже
    public static EquipmentDefinition RollItem(Random random)
    {
        for (var rarity = RollItemRarity(random); rarity >= Rarity.Common; rarity--)
        {
            var pool = EquipmentCatalog.All.Where(i => i.Rarity == rarity).ToList();
            if (pool.Count > 0)
                return pool[random.Next(pool.Count)];
        }
        return EquipmentCatalog.All[0];
    }

    // Дворф для полки. На рынке — чистый рандом, как в сундуках. На подпольном рынке и в лавке гильдии
    // свои правила (пока одни на двоих): пассивка всегда наибольшего уровня для ранга, иммунитет у 4-го ранга втрое чаще
    public const double SpecialImmunityChance = 0.3;

    public static Hero RollHero(ShopKind shop, Guid playerId, Random random)
    {
        var hero = HeroGenerator.Create(playerId, RollHeroRank(random), string.Empty, random);
        if (shop == ShopKind.Market)
            return hero;

        if (hero.Passives.Count > 0)
            hero.PassiveLevel = Math.Max(hero.PassiveLevel, PassiveBook.MaxLevelFor(hero.Level));
        if (hero.Level >= HeroGenerator.ImmunityMinRank && hero.Immunities.Count == 0 && random.NextDouble() < SpecialImmunityChance)
            hero.Immunities = [HeroGenerator.Immunities[random.Next(HeroGenerator.Immunities.Length)]];
        return hero;
    }

    // ===== Цены рынка =====

    private static readonly Dictionary<Rarity, long> ItemGold = new()
    {
        [Rarity.Common] = 2_000,
        [Rarity.Uncommon] = 7_500,
        [Rarity.Rare] = 30_000,
        [Rarity.Mythic] = 120_000,
        [Rarity.Legendary] = 600_000,
        [Rarity.Artifact] = 3_000_000
    };

    private static readonly Dictionary<int, long> HeroGold = new()
    {
        [1] = 3_000,
        [2] = 30_000,
        [3] = 400_000,
        [4] = 6_000_000
    };

    // Камни стихии дворфа: ступень по рангу и сколько. У 1-го ранга стихии нет — только золото
    private static readonly Dictionary<int, (Rarity Rarity, int Amount)> HeroGems = new()
    {
        [2] = (Rarity.Common, 40),
        [3] = (Rarity.Uncommon, 30),
        [4] = (Rarity.Rare, 20)
    };

    // ===== Цены подпольного рынка и лавки гильдии (своя валюта) =====
    // Доход: PvP — до ~10 боёв в сутки по BloodCoinsFor; гильдия — удар раз в час по каждому из трёх боссов.
    // Редкий предмет — пара дней игры, дворф 3-го ранга — неделя, 4-го — около месяца

    private static readonly Dictionary<Rarity, long> ItemCurrency = new()
    {
        [Rarity.Common] = 15,
        [Rarity.Uncommon] = 40,
        [Rarity.Rare] = 100,
        [Rarity.Mythic] = 250,
        [Rarity.Legendary] = 900,
        [Rarity.Artifact] = 3_000
    };

    private static readonly Dictionary<int, long> HeroCurrency = new()
    {
        [1] = 25,
        [2] = 90,
        [3] = 350,
        [4] = 1_500
    };

    // Способности дорожают дворфа: каждый уровень пассивки выше первого +15%, иммунитет +30%.
    // Заклинание и аура положены рангу и в цену ранга уже входят
    public const double PassiveLevelMarkup = 0.15;
    public const double ImmunityMarkup = 0.3;

    public static double AbilityFactor(Hero hero)
    {
        double factor = 1;
        if (hero.Passives.Count > 0)
            factor += PassiveLevelMarkup * (Math.Clamp(hero.PassiveLevel, PassiveBook.MinLevel, PassiveBook.MaxLevel) - PassiveBook.MinLevel);
        if (hero.Immunities.Count > 0)
            factor += ImmunityMarkup;
        return factor;
    }

    // Редкость дворфа на полке — как у героя из сундука (LootSeedData.Hero): по рангу
    public static Rarity RarityOf(int rank) => (Rarity)Math.Clamp(rank - 1, (int)Rarity.Common, (int)Rarity.Legendary);

    public static ShopPrice PriceOf(ShopKind shop, EquipmentDefinition item) => shop == ShopKind.Market
        ? new ShopPrice { Gold = ItemGold[item.Rarity] }
        : new ShopPrice { Currency = ItemCurrency[item.Rarity] };

    public static ShopPrice PriceOf(ShopKind shop, Hero hero)
    {
        int rank = Math.Clamp(hero.Level, HeroLevels.Min, HeroLevels.Available);
        double factor = AbilityFactor(hero);
        if (shop != ShopKind.Market)
            return new ShopPrice { Currency = Nice(HeroCurrency[rank] * factor) };

        var price = new ShopPrice { Gold = Nice(HeroGold[rank] * factor) };
        if (hero.Element != HeroElement.Basic && HeroGems.TryGetValue(rank, out var gems))
        {
            price.Gem = InfinityTower.GemOf(hero.Element);
            price.GemRarity = gems.Rarity;
            price.Gems = (int)Nice(gems.Amount * factor);
        }
        return price;
    }

    // Круглая цена: две значащие цифры (3 450 000 → 3 500 000, 117 → 120)
    public static long Nice(double value)
    {
        if (value < 100)
            return (long)Math.Round(value, MidpointRounding.AwayFromZero);
        double step = Math.Pow(10, Math.Floor(Math.Log10(value)) - 1);
        return (long)(Math.Round(value / step, MidpointRounding.AwayFromZero) * step);
    }

    // ===== Заработок валют =====

    // Кровавые монеты за бой на арене: победа в рейтинговой дороже, за поражение — утешительная монета
    public static int BloodCoinsFor(PvpMode mode, bool won) => (won, mode) switch
    {
        (false, _) => 1,
        (true, PvpMode.Ranked) => 6,
        _ => 4
    };

    // Значки почёта: за каждый удар по боссу гильдии и сверху — за награду убитого босса
    public static int HonorBadgesForAttack(GuildBossTier tier) => tier switch
    {
        GuildBossTier.Easy => 2,
        GuildBossTier.Medium => 3,
        _ => 5
    };

    public static int HonorBadgesForKill(GuildBossTier tier) => tier switch
    {
        GuildBossTier.Easy => 10,
        GuildBossTier.Medium => 20,
        _ => 30
    };

    private static T Roll<T>((T Value, int Weight)[] table, Random random)
    {
        int roll = random.Next(table.Sum(t => t.Weight));
        foreach (var (value, weight) in table)
        {
            if (roll < weight) return value;
            roll -= weight;
        }
        return table[^1].Value;
    }
}

// Цена товара: Gold и камни — на рынке, Currency — своя валюта подпольного рынка или лавки гильдии
public class ShopPrice
{
    public long Gold { get; set; }
    public long Currency { get; set; }
    public GemType? Gem { get; set; }
    public Rarity GemRarity { get; set; }
    public int Gems { get; set; }
}
