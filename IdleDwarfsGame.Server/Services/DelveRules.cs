public enum DelveNodeType { Fight, Elite, Camp, Treasure, Boss }

public record DelveNode(int Floor, int Column, DelveNodeType Type);

// Реликвия забега: усиливает весь отряд до конца забега. Одну и ту же можно брать сколько угодно раз —
// действие складывается (DelveService.BuildSquadWorld, RewardOffer); у Max — потолок числа копий
public record DelveRelic(string Key, bool Rare = false, int Max = int.MaxValue);

// Награда после узла. Element и Spell этажей 6–10 сами находят дворфа, остальные надеваются на выбранного (NeedsTarget):
//   Element   — дворф 1-го ранга становится дворфом 2-го ранга стихии Element
//   Spell     — спелл SpellId (Level 1..3) стихии Element: на этажах 6–10 — первому дворфу этой стихии без спелла,
//               дальше (Targeted) — выбранному дворфу этой стихии, прежний спелл заменяется
//   Item      — предмет Key из EquipmentCatalog (у дворфа один предмет, новый заменяет старый; стихия — по предмету)
//   Passive   — пассивка Key уровня Level (заменяет прежнюю)
//   Aura      — аура стихии Element (эффект Key) уровня Level — только дворфу этой стихии, заменяет прежнюю
//   SpellUp / PassiveUp / AuraUp — уровень спелла, пассивки или ауры дворфа +1 (до 3)
//   LevelUp   — ранг дворфа +1 (у дворфа 1-го ранга ещё и стихия Element)
public enum DelveRewardKind { Element, Spell, Item, Passive, Aura, LevelUp, SpellUp, PassiveUp, AuraUp }

public record DelveReward(DelveRewardKind Kind, string? Key = null, int Level = 0, int? SpellId = null, HeroElement? Element = null,
    bool Targeted = false)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public bool NeedsTarget => Kind is not (DelveRewardKind.Element or DelveRewardKind.Spell) || Targeted;
}

// «Глубинный спуск» — бесконечный забег-рогалик.
//
// Забег — раз в PeriodDays суток (периоды отсчитываются от 00:00 UTC); в рейтинге периода — самый глубокий этаж.
// Отряд свой, а не игрока: StartSize секироносцев 1-го ранга. Его растят награды после каждого узла (RewardPhase):
//   этажи 1–5  — стихия: дворф 1-го ранга становится дворфом 2-го ранга выбранной стихии (1 из 3);
//   этажи 6–10 — спелл: 1 из 3, только стихий, у которых есть дворф без спелла, уровень спелла случайный;
//   дальше     — что угодно: предмет, пассивка, аура, спелл, улучшение спелла, пассивки или ауры, изредка ранг.
// Стихийное (предметы стихий, ауры, спеллы) выпадает, только если в отряде есть дворф этой стихии, и надевается только на него.
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
    public const double BossHp = 1.3;
    // Уровни кампании рассчитаны на дворфов, прокачанных за золото, а у отряда спуска прокачки нет: враги слабее
    public const double EnemyStrength = 0.25;
    // И с каждым этажом сильнее на столько (сложный процент): отряд рангов 3–4 с предметами не уходит в бесконечность
    public const double DepthGrowth = 0.045;
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

    // Реликвии складываются: каждая копия даёт своё значение ещё раз
    public static readonly DelveRelic[] Relics =
    [
        new("rage"),                 // +RageAttack атаки
        new("stoneskin"),            // +StoneskinHp HP
        new("swiftboots", true),     // +1 скорость
        new("bloodthirst"),          // после победы живые лечатся на BloodthirstHeal
        new("ale"),                  // привал лечит ещё на AleHeal
        new("spark"),                // +SparkEnergy энергии на старте боя
        new("giantslayer"),          // +GiantslayerAttack атаки в боях с элитой и боссом
        new("phoenix", true),        // сразу воскрешает всех павших с PhoenixHp HP
        new("luck"),                 // предметы лучшей редкости выпадают чаще
        new("wisdom"),               // спеллы, пассивки и ауры высокого уровня выпадают чаще
        new("greed"),                // +GreedReward золота и опыта в спуске
        new("insight", true, 2)      // +1 награда на выбор
    ];

    public const double RageAttack = 0.05;
    public const double StoneskinHp = 0.06;
    public const int SwiftSpeed = 1;
    public const double BloodthirstHeal = 0.04;
    public const double AleHeal = 0.1;
    public const double SparkEnergy = 0.1;
    public const double GiantslayerAttack = 0.1;
    public const double PhoenixHp = 0.3;
    // Удача: вес редкости с номером r (Common = 0) умножается на 1 + LuckPerRarity × r за каждую копию
    public const double LuckPerRarity = 0.3;
    // Мудрость: вес уровня L умножается на 1 + WisdomPerLevel × (L - 1) за каждую копию
    public const double WisdomPerLevel = 0.5;
    public const double GreedReward = 0.1;

    public static int Count(IEnumerable<string> relics, string key) => relics.Count(r => r == key);

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

    // Повышение ранга — редкость: ранг 3 и 4 умножают статы в разы
    private static readonly (DelveRewardKind Kind, int Weight)[] RandomKinds =
    [
        (DelveRewardKind.Item, 34), (DelveRewardKind.Passive, 16), (DelveRewardKind.Aura, 12), (DelveRewardKind.Spell, 10),
        (DelveRewardKind.SpellUp, 8), (DelveRewardKind.PassiveUp, 8), (DelveRewardKind.AuraUp, 8), (DelveRewardKind.LevelUp, 3)
    ];

    private static T Pick<T>(Random random, IReadOnlyList<(T Value, double Weight)> table)
    {
        double roll = random.NextDouble() * table.Sum(x => x.Weight);
        foreach (var (value, weight) in table)
        {
            if (roll < weight) return value;
            roll -= weight;
        }
        return table[^1].Value;
    }

    private static T Pick<T>(Random random, IReadOnlyList<(T Value, int Weight)> table) =>
        Pick(random, table.Select(x => (x.Value, (double)x.Weight)).ToList());

    public static int OfferSizeFor(IReadOnlyCollection<string> relics) => OfferSize + Count(relics, "insight");

    // Награды на выбор после узла этажа floor — по сиду забега и этажу. Пустой список — нечего предложить
    public static List<DelveReward> RewardOffer(int seed, int floor, IReadOnlyList<Hero> squad, IReadOnlyCollection<string> relics)
    {
        var random = new Random(Mix(seed, floor, 1000));
        var phase = RewardPhase(floor);
        if (phase == Phase.Element && !squad.Any(h => h.Level == HeroLevels.Min))
            phase = Phase.Spell;
        if (phase == Phase.Spell && SpellElements(squad).Count == 0)
            phase = Phase.Random;

        // Мудрость сдвигает уровни вверх, удача — редкость предметов
        int wisdom = Count(relics, "wisdom");
        var levels = LevelWeights.Select(x => (x.Level, x.Weight * (1 + WisdomPerLevel * wisdom * (x.Level - 1)))).ToList();
        int luck = Count(relics, "luck");
        var rarities = RarityWeights.Select(x => (x.Rarity, x.Weight * (1 + LuckPerRarity * luck * (int)x.Rarity))).ToList();

        int size = OfferSizeFor(relics);
        var offer = new List<DelveReward>();
        for (int tries = 0; offer.Count < size && tries < 80; tries++)
        {
            var reward = phase switch
            {
                Phase.Element => new DelveReward(DelveRewardKind.Element, Element: Pick(random, ElementWeights)),
                Phase.Spell => RandomSpell(random, SpellElements(squad), levels, targeted: false),
                _ => RandomReward(random, squad, levels, rarities)
            };
            if (reward != null && !offer.Contains(reward))
                offer.Add(reward);
        }
        return offer;
    }

    // Стихии, у которых есть дворф без спелла
    public static List<HeroElement> SpellElements(IReadOnlyList<Hero> squad) =>
        squad.Where(h => h.Element != HeroElement.Basic && h.SpellIds.Count == 0).Select(h => h.Element).Distinct().ToList();

    // Стихии отряда (без Basic) — для стихийных наград
    private static List<HeroElement> SquadElements(IReadOnlyList<Hero> squad) =>
        squad.Select(h => h.Element).Where(e => e != HeroElement.Basic).Distinct().ToList();

    private static DelveReward? RandomSpell(Random random, List<HeroElement> elements, List<(int Level, double Weight)> levels, bool targeted)
    {
        if (elements.Count == 0) return null;
        var element = elements[random.Next(elements.Count)];
        var families = ElementSpellBook.FamiliesOf(element);
        if (families.Count == 0) return null;
        int level = Pick(random, levels);
        return new DelveReward(DelveRewardKind.Spell, Level: level,
            SpellId: SpellRegistry.IdOf(families[random.Next(families.Count)], level), Element: element, Targeted: targeted);
    }

    private static DelveReward? RandomReward(Random random, IReadOnlyList<Hero> squad,
        List<(int Level, double Weight)> levels, List<(Rarity Rarity, double Weight)> rarities)
    {
        var elements = SquadElements(squad);
        DelveReward? reward = Pick(random, RandomKinds) switch
        {
            DelveRewardKind.Item => RandomItem(random, squad, Pick(random, rarities)),
            DelveRewardKind.Passive => new DelveReward(DelveRewardKind.Passive,
                Enum.GetValues<HeroPassive>()[random.Next(Enum.GetValues<HeroPassive>().Length)].ToString(), Pick(random, levels)),
            DelveRewardKind.Aura => RandomAura(random, elements, levels),
            DelveRewardKind.Spell => RandomSpell(random, elements, levels, targeted: true),
            DelveRewardKind.SpellUp => new DelveReward(DelveRewardKind.SpellUp),
            DelveRewardKind.PassiveUp => new DelveReward(DelveRewardKind.PassiveUp),
            DelveRewardKind.AuraUp => new DelveReward(DelveRewardKind.AuraUp),
            _ => new DelveReward(DelveRewardKind.LevelUp, Element: Pick(random, ElementWeights))
        };
        // Награда, которую некому надеть, не предлагается
        return reward != null && squad.Any(h => CannotApply(reward, h) == null) ? reward : null;
    }

    // Аура стихии, которая есть в отряде
    private static DelveReward? RandomAura(Random random, List<HeroElement> elements, List<(int Level, double Weight)> levels)
    {
        if (elements.Count == 0) return null;
        var element = elements[random.Next(elements.Count)];
        return AuraBook.EffectFor(element) is { } effect
            ? new DelveReward(DelveRewardKind.Aura, effect, Pick(random, levels), Element: element)
            : null;
    }

    private static DelveReward? RandomItem(Random random, IReadOnlyList<Hero> squad, Rarity rarity)
    {
        var items = EquipmentCatalog.All.Where(i => i.Rarity == rarity && squad.Any(h => i.Fits(h.Element))).ToList();
        return items.Count == 0 ? null : new DelveReward(DelveRewardKind.Item, items[random.Next(items.Count)].Key);
    }

    private static int SpellLevelOf(Hero hero) =>
        hero.SpellIds.Count > 0 && SpellRegistry.All.TryGetValue(hero.SpellIds[0], out var spell) ? spell.Level : 0;

    // Можно ли надеть награду на этого дворфа; null — можно, иначе — почему нельзя
    public static string? CannotApply(DelveReward reward, Hero hero)
    {
        switch (reward.Kind)
        {
            case DelveRewardKind.Item:
                var item = EquipmentCatalog.Find(reward.Key ?? string.Empty);
                if (item == null) return "Unknown item";
                return item.Fits(hero.Element) ? null : "This item does not fit the dwarf's element";
            case DelveRewardKind.Spell:
            case DelveRewardKind.Aura:
                return hero.Element == reward.Element && hero.Element != HeroElement.Basic ? null : "Only a dwarf of this element can take it";
            case DelveRewardKind.SpellUp:
                int spellLevel = SpellLevelOf(hero);
                return spellLevel is > 0 and < ElementSpellBook.MaxLevel ? null : "The dwarf has no spell to improve";
            case DelveRewardKind.PassiveUp:
                return hero.Passives.Count > 0 && hero.PassiveLevel < PassiveBook.MaxLevel ? null : "The dwarf has no passive to improve";
            case DelveRewardKind.AuraUp:
                return hero.AuraEffect != null && hero.AuraLevel < AuraBook.MaxLevel ? null : "The dwarf has no aura to improve";
            case DelveRewardKind.LevelUp:
                return hero.Level < HeroLevels.Available ? null : "The dwarf already has the highest rank";
            default:
                return null;
        }
    }

    // Награда в отряд. target — выбранный игроком дворф (для NeedsTarget), иначе дворф находится сам.
    // Возвращает дворфа, которому досталась награда; null — некому
    public static Hero? Apply(DelveReward reward, List<Hero> squad, Hero? target)
    {
        if (!reward.NeedsTarget)
        {
            var hero = reward.Kind == DelveRewardKind.Element
                ? squad.OrderBy(h => h.PlaceInSquad).FirstOrDefault(h => h.Level == HeroLevels.Min)
                : squad.OrderBy(h => h.PlaceInSquad).FirstOrDefault(h => h.Element == reward.Element && h.SpellIds.Count == 0);
            if (hero == null) return null;
            if (reward.Kind == DelveRewardKind.Element)
                Promote(hero, reward.Element ?? HeroElement.Fire);
            else if (reward.SpellId is { } id)
                hero.SpellIds = [id];
            return hero;
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
            case DelveRewardKind.Spell:
                if (reward.SpellId is not { } spellId) return null;
                target.SpellIds = [spellId];
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
            case DelveRewardKind.SpellUp:
                // Уровни семейства идут подряд (SpellRegistry.IdOf)
                target.SpellIds = [target.SpellIds[0] + 1];
                break;
            case DelveRewardKind.PassiveUp:
                target.PassiveLevel++;
                break;
            case DelveRewardKind.AuraUp:
                AuraBook.Apply(target, AuraBook.Find(target.AuraEffect)!, target.AuraLevel + 1);
                break;
            case DelveRewardKind.LevelUp:
                Promote(target, reward.Element ?? HeroElement.Fire);
                break;
        }
        return target;
    }

    /* ===== Реликвии ===== */

    // Реликвии на выбор: разные, повторять уже взятые можно (кроме упёршихся в Max); редкие выпадают реже
    public static List<string> RelicOffer(int seed, int floor, IReadOnlyCollection<string> owned)
    {
        var random = new Random(Mix(seed, floor, 2000));
        var pool = Relics.Where(r => Count(owned, r.Key) < r.Max).Select(r => (r.Key, Weight: r.Rare ? 1 : 3)).ToList();
        var offer = new List<string>();
        while (offer.Count < OfferSize && pool.Count > 0)
        {
            var key = Pick(random, pool);
            offer.Add(key);
            pool.RemoveAll(x => x.Key == key);
        }
        return offer;
    }
}
