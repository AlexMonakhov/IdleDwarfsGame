using IdleDwarfsGame.Server.Enums;

// Бесконечная башня — недельное событие. Этажи идут блоками по FloorsPerBlock: сначала мобы одной стихии
// (на каждом этаже свой состав, Lineups), последний этаж блока — босс той же стихии. Бой — на арене стихии блока.
// Стихии блоков идут по кругу ElementOrder, а каждую неделю круг сдвигается на одну стихию:
// на неделе RotationStart башня начинается с огня, на следующей — со льда и т.д.
// Неделя — с понедельника 00:00 UTC (как недельный сезон PvP); в новую неделю башня начинается с первого этажа,
// а этажи до лучшего результата прошлых недель можно пропустить разом, забрав их награды (TowerService.SkipAsync)
public static class InfinityTower
{
    public const int FloorsPerBlock = 5;

    public static readonly HeroElement[] ElementOrder =
    [
        HeroElement.Fire, HeroElement.Frost, HeroElement.Nature, HeroElement.Lightning,
        HeroElement.Dark, HeroElement.Radiance, HeroElement.Blood
    ];

    // Неделя, на которой круг стихий начинался с огня
    public static readonly DateTime RotationStart = new(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc);

    // Этаж — уровень врагов, как у кампании (CampaignGenerator.EnemyLevelFor): статы растут ×1.045 за уровень
    // (BalanceCalculator). Башня идёт за прогрессом игрока: первый этаж — как конец вводной кампании,
    // дальше по EnemyLevelsPerFloor за этаж, поэтому каждый новый этаж требует заметно прокачанного отряда.
    // У босса HP дополнительно умножается на BossHpMultiplier
    public const int FirstFloorEnemyLevel = 13;
    public const int EnemyLevelsPerFloor = 2;
    public const double BossHpMultiplier = CampaignGenerator.BossHpMultiplier;

    public static int EnemyLevelFor(int floor) => FirstFloorEnemyLevel + (Math.Max(1, floor) - 1) * EnemyLevelsPerFloor;

    // Уровень заклинаний врагов (у босса на один выше) — по уровню врагов, с теми же порогами, что у кампании
    // (CampaignGenerator.MonsterSpellLevel: до 50-го уровня кампании — 1, до 150-го — 2)
    public static int SpellLevelFor(int floor) => EnemyLevelFor(floor) switch
    {
        <= 22 => 1,
        <= 54 => 2,
        _ => 3
    };

    // Враги этажа по позициям (первая — передняя линия): индекс — номер этажа в блоке.
    // Босс — на 3-м слоте (правила расстановки — в шапке CampaignSeedData)
    public static readonly TowerSeedData.Role[][] Lineups =
    [
        [TowerSeedData.Role.Guard, TowerSeedData.Role.Striker, TowerSeedData.Role.Caster],
        [TowerSeedData.Role.Guard, TowerSeedData.Role.Mystic, TowerSeedData.Role.Caster, TowerSeedData.Role.Striker],
        [TowerSeedData.Role.Guard, TowerSeedData.Role.Striker, TowerSeedData.Role.Mystic, TowerSeedData.Role.Caster, TowerSeedData.Role.Striker],
        [TowerSeedData.Role.Guard, TowerSeedData.Role.Guard, TowerSeedData.Role.Caster, TowerSeedData.Role.Mystic, TowerSeedData.Role.Caster],
        [TowerSeedData.Role.Guard, TowerSeedData.Role.Mystic, TowerSeedData.Role.Boss, TowerSeedData.Role.Caster]
    ];

    // Арена и камень стихии
    private static readonly Dictionary<HeroElement, (int ArenaId, GemType Gem)> ElementInfo = new()
    {
        [HeroElement.Fire] = (CampaignSeedData.LavaArenaId, GemType.Ruby),
        [HeroElement.Frost] = (CampaignSeedData.IceArenaId, GemType.Sapphire),
        [HeroElement.Lightning] = (CampaignSeedData.StormArenaId, GemType.Topaz),
        [HeroElement.Nature] = (CampaignSeedData.MarshArenaId, GemType.Emerald),
        [HeroElement.Dark] = (CampaignSeedData.CryptArenaId, GemType.Amethyst),
        [HeroElement.Radiance] = (CampaignSeedData.SanctumArenaId, GemType.Diamond),
        [HeroElement.Blood] = (CampaignSeedData.AltarArenaId, GemType.Garnet)
    };

    public static DateTime WeekOf(DateTime utc) => PvpSeasons.StartOf(PvpSeasonType.Weekly, utc);

    public static DateTime EndOf(DateTime week) => week.AddDays(7);

    // Сколько недель прошло с RotationStart (до неё — отрицательное число)
    public static int WeekIndex(DateTime week) => (int)Math.Floor((WeekOf(week) - RotationStart).TotalDays / 7);

    public static int BlockOf(int floor) => (floor - 1) / FloorsPerBlock;

    public static bool IsBoss(int floor) => floor % FloorsPerBlock == 0;

    public static HeroElement ElementOf(DateTime week, int floor)
    {
        int index = (WeekIndex(week) + BlockOf(floor)) % ElementOrder.Length;
        return ElementOrder[index < 0 ? index + ElementOrder.Length : index];
    }

    // Стихии блоков недели по порядку: первая — у этажей 1–5
    public static List<HeroElement> RotationOf(DateTime week) =>
        Enumerable.Range(0, ElementOrder.Length).Select(block => ElementOf(week, block * FloorsPerBlock + 1)).ToList();

    public static GemType GemOf(HeroElement element) => ElementInfo[element].Gem;

    // Арены лежат в сидах вместе с кампанией; в БД — те же строки
    public static Arena ArenaOf(HeroElement element) =>
        CampaignSeedData.Arenas.First(a => a.Id == ElementInfo[element].ArenaId);

    public static TowerFloor Floor(DateTime week, int floor)
    {
        floor = Math.Max(1, floor);
        var element = ElementOf(week, floor);
        int spellLevel = SpellLevelFor(floor);
        var lineup = Lineups[(floor - 1) % FloorsPerBlock];

        return new TowerFloor(
            floor,
            EnemyLevelFor(floor),
            element,
            IsBoss(floor) ? StageType.Boss : StageType.Mobs,
            ArenaOf(element),
            lineup.Select((role, index) => new TowerEnemy(TowerSeedData.Create(element, role, spellLevel), index + 1, role == TowerSeedData.Role.Boss))
                .ToList());
    }
}

public record TowerFloor(int Number, int EnemyLevel, HeroElement Element, StageType Type, Arena Arena, List<TowerEnemy> Enemies);

public record TowerEnemy(Hero Hero, int Position, bool IsBoss);

// Награда за этаж — фиксированная, поэтому её видно заранее, а пропуск этажей даёт ровно сумму их наград.
// Каждый этаж: золото, опыт и обычные камни стихии этажа. Босс: всё вдвое, необычные (с 20-го этажа и редкие) камни
// и сундук бесконечности своей стихии; каждый второй босс — ещё и сундук снаряжения.
// Что лежит в сундуках — таблицы дропа (LootSeedData)
public static class TowerRewards
{
    // Золото и опыт этажа — как фиксированная часть награды уровня кампании с теми же врагами (CampaignRewards.FixedFor):
    // по силе врагов этажа. Башню проходят каждую неделю, но сумма этажей набирается в основном последними,
    // поэтому за неделю выходит примерно как 6–7 уровней кампании на передовой игрока.
    // 1-й этаж → ~110 золота, 10-й → ~490, 30-й → ~14 тыс., 40-й → ~74 тыс. (у босса ×BossMultiplier)
    public const double BossMultiplier = 2;

    // Уровень сундуков растёт каждые 10 этажей: 5 → 1, 10–19 → 2, дальше 3
    public static int ChestLevelFor(int floor) => Math.Clamp(1 + floor / 10, 1, LootSeedData.MaxInfinityChestLevel);

    public static int CommonGemsFor(int floor) => 2 + floor / 2;

    public static PlayerReward For(Guid playerId, DateTime week, int floor, DateTime now) =>
        ForFloors(playerId, week, floor, floor, now);

    // Одна награда за этажи from..to включительно: золото и опыт складываются, одинаковые камни — в одну стопку
    public static PlayerReward ForFloors(Guid playerId, DateTime week, int from, int to, DateTime now)
    {
        var rewardId = Guid.NewGuid();
        var reward = new PlayerReward
        {
            Id = rewardId,
            PlayerId = playerId,
            Source = RewardSource.Tower,
            SourceId = to,
            CreatedAt = now
        };

        var gems = new Dictionary<(GemType Gem, Rarity Rarity), int>();
        void AddGems(GemType gem, Rarity rarity, int amount)
        {
            if (amount > 0)
                gems[(gem, rarity)] = gems.GetValueOrDefault((gem, rarity)) + amount;
        }

        for (int floor = Math.Max(1, from); floor <= to; floor++)
        {
            var element = InfinityTower.ElementOf(week, floor);
            var gem = InfinityTower.GemOf(element);
            bool boss = InfinityTower.IsBoss(floor);
            double multiplier = boss ? BossMultiplier : 1;
            double scale = Math.Pow(BalanceCalculator.Factor(InfinityTower.EnemyLevelFor(floor)), CampaignRewards.RewardExponent) * multiplier;

            reward.Gold += (int)Math.Round(CampaignRewards.BaseGold * scale, MidpointRounding.AwayFromZero);
            reward.Experience += (int)Math.Round(CampaignRewards.BaseExperience * scale, MidpointRounding.AwayFromZero);
            AddGems(gem, Rarity.Common, (int)(CommonGemsFor(floor) * multiplier));

            if (!boss)
                continue;

            AddGems(gem, Rarity.Uncommon, 1 + floor / 10);
            AddGems(gem, Rarity.Rare, floor / 20);

            int chestLevel = ChestLevelFor(floor);
            reward.Chests.Add(new PlayerRewardChest
            {
                Id = Guid.NewGuid(), RewardId = rewardId, ChestType = ChestType.Infinity, ChestLevel = chestLevel, Element = element
            });
            if (floor % (InfinityTower.FloorsPerBlock * 2) == 0)
                reward.Chests.Add(new PlayerRewardChest { Id = Guid.NewGuid(), RewardId = rewardId, ChestType = ChestType.Equipment, ChestLevel = chestLevel });
        }

        reward.Items = gems
            .OrderBy(g => g.Key.Rarity)
            .Select(g => new PlayerRewardItem
            {
                Id = Guid.NewGuid(), RewardId = rewardId, ItemType = ItemType.Gem, SubType = g.Key.Gem.ToString(),
                Rarity = g.Key.Rarity, Level = 1, Quantity = g.Value
            })
            .ToList();
        return reward;
    }
}
