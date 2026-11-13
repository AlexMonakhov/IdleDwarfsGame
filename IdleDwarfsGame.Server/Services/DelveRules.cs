public enum DelveNodeType { Fight, Elite, Camp, Treasure, Boss }

public record DelveNode(int Floor, int Column, DelveNodeType Type);

// Реликвия забега: усиливает весь отряд до конца забега (DelveService.BuildSquadWorld)
public record DelveRelic(string Key, bool Rare = false);

// Награда после узла. Element и Spell сами находят дворфа, остальные — на выбранного игроком (NeedsTarget):
//   Element  — дворф 1-го ранга становится дворфом 2-го ранга стихии Element
//   Spell    — спелл SpellId (Level 1..3) первому дворфу стихии Element без спелла
//   Item     — предмет Key из EquipmentCatalog (у дворфа один предмет, новый заменяет старый)
//   Passive  — пассивка Key уровня Level (заменяет прежнюю)
//   Aura     — аура Key (эффект из AuraBook) уровня Level (заменяет прежнюю)
//   LevelUp  — ранг дворфа +1 (у дворфа 1-го ранга ещё и случайная стихия Element)
public enum DelveRewardKind { Element, Spell, Item, Passive, Aura, LevelUp }

public record DelveReward(DelveRewardKind Kind, string? Key = null, int Level = 0, int? SpellId = null, HeroElement? Element = null)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public bool NeedsTarget => Kind is DelveRewardKind.Item or DelveRewardKind.Passive or DelveRewardKind.Aura or DelveRewardKind.LevelUp;
}

// «Глубинный спуск» — бесконечный забег-рогалик.
//
// Забег — раз в PeriodDays суток (периоды отсчитываются от 00:00 UTC); в рейтинге периода — самый глубокий этаж.
// Отряд свой, а не игрока: StartSize секироносцев 1-го ранга. Его растят награды после каждого узла (RewardPhase):
//   этажи 1–5  — стихия: дворф 1-го ранга становится дворфом 2-го ранга выбранной стихии (1 из 3);
//   этажи 6–10 — спелл: 1 из 3, только стихий, у которых есть дворф без спелла, уровень спелла случайный;
//   дальше     — что угодно: предмет, пассивка, аура, повышение ранга.
// От любой награды можно отказаться и взять золото с опытом на аккаунт.
//
// Карта у каждого забега своя (сид забега), бесконечна и считается по (сид, этаж, столбец) — в базе её нет.
// Columns столбцов, из столбца c можно спуститься в c-1, c, c+1. Каждый BossEvery-й этаж — босс во всех столбцах,
// на каждом CampEvery-м (кроме боссов) хотя бы один привал. Видно только следующий этаж целиком и узел через этаж
// прямо под текущим — чтобы решить, сдвигаться или нет.
//
// Враги — монстры уровня кампании по глубине (StageFor), сила игрока не важна. Награды — от уровня кампании игрока
// и глубины. За каждого побеждённого босса — радужная монета. Пал весь отряд — забег окончен; раз за забег можно
// воскресить всех за ReviveCost радужных монет.
public static class DelveRules
{
    public const int Columns = 5;
    public const int StartColumn = 2;
    public const int BossEvery = 10;
    public const int CampEvery = 5;
    public const int PeriodDays = 3;

    public const int StartSize = 5;
    public const int ElementFloors = 5;
    public const int SpellFloors = 10;

    // Этаж → уровень кампании врагов: 1 + (этаж - 1) × StagePerFloor; элита и босс — сильнее
    public const double StagePerFloor = 0.5;
    public const int EliteStageBonus = 2;
    public const int BossStageBonus = 3;
    public const double EliteHp = 1.25;
    public const double BossHp = 1.5;
    // Уровни кампании рассчитаны на дворфов, прокачанных за золото, а у отряда спуска прокачки нет: враги слабее
    public const double EnemyStrength = 0.35;
    // И с каждым этажом сильнее на столько (сложный процент): отряд рангов 3–4 с предметами не уходит в бесконечность
    public const double DepthGrowth = 0.04;
    // За последним уровнем кампании враги растут на столько за каждый «лишний» уровень
    public const double OverflowPerStage = 0.05;

    // Привал: лечение живых или воскрешение одного павшего
    public const double CampHeal = 0.35;
    public const double CampRevive = 0.4;

    // Воскрешение всего отряда после гибели: раз за забег
    public const int ReviveCost = 20;
    public const int BossRainbowCoins = 1;

    // Золото и опыт за победу: (база + за этаж) уровней кампании игрока; элита и босс — больше
    public const double FightStagesBase = 0.3;
    public const double FightStagesPerFloor = 0.02;
    public const double EliteReward = 2;
    public const double BossReward = 4;
    // Отказ от награды узла
    public const double DeclineStagesBase = 0.5;
    public const double DeclineStagesPerFloor = 0.03;

    public const int LeaderboardSize = 10;
    public const int OfferSize = 3;

    public static readonly DelveRelic[] Relics =
    [
        new("rage"),          // +15% атаки
        new("stoneskin"),     // +20% HP
        new("swiftboots"),    // +1 скорость
        new("bloodthirst"),   // после победы живые лечатся на 10%
        new("ale"),           // привал лечит ещё на 25%
        new("spark"),         // +40% энергии на старте боя
        new("giantslayer"),   // +30% атаки в боях с элитой и боссом
        new("phoenix", true)  // сразу воскрешает всех павших с 30% HP
    ];

    public const double RageAttack = 0.15;
    public const double StoneskinHp = 0.2;
    public const int SwiftSpeed = 1;
    public const double BloodthirstHeal = 0.1;
    public const double AleHeal = 0.25;
    public const double SparkEnergy = 0.4;
    public const double GiantslayerAttack = 0.3;
    public const double PhoenixHp = 0.3;

    /* ===== Период ===== */

    // Периоды по PeriodDays суток подряд, отсчёт — от 1 января 1970 (00:00 UTC)
    public static DateTime PeriodOf(DateTime utc)
    {
        int days = (int)(utc.Date - DateTime.UnixEpoch).TotalDays;
        return DateTime.SpecifyKind(DateTime.UnixEpoch.AddDays(days - days % PeriodDays), DateTimeKind.Utc);
    }

    /* ===== Карта ===== */

    // Детерминированный сид без HashCode.Combine: тот случаен в каждом процессе
    private static int Mix(int seed, int a, int b = 0)
    {
        unchecked
        {
            uint h = (uint)seed * 0x9E3779B1u;
            h ^= (uint)a * 0x85EBCA77u + 0x165667B1u + (h << 6) + (h >> 2);
            h ^= (uint)b * 0xC2B2AE3Du + 0x27D4EB2Fu + (h << 6) + (h >> 2);
            h ^= h >> 15;
            h *= 0x2C1B3C6Du;
            h ^= h >> 12;
            return (int)(h & 0x7FFFFFFF);
        }
    }

    public static bool IsBossFloor(int floor) => floor > 0 && floor % BossEvery == 0;

    public static DelveNode NodeAt(int seed, int floor, int column)
    {
        if (floor == 1)
            return new DelveNode(floor, column, DelveNodeType.Fight);
        if (IsBossFloor(floor))
            return new DelveNode(floor, column, DelveNodeType.Boss);
        if (floor % CampEvery == 0 && column == new Random(Mix(seed, floor, -1)).Next(Columns))
            return new DelveNode(floor, column, DelveNodeType.Camp);
        return new DelveNode(floor, column, Roll(new Random(Mix(seed, floor, column)), floor));
    }

    public static List<DelveNode> FloorOf(int seed, int floor) =>
        Enumerable.Range(0, Columns).Select(c => NodeAt(seed, floor, c)).ToList();

    // Глубже элиты больше
    private static DelveNodeType Roll(Random random, int floor)
    {
        int elite = Math.Min(10 + floor / 2, 25);
        int roll = random.Next(100);
        if (roll < elite) return DelveNodeType.Elite;
        roll -= elite;
        if (roll < 15) return DelveNodeType.Camp;
        roll -= 15;
        if (roll < 12) return DelveNodeType.Treasure;
        return DelveNodeType.Fight;
    }

    // Можно ли с узла (floor, column) спуститься в столбец next следующего этажа. С входа — в любой
    public static bool CanMove(int floor, int column, int next) =>
        next is >= 0 and < Columns && (floor == 0 || Math.Abs(next - column) <= 1);

    /* ===== Враги и награды за бой ===== */

    // Уровень кампании без потолка: выше последнего уровня враги домножаются (EnemyFactor)
    public static int RawStageFor(DelveNode node) =>
        1 + (int)Math.Round((node.Floor - 1) * StagePerFloor) + node.Type switch
        {
            DelveNodeType.Elite => EliteStageBonus,
            DelveNodeType.Boss => BossStageBonus,
            _ => 0
        };

    public static int StageFor(DelveNode node, int maxStage) => Math.Clamp(RawStageFor(node), 1, Math.Max(1, maxStage));

    // Множитель HP и атаки врагов: EnemyStrength с ростом за этаж (DepthGrowth), а за последним уровнем кампании — ещё и рост за каждый лишний уровень
    public static double EnemyFactor(DelveNode node, int maxStage) =>
        EnemyStrength * Math.Pow(1 + DepthGrowth, node.Floor - 1) * Math.Pow(1 + OverflowPerStage, Math.Max(0, RawStageFor(node) - Math.Max(1, maxStage)));

    public static double FightStagesFor(DelveNode node) =>
        (FightStagesBase + FightStagesPerFloor * node.Floor) * node.Type switch
        {
            DelveNodeType.Elite => EliteReward,
            DelveNodeType.Boss => BossReward,
            _ => 1
        };

    public static double DeclineStagesFor(int floor) => DeclineStagesBase + DeclineStagesPerFloor * floor;

    /* ===== Отряд ===== */

    public static List<Hero> StartSquad(Guid playerId, Random random)
    {
        var role = ProfessionBook.Of(HeroProfession.Axeman);
        return StarterSquad.Names.OrderBy(_ => random.Next()).Take(StartSize)
            .Select((name, index) => new Hero
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Name = name,
                UnitType = "Dwarf",
                Profession = HeroProfession.Axeman,
                Element = HeroElement.Basic,
                Level = HeroLevels.Min,
                MaxHp = role.MaxHp,
                Attack = role.Attack,
                Speed = role.Speed,
                PlaceInSquad = index + 1
            })
            .ToList();
    }

    // Новый ранг: статы профессии с множителем ранга, на 1-м → 2-м ранге — стихия
    public static void Promote(Hero hero, HeroElement element)
    {
        hero.Level = Math.Min(hero.Level + 1, HeroLevels.Available);
        if (hero.Element == HeroElement.Basic)
            hero.Element = element;
        var role = ProfessionBook.Of(hero.Profession ?? HeroProfession.Axeman);
        hero.MaxHp = HeroGenerator.ScaleToRank(role.MaxHp, hero.Level);
        hero.Attack = HeroGenerator.ScaleToRank(role.Attack, hero.Level);
    }

    /* ===== Награды узлов ===== */

    public enum Phase { Element, Spell, Random }

    public static Phase RewardPhase(int floor) =>
        floor <= ElementFloors ? Phase.Element : floor <= SpellFloors ? Phase.Spell : Phase.Random;

    // Аметист, гранат и бриллиант реже остальных — как у сундуков (HeroGenerator)
    private static readonly (HeroElement Element, int Weight)[] ElementWeights =
    [
        (HeroElement.Fire, 2), (HeroElement.Frost, 2), (HeroElement.Lightning, 2), (HeroElement.Nature, 2),
        (HeroElement.Dark, 1), (HeroElement.Blood, 1), (HeroElement.Radiance, 1)
    ];

    // Уровень спелла, пассивки и ауры: чаще низкий
    private static readonly (int Level, int Weight)[] LevelWeights = [(1, 50), (2, 35), (3, 15)];

    private static readonly (Rarity Rarity, int Weight)[] RarityWeights =
    [
        (Rarity.Common, 30), (Rarity.Uncommon, 25), (Rarity.Rare, 20), (Rarity.Mythic, 12), (Rarity.Legendary, 8), (Rarity.Artifact, 5)
    ];

    private static readonly (DelveRewardKind Kind, int Weight)[] RandomKinds =
    [
        (DelveRewardKind.Item, 40), (DelveRewardKind.Passive, 25), (DelveRewardKind.Aura, 20), (DelveRewardKind.LevelUp, 15)
    ];

    private static readonly string[] AuraEffects = ElementSpellBook.Elements
        .Select(AuraBook.EffectFor).OfType<string>().Distinct().ToArray();

    private static T Pick<T>(Random random, IReadOnlyList<(T Value, int Weight)> table)
    {
        int roll = random.Next(table.Sum(x => x.Weight));
        foreach (var (value, weight) in table)
        {
            if (roll < weight) return value;
            roll -= weight;
        }
        return table[^1].Value;
    }

    // Награды на выбор после узла этажа floor — по сиду забега и этажу. Пустой список — нечего предложить
    public static List<DelveReward> RewardOffer(int seed, int floor, IReadOnlyList<Hero> squad)
    {
        var random = new Random(Mix(seed, floor, 1000));
        var phase = RewardPhase(floor);
        if (phase == Phase.Element && !squad.Any(h => h.Level == HeroLevels.Min))
            phase = Phase.Spell;
        if (phase == Phase.Spell && SpellElements(squad).Count == 0)
            phase = Phase.Random;

        var offer = new List<DelveReward>();
        for (int tries = 0; offer.Count < OfferSize && tries < 50; tries++)
        {
            var reward = phase switch
            {
                Phase.Element => new DelveReward(DelveRewardKind.Element, Element: Pick(random, ElementWeights)),
                Phase.Spell => RandomSpell(random, squad),
                _ => RandomReward(random, squad)
            };
            if (reward != null && !offer.Contains(reward))
                offer.Add(reward);
        }
        return offer;
    }

    // Стихии, у которых есть дворф без спелла
    public static List<HeroElement> SpellElements(IReadOnlyList<Hero> squad) =>
        squad.Where(h => h.Element != HeroElement.Basic && h.SpellIds.Count == 0).Select(h => h.Element).Distinct().ToList();

    private static DelveReward? RandomSpell(Random random, IReadOnlyList<Hero> squad)
    {
        var elements = SpellElements(squad);
        if (elements.Count == 0) return null;
        var element = elements[random.Next(elements.Count)];
        var families = ElementSpellBook.FamiliesOf(element);
        if (families.Count == 0) return null;
        int level = Pick(random, LevelWeights);
        return new DelveReward(DelveRewardKind.Spell, Level: level,
            SpellId: SpellRegistry.IdOf(families[random.Next(families.Count)], level), Element: element);
    }

    private static DelveReward? RandomReward(Random random, IReadOnlyList<Hero> squad)
    {
        switch (Pick(random, RandomKinds))
        {
            case DelveRewardKind.Item:
                var rarity = Pick(random, RarityWeights);
                var items = EquipmentCatalog.All.Where(i => i.Rarity == rarity && squad.Any(h => i.Fits(h.Element))).ToList();
                return items.Count == 0 ? null : new DelveReward(DelveRewardKind.Item, items[random.Next(items.Count)].Key);
            case DelveRewardKind.Passive:
                var passives = Enum.GetValues<HeroPassive>();
                return new DelveReward(DelveRewardKind.Passive, passives[random.Next(passives.Length)].ToString(), Pick(random, LevelWeights));
            case DelveRewardKind.Aura:
                return new DelveReward(DelveRewardKind.Aura, AuraEffects[random.Next(AuraEffects.Length)], Pick(random, LevelWeights));
            default:
                return squad.Any(h => h.Level < HeroLevels.Available)
                    ? new DelveReward(DelveRewardKind.LevelUp, Element: Pick(random, ElementWeights))
                    : null;
        }
    }

    // Можно ли надеть награду на этого дворфа; null — можно, иначе — почему нельзя
    public static string? CannotApply(DelveReward reward, Hero hero)
    {
        if (reward.Kind == DelveRewardKind.Item)
        {
            var item = EquipmentCatalog.Find(reward.Key ?? string.Empty);
            if (item == null) return "Unknown item";
            if (!item.Fits(hero.Element)) return "This item does not fit the dwarf's element";
        }
        if (reward.Kind == DelveRewardKind.LevelUp && hero.Level >= HeroLevels.Available)
            return "The dwarf already has the highest rank";
        return null;
    }

    // Награда в отряд. target — выбранный игроком дворф (для NeedsTarget), иначе дворф находится сам.
    // Возвращает дворфа, которому досталась награда; null — некому
    public static Hero? Apply(DelveReward reward, List<Hero> squad, Hero? target)
    {
        switch (reward.Kind)
        {
            case DelveRewardKind.Element:
            {
                var hero = squad.OrderBy(h => h.PlaceInSquad).FirstOrDefault(h => h.Level == HeroLevels.Min);
                if (hero != null) Promote(hero, reward.Element ?? HeroElement.Fire);
                return hero;
            }
            case DelveRewardKind.Spell:
            {
                var hero = squad.OrderBy(h => h.PlaceInSquad).FirstOrDefault(h => h.Element == reward.Element && h.SpellIds.Count == 0);
                if (hero != null && reward.SpellId is { } spell) hero.SpellIds = [spell];
                return hero;
            }
        }

        if (target == null || CannotApply(reward, target) != null)
            return null;
        switch (reward.Kind)
        {
            case DelveRewardKind.Item:
                var item = EquipmentCatalog.Find(reward.Key!)!;
                target.Equipment =
                [
                    new PlayerEquipment
                    {
                        Id = Guid.NewGuid(), PlayerId = target.PlayerId, ItemKey = item.Key, HeroId = target.Id,
                        Slot = EquipmentCatalog.SlotsFor(item.Type).First()
                    }
                ];
                break;
            case DelveRewardKind.Passive:
                if (!Enum.TryParse<HeroPassive>(reward.Key, out var passive)) return null;
                target.Passives = [passive];
                target.PassiveLevel = Math.Clamp(reward.Level, PassiveBook.MinLevel, PassiveBook.MaxLevel);
                break;
            case DelveRewardKind.Aura:
                if (AuraBook.Find(reward.Key) is not { } aura) return null;
                AuraBook.Apply(target, aura, reward.Level);
                break;
            case DelveRewardKind.LevelUp:
                Promote(target, reward.Element ?? HeroElement.Fire);
                break;
        }
        return target;
    }

    /* ===== Реликвии ===== */

    // Три реликвии на выбор: без уже взятых, по сиду забега и этажа
    public static List<string> RelicOffer(int seed, int floor, IReadOnlyCollection<string> owned)
    {
        var random = new Random(Mix(seed, floor, 2000));
        return Relics.Where(r => !owned.Contains(r.Key))
            .OrderBy(_ => random.Next())
            .Take(OfferSize)
            .Select(r => r.Key)
            .ToList();
    }
}
