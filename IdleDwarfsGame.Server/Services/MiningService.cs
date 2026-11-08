// Причина отказа — фронт показывает по коду переведённый текст (mining.errors.<code>)
public enum MiningError
{
    PlotNotFound,
    PlotTaken,
    MineNotFound,
    MaxLevel,
    // Улучшение упёрлось в уровень шахты — сначала поднимите его
    MineLevelTooLow,
    NotEnoughGold,
    NotEnoughExperience,
    NotEnoughCrystals,
    DwarfLocked,
    HeroNotFound,
    HeroInSquad,
    HeroBusy,
    // Все открытые уровнем шахты места заняты
    NoFreeSlot,
    NoDwarf,
    NothingToCollect,
    // Шахту изменили параллельным запросом (двойной клик) — данные уже другие
    Changed
}

public class MiningException(MiningError code, string message) : InvalidOperationException(message)
{
    public MiningError Code { get; } = code;
}

// Шахты: на местах (PlotCount) игрок ставит шахты золота, опыта или кристаллов стихий.
// Сначала шахту копают кликами — каждый клик сразу приносит ресурс. С уровня DwarfUnlockLevel в шахту
// можно поставить дворфа: он копит добычу на складе шахты, пока тот не заполнится, — игрок заходит и забирает.
// Уровень шахты открывает новые места: до MaxDwarves дворфов (DwarfSlotLevels). Каждый дворф копает со скоростью шахты,
// склад растёт вместе с ними — заполняется за то же время, сколько бы их ни было.
// Добыча растёт экспоненциально, как и цены прокачки дворфов: полностью улучшенная золотая шахта даёт ~17 тыс. в час на дворфа.
// Клик стоит ClickSeconds секунд работы шахты, склад вмещает несколько часов её работы
public class MiningService(
    IMiningRepository mining,
    IHeroesRepository heroes,
    IExpeditionRepository expeditions,
    TimeProvider time,
    IActivityTracker activity)
{
    public const int PlotCount = 6;
    public const int MaxLevel = 10;
    public const int DwarfUnlockLevel = 2;

    // Уровни шахты, на которых открывается 1-е, 2-е и 3-е место для дворфа
    public static readonly int[] DwarfSlotLevels = [DwarfUnlockLevel, 4, 7];
    public static int MaxDwarves => DwarfSlotLevels.Length;

    public static int DwarfSlots(int mineLevel) => DwarfSlotLevels.Count(level => mineLevel >= level);

    // Уровень, на котором откроется следующее место; null — все места открыты
    public static int? NextSlotLevel(int mineLevel) => DwarfSlotLevels.Where(level => level > mineLevel).Cast<int?>().FirstOrDefault();

    // Во сколько раз шахта копает быстрее одного дворфа. Без дворфов показываем цифры одного — что он даст
    private static int Crew(Mine mine) => Math.Max(1, mine.Dwarves.Count);
    // Скорость, добыча и склад прокачиваются не выше уровня шахты × UpgradesPerLevel
    public const int UpgradesPerLevel = 3;

    // Клики приходят пачками раз в секунду; больше MaxClicksPerSecond в секунду не засчитываем
    public const int MaxClicksPerSecond = 10;
    public const int MaxClicksPerBatch = 50;

    // Цена улучшения с уровня L: BaseUpgradeGold × CostGrowth^(L-1). Уровень шахты открывает UpgradesPerLevel
    // улучшений каждого вида, поэтому стоит как они: ×LevelCostFactor и рост на UpgradesPerLevel шагов за уровень
    public const double BaseUpgradeGold = 100;
    public const double BaseUpgradeCrystals = 4;
    public const double CostGrowth = 1.22;
    public const double CrystalGrowth = 1.15;
    public const double LevelCostFactor = 3;

    // Добыча дворфом: база × SpeedGrowth^(скорость-1) × YieldGrowth^(добыча-1)
    public const double SpeedGrowth = 1.09;
    public const double YieldGrowth = 1.07;

    // Клик приносит столько секунд работы одного дворфа — дробно, без округления вверх: доля копится в Mine.DigProgress.
    // Раньше клик округлялся минимум до 1 — кристальная шахта (6 в час) давала кликами в 60 раз больше дворфа
    public const double ClickSeconds = 45;

    // Сила кирки: общий на все шахты запас кликов, без улучшений полностью восстанавливается за час.
    // Кто кликает без перерыва, получает примерно DigEnergyPerHour × ClickSeconds / 3600 ≈ 2.5 дворфа к добыче:
    // на старте ~500 золота или ~15 камней в час. Новая кристальная шахта (12 кристаллов до дворфа) — за один запас
    public const int DigEnergyMax = 200;
    public const double DigEnergyPerHour = 200;

    // Улучшения кирки (у игрока, общие на все шахты), PickaxeMaxLevel уровней каждое:
    //   ёмкость — +EnergyPerCapacityLevel кликов запаса (200 → 650), восстановление — +EnergyPerRecoveryLevel в час (200 → 560),
    //   сила — +StrengthPerLevel к добыче клика (45 → ~86 секунд работы дворфа).
    // Цена — золото и половина опыта, растёт в PickaxeCostGrowth раз за уровень; сила вдвое дороже — она умножает всё.
    // Все три до потолка — ~170 тыс. золота: заметная трата середины игры, но окупается кликами
    public const int PickaxeMaxLevel = 10;
    public const int EnergyPerCapacityLevel = 50;
    public const double EnergyPerRecoveryLevel = 40;
    public const double StrengthPerLevel = 0.1;
    public const double PickaxeBaseGold = 500;
    public const double PickaxeCostGrowth = 1.6;
    public const double PickaxeExperienceShare = 0.5;

    public static int EnergyMax(int capacityLevel) => DigEnergyMax + EnergyPerCapacityLevel * (ClampPickaxe(capacityLevel) - 1);

    public static double EnergyPerHour(int recoveryLevel) => DigEnergyPerHour + EnergyPerRecoveryLevel * (ClampPickaxe(recoveryLevel) - 1);

    public static double StrengthFactor(int strengthLevel) => 1 + StrengthPerLevel * (ClampPickaxe(strengthLevel) - 1);

    private static int ClampPickaxe(int level) => Math.Clamp(level, 1, PickaxeMaxLevel);

    // Цена следующего уровня улучшения кирки; null — уровень максимальный
    public static MineCost? PickaxeCost(PickaxeUpgrade upgrade, int level)
    {
        if (level >= PickaxeMaxLevel)
            return null;
        double gold = PickaxeBaseGold * (upgrade == PickaxeUpgrade.Strength ? 2 : 1) * Math.Pow(PickaxeCostGrowth, ClampPickaxe(level) - 1);
        return new MineCost { Gold = Round(gold), Experience = Round(gold * PickaxeExperienceShare) };
    }

    // Сила кирки на момент now: копится с DigEnergyAt до потолка; null — ни разу не тратилась, полная
    public static double DigEnergyAt(double stored, DateTime? at, DateTime now, int max = DigEnergyMax, double perHour = DigEnergyPerHour) =>
        at is { } since
            ? Math.Min(max, stored + Math.Max(0, (now - since).TotalHours) * perHour)
            : max;

    // То же по уровням кирки из кошелька
    public static double DigEnergyOf(MiningWallet wallet, DateTime now) => DigEnergyAt(
        wallet.DigEnergy, wallet.DigEnergyAt, now, EnergyMax(wallet.PickaxeCapacityLevel), EnergyPerHour(wallet.PickaxeRecoveryLevel));

    // Склад вмещает StorageBaseHours + уровень склада часов работы шахты
    public const int StorageBaseHours = 1;

    // Вторая и следующие шахты стоят BuildGold × (число шахт)²
    public const int BuildGold = 200;

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public static GemType? GemOf(MineType type) => type switch
    {
        MineType.Ruby => GemType.Ruby,
        MineType.Sapphire => GemType.Sapphire,
        MineType.Topaz => GemType.Topaz,
        MineType.Emerald => GemType.Emerald,
        MineType.Amethyst => GemType.Amethyst,
        MineType.Diamond => GemType.Diamond,
        MineType.Garnet => GemType.Garnet,
        _ => null
    };

    /* ===== Формулы ===== */

    // Сколько приносит клик — дробное: золотая шахта на старте 2.5, кристальная 0.075 камня; сила кирки умножает
    public static double PerClick(MineType type, int speedLevel, int yieldLevel, int strengthLevel = 1) =>
        RatePerHour(type, speedLevel, yieldLevel) * ClickSeconds * StrengthFactor(strengthLevel) / 3600;

    public static double RatePerHour(MineType type, int speedLevel, int yieldLevel)
    {
        double baseRate = type switch
        {
            MineType.Gold => 200,
            MineType.Experience => 100,
            _ => 6
        };
        return baseRate * Math.Pow(SpeedGrowth, speedLevel - 1) * Math.Pow(YieldGrowth, yieldLevel - 1);
    }

    public static int Capacity(MineType type, int speedLevel, int yieldLevel, int storageLevel) =>
        (int)Math.Round(RatePerHour(type, speedLevel, yieldLevel) * (StorageBaseHours + storageLevel), MidpointRounding.AwayFromZero);

    public static int MaxUpgradeLevel(Mine mine, MineUpgrade upgrade) =>
        upgrade == MineUpgrade.Level ? MaxLevel : mine.Level * UpgradesPerLevel;

    public static int LevelOf(Mine mine, MineUpgrade upgrade) => upgrade switch
    {
        MineUpgrade.Level => mine.Level,
        MineUpgrade.Speed => mine.SpeedLevel,
        MineUpgrade.Yield => mine.YieldLevel,
        _ => mine.StorageLevel
    };

    // Шахта золота просит только золото, шахта опыта — ещё и опыт, шахта кристаллов — свои кристаллы
    public static MineCost UpgradeCost(MineType type, MineUpgrade upgrade, int level)
    {
        bool isLevel = upgrade == MineUpgrade.Level;
        int steps = (isLevel ? UpgradesPerLevel : 1) * (level - 1);
        double factor = isLevel ? LevelCostFactor : 1;
        int gold = Round(BaseUpgradeGold * factor * Math.Pow(CostGrowth, steps));
        var gem = GemOf(type);
        return type switch
        {
            MineType.Gold => new MineCost { Gold = Round(gold * 1.5) },
            MineType.Experience => new MineCost { Gold = gold, Experience = Round(gold / 2.0) },
            _ => new MineCost { Gold = gold, Gem = gem, Crystals = Round(BaseUpgradeCrystals * factor * Math.Pow(CrystalGrowth, steps)) }
        };
    }

    public static MineCost BuildCost(int minesBuilt) => new() { Gold = BuildGold * minesBuilt * minesBuilt };

    // Добыча и склад всей шахты: каждый дворф копает с её скоростью, склад растёт вместе с ними
    public static double MineRate(Mine mine) => RatePerHour(mine.Type, mine.SpeedLevel, mine.YieldLevel) * Crew(mine);

    public static int MineCapacity(Mine mine) => Capacity(mine.Type, mine.SpeedLevel, mine.YieldLevel, mine.StorageLevel) * Crew(mine);

    // Добыча дворфов на момент now: копится с UpdatedAt, пока не упрётся в склад.
    // Склад, накопленный большим составом, при уходе дворфа не урезается — просто не растёт выше нового
    public static double StoredAt(Mine mine, DateTime now)
    {
        if (mine.Dwarves.Count == 0 || now <= mine.UpdatedAt)
            return mine.Stored;

        double mined = MineRate(mine) * (now - mine.UpdatedAt).TotalHours;
        return Math.Max(mine.Stored, Math.Min(MineCapacity(mine), mine.Stored + mined));
    }

    /* ===== Действия ===== */

    public async Task<MiningBoard> GetBoardAsync(Guid playerId)
    {
        var now = Now;
        var mines = await mining.GetMinesAsync(playerId);
        var wallet = await WalletAsync(playerId, now);
        return new MiningBoard
        {
            ServerTime = now,
            PlotCount = PlotCount,
            DwarfUnlockLevel = DwarfUnlockLevel,
            MaxClicksPerSecond = MaxClicksPerSecond,
            DigEnergyMax = EnergyMax(wallet.PickaxeCapacityLevel),
            DigEnergyPerHour = EnergyPerHour(wallet.PickaxeRecoveryLevel),
            Pickaxe = DescribePickaxe(wallet),
            BuildCost = BuildCost(mines.Count),
            Mines = mines.Select(m => ToInfo(m, now, wallet.PickaxeStrengthLevel)).ToList(),
            Wallet = wallet
        };
    }

    // Кошелёк с силой кирки, досчитанной на now: дальше фронт сам растит её по DigEnergyPerHour
    private async Task<MiningWallet> WalletAsync(Guid playerId, DateTime now)
    {
        var wallet = await mining.GetWalletAsync(playerId);
        wallet.DigEnergy = DigEnergyOf(wallet, now);
        wallet.DigEnergyAt = now;
        return wallet;
    }

    // Улучшение кирки: золото и опыт. Сила кирки сначала фиксируется по старым уровням — запас не теряется
    // и не появляется из воздуха; новый потолок и скорость действуют с этой минуты. В ответ — вся доска:
    // меняются и запас, и добыча клика во всех шахтах
    public async Task<MiningBoard> UpgradePickaxeAsync(Guid playerId, PickaxeUpgrade upgrade)
    {
        if (!Enum.IsDefined(upgrade))
            throw new MiningException(MiningError.MineNotFound, "Unknown pickaxe upgrade");

        var now = Now;
        var raw = await mining.GetWalletAsync(playerId);
        int level = PickaxeLevelOf(raw, upgrade);
        var cost = PickaxeCost(upgrade, level)
            ?? throw new MiningException(MiningError.MaxLevel, $"The pickaxe {upgrade} is already at level {PickaxeMaxLevel}");

        var settle = new DigEnergySpend(raw.DigEnergyAt, DigEnergyOf(raw, now), now);
        if (!await mining.UpgradePickaxeAsync(playerId, upgrade, level, cost, settle))
        {
            // Не хватило ресурсов — назовём каких; иначе кирку только что поменяли параллельным запросом
            var fresh = await mining.GetWalletAsync(playerId);
            if (PickaxeLevelOf(fresh, upgrade) == level && fresh.DigEnergyAt == raw.DigEnergyAt)
                throw await NotEnoughAsync(playerId, cost);
            throw new MiningException(MiningError.Changed, "The pickaxe has just changed, try again");
        }

        return await GetBoardAsync(playerId);
    }

    public static int PickaxeLevelOf(MiningWallet wallet, PickaxeUpgrade upgrade) => upgrade switch
    {
        PickaxeUpgrade.Capacity => wallet.PickaxeCapacityLevel,
        PickaxeUpgrade.Recovery => wallet.PickaxeRecoveryLevel,
        _ => wallet.PickaxeStrengthLevel
    };

    private static List<PickaxeUpgradeInfo> DescribePickaxe(MiningWallet wallet) => Enum.GetValues<PickaxeUpgrade>()
        .Select(upgrade =>
        {
            int level = PickaxeLevelOf(wallet, upgrade);
            return new PickaxeUpgradeInfo
            {
                Upgrade = upgrade,
                Level = level,
                MaxLevel = PickaxeMaxLevel,
                Cost = PickaxeCost(upgrade, level),
                Value = PickaxeEffect(upgrade, level),
                Next = level < PickaxeMaxLevel ? PickaxeEffect(upgrade, level + 1) : null
            };
        })
        .ToList();

    // Что даёт улучшение кирки на уровне level: запас кликов, кликов в час или процент добычи клика
    private static double PickaxeEffect(PickaxeUpgrade upgrade, int level) => upgrade switch
    {
        PickaxeUpgrade.Capacity => EnergyMax(level),
        PickaxeUpgrade.Recovery => EnergyPerHour(level),
        _ => Math.Round(StrengthFactor(level) * 100)
    };

    public async Task<MiningActionResult> BuildAsync(Guid playerId, int plot, MineType type)
    {
        if (plot < 1 || plot > PlotCount)
            throw new MiningException(MiningError.PlotNotFound, $"Plot must be from 1 to {PlotCount}");
        if (!Enum.IsDefined(type))
            throw new MiningException(MiningError.MineNotFound, "Unknown mine type");

        var now = Now;
        var mines = await mining.GetMinesAsync(playerId);
        if (mines.Any(m => m.Plot == plot))
            throw new MiningException(MiningError.PlotTaken, "This plot already has a mine");

        var cost = BuildCost(mines.Count);
        var mine = new Mine { Id = Guid.NewGuid(), PlayerId = playerId, Plot = plot, Type = type, UpdatedAt = now };

        switch (await mining.BuildAsync(mine, cost.AsDebit()))
        {
            case MiningSaveOutcome.Changed:
                throw new MiningException(MiningError.PlotTaken, "This plot already has a mine");
            case MiningSaveOutcome.NotEnoughResources:
                throw await NotEnoughAsync(playerId, cost);
        }

        return await ResultAsync(playerId, mine, now);
    }

    public async Task<MiningDigResult> DigAsync(Guid playerId, Guid mineId, int clicks)
    {
        var now = Now;
        var mine = await GetMineAsync(playerId, mineId);

        // Не больше MaxClicksPerSecond за время с прошлой пачки: так автокликер не обгонит руку.
        // И не больше, чем осталось силы кирки
        var raw = await mining.GetWalletAsync(playerId);
        double energy = DigEnergyOf(raw, now);
        double seconds = mine.LastDigAt is { } last ? (now - last).TotalSeconds : MaxClicksPerBatch;
        int allowed = Math.Min(Math.Min(clicks, MaxClicksPerBatch), (int)Math.Ceiling(seconds * MaxClicksPerSecond));
        allowed = Math.Min(allowed, (int)Math.Floor(energy));
        if (allowed <= 0)
            return new MiningDigResult { Mine = ToInfo(mine, now, raw.PickaxeStrengthLevel), Wallet = await WalletAsync(playerId, now) };

        // Клик приносит дробную долю: целые штуки зачисляются сразу, остаток копится в шахте.
        // Погрешность double не должна съедать штуку на ровной границе (0.999999…)
        double progress = mine.DigProgress + allowed * PerClick(mine.Type, mine.SpeedLevel, mine.YieldLevel, raw.PickaxeStrengthLevel);
        int mined = (int)Math.Floor(progress + 1e-9);
        int version = mine.Version;
        mine.DigProgress = Math.Max(0, progress - mined);
        mine.LastDigAt = now;

        var change = MiningChange.Credit(mine.Type, mined) with
        {
            Energy = new DigEnergySpend(raw.DigEnergyAt, energy - allowed, now)
        };
        if (await mining.SaveAsync(mine, version, change) != MiningSaveOutcome.Saved)
            throw new MiningException(MiningError.Changed, "Mine has just changed, try again");
        await activity.RecordAsync(playerId, ActivityKind.MineDig, allowed);

        var result = await ResultAsync(playerId, mine, now);
        return new MiningDigResult { Mined = mined, Clicks = allowed, Mine = result.Mine, Wallet = result.Wallet };
    }

    public async Task<MiningActionResult> UpgradeAsync(Guid playerId, Guid mineId, MineUpgrade upgrade)
    {
        var now = Now;
        var mine = await GetMineAsync(playerId, mineId);

        int level = LevelOf(mine, upgrade);
        if (upgrade == MineUpgrade.Level && level >= MaxLevel)
            throw new MiningException(MiningError.MaxLevel, $"Mine is already at level {MaxLevel}");
        if (level >= MaxUpgradeLevel(mine, upgrade))
            throw new MiningException(MiningError.MineLevelTooLow, "Raise the mine level first");

        var cost = UpgradeCost(mine.Type, upgrade, level);
        int version = mine.Version;
        // Скорость и склад меняются — сначала фиксируем добытое по старым
        Settle(mine, now);
        switch (upgrade)
        {
            case MineUpgrade.Level: mine.Level++; break;
            case MineUpgrade.Speed: mine.SpeedLevel++; break;
            case MineUpgrade.Yield: mine.YieldLevel++; break;
            case MineUpgrade.Storage: mine.StorageLevel++; break;
        }

        await SaveOrThrowAsync(mine, version, cost.AsDebit(), cost);
        return await ResultAsync(playerId, mine, now);
    }

    public async Task<MiningActionResult> AssignDwarfAsync(Guid playerId, Guid mineId, Guid heroId)
    {
        var now = Now;
        var mine = await GetMineAsync(playerId, mineId);
        if (mine.Level < DwarfUnlockLevel)
            throw new MiningException(MiningError.DwarfLocked, $"A dwarf can work in a mine from level {DwarfUnlockLevel}");

        var hero = await heroes.GetByIdAsync(heroId);
        if (hero == null || hero.PlayerId != playerId)
            throw new MiningException(MiningError.HeroNotFound, "Hero not found");
        if (hero.PlaceInSquad != null)
            throw new MiningException(MiningError.HeroInSquad, "Heroes from the squad can't work in a mine");
        if ((await expeditions.GetBusyHeroIdsAsync(playerId)).Contains(heroId))
            throw new MiningException(MiningError.HeroBusy, "Hero is on an expedition");
        if ((await mining.GetHeroIdsAsync(playerId)).Contains(heroId))
            throw new MiningException(MiningError.HeroBusy, "Hero already works in a mine");

        // Первое свободное место из открытых уровнем шахты
        int slots = DwarfSlots(mine.Level);
        var taken = mine.Dwarves.Select(d => d.Slot).ToHashSet();
        int slot = Enumerable.Range(1, slots).FirstOrDefault(s => !taken.Contains(s));
        if (slot == 0)
            throw new MiningException(MiningError.NoFreeSlot, NextSlotLevel(mine.Level) is { } next
                ? $"All {slots} places are taken; the next one opens at mine level {next}"
                : $"All {slots} places are taken");

        // Добытое до прихода дворфа фиксируем по старому составу
        int version = mine.Version;
        Settle(mine, now);
        mine.Dwarves.Add(new MineDwarf { MineId = mine.Id, HeroId = heroId, Slot = slot });

        await SaveOrThrowAsync(mine, version, MiningChange.None, null, MineDwarfChange.Put(heroId, slot));
        return await ResultAsync(playerId, mine, now);
    }

    // heroId == null — шахта с одним дворфом: отзываем его (старый вызов без указания дворфа)
    public async Task<MiningActionResult> RemoveDwarfAsync(Guid playerId, Guid mineId, Guid? heroId = null)
    {
        var now = Now;
        var mine = await GetMineAsync(playerId, mineId);
        var dwarf = heroId is { } id
            ? mine.Dwarves.FirstOrDefault(d => d.HeroId == id)
            : mine.Dwarves.Count == 1 ? mine.Dwarves[0] : null;
        if (dwarf == null)
            throw new MiningException(MiningError.NoDwarf, "There is no such dwarf in this mine");

        // Добытое до ухода дворфа остаётся на складе
        int version = mine.Version;
        Settle(mine, now);
        mine.Dwarves.Remove(dwarf);

        await SaveOrThrowAsync(mine, version, MiningChange.None, null, MineDwarfChange.Take(dwarf.HeroId));
        return await ResultAsync(playerId, mine, now);
    }

    public async Task<MiningCollectResult> CollectAsync(Guid playerId, Guid mineId)
    {
        var now = Now;
        var mine = await GetMineAsync(playerId, mineId);

        int version = mine.Version;
        Settle(mine, now);
        int collected = (int)Math.Floor(mine.Stored);
        if (collected <= 0)
            throw new MiningException(MiningError.NothingToCollect, "Nothing to collect yet");
        mine.Stored -= collected;

        await SaveOrThrowAsync(mine, version, MiningChange.Credit(mine.Type, collected), null);
        await activity.RecordAsync(playerId, ActivityKind.MineCollect);
        var result = await ResultAsync(playerId, mine, now);
        return new MiningCollectResult { Collected = collected, Mine = result.Mine, Wallet = result.Wallet };
    }

    /* ===== Вспомогательное ===== */

    private static void Settle(Mine mine, DateTime now)
    {
        mine.Stored = StoredAt(mine, now);
        mine.UpdatedAt = now;
    }

    private async Task<Mine> GetMineAsync(Guid playerId, Guid mineId) =>
        await mining.GetAsync(playerId, mineId)
            ?? throw new MiningException(MiningError.MineNotFound, "Mine not found");

    private async Task SaveOrThrowAsync(Mine mine, int version, MiningChange change, MineCost? cost, MineDwarfChange? dwarf = null)
    {
        switch (await mining.SaveAsync(mine, version, change, dwarf))
        {
            case MiningSaveOutcome.Changed:
                throw new MiningException(MiningError.Changed, "Mine has just changed, try again");
            case MiningSaveOutcome.HeroTaken:
                throw new MiningException(MiningError.HeroBusy, "Hero already works in another mine");
            case MiningSaveOutcome.NotEnoughResources:
                throw await NotEnoughAsync(mine.PlayerId, cost ?? new MineCost());
        }
    }

    // Ресурсов не хватило — называем, каких именно
    private async Task<MiningException> NotEnoughAsync(Guid playerId, MineCost cost)
    {
        var wallet = await mining.GetWalletAsync(playerId);
        if (wallet.Gold < cost.Gold)
            return new MiningException(MiningError.NotEnoughGold, $"Not enough gold: {wallet.Gold} of {cost.Gold}");
        if (wallet.Experience < cost.Experience)
            return new MiningException(MiningError.NotEnoughExperience, $"Not enough experience: {wallet.Experience} of {cost.Experience}");
        return new MiningException(MiningError.NotEnoughCrystals, "Not enough crystals");
    }

    private async Task<MiningActionResult> ResultAsync(Guid playerId, Mine mine, DateTime now)
    {
        var wallet = await WalletAsync(playerId, now);
        return new MiningActionResult { Mine = ToInfo(mine, now, wallet.PickaxeStrengthLevel), Wallet = wallet };
    }

    // strengthLevel — сила кирки игрока: от неё зависит, сколько приносит клик
    public static MineInfo ToInfo(Mine mine, DateTime now, int strengthLevel = 1) => new()
    {
        Id = mine.Id,
        Plot = mine.Plot,
        Type = mine.Type,
        Gem = GemOf(mine.Type),
        Level = mine.Level,
        MaxLevel = MaxLevel,
        PerClick = PerClick(mine.Type, mine.SpeedLevel, mine.YieldLevel, strengthLevel),
        DigProgress = mine.DigProgress,
        RatePerHour = MineRate(mine),
        RatePerDwarf = RatePerHour(mine.Type, mine.SpeedLevel, mine.YieldLevel),
        Capacity = MineCapacity(mine),
        HeroIds = mine.Dwarves.OrderBy(d => d.Slot).Select(d => d.HeroId).ToList(),
        DwarfSlots = DwarfSlots(mine.Level),
        MaxDwarves = MaxDwarves,
        NextSlotLevel = NextSlotLevel(mine.Level),
        // Склад посчитан на now: дальше фронт сам досчитывает его по RatePerHour
        Stored = StoredAt(mine, now),
        StoredAt = now,
        Upgrades = Enum.GetValues<MineUpgrade>().Select(upgrade => DescribeUpgrade(mine, upgrade)).ToList()
    };

    private static MineUpgradeInfo DescribeUpgrade(Mine mine, MineUpgrade upgrade)
    {
        int level = LevelOf(mine, upgrade);
        int max = MaxUpgradeLevel(mine, upgrade);
        return new MineUpgradeInfo
        {
            Upgrade = upgrade,
            Level = level,
            MaxLevel = max,
            Cost = level < max ? UpgradeCost(mine.Type, upgrade, level) : null,
            Value = EffectOf(mine, upgrade, level),
            Next = level < max ? EffectOf(mine, upgrade, level + 1) : null
        };
    }

    // Что даёт улучшение на уровне level — для всей шахты с её дворфами; для уровня шахты — сам уровень
    private static double EffectOf(Mine mine, MineUpgrade upgrade, int level) => upgrade switch
    {
        MineUpgrade.Speed => RatePerHour(mine.Type, level, mine.YieldLevel) * Crew(mine),
        MineUpgrade.Yield => RatePerHour(mine.Type, mine.SpeedLevel, level) * Crew(mine),
        MineUpgrade.Storage => Capacity(mine.Type, mine.SpeedLevel, mine.YieldLevel, level) * Crew(mine),
        _ => level
    };

    private static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
}

public enum MiningSaveOutcome
{
    Saved,
    // Шахту изменили параллельно (или место уже занято при постройке)
    Changed,
    NotEnoughResources,
    // Дворфа только что поставили в другую шахту
    HeroTaken
}

// Изменение ресурсов игрока: плюс — начислить, минус — списать
// Energy — потраченная сила кирки: записывается, только если с прошлого чтения её никто не тратил (ExpectedAt)
public record MiningChange(long Gold, long Experience, GemType? Gem, int Crystals, DigEnergySpend? Energy = null)
{
    public static readonly MiningChange None = new(0, 0, null, 0);

    public static MiningChange Credit(MineType type, int amount) => type switch
    {
        MineType.Gold => new(amount, 0, null, 0),
        MineType.Experience => new(0, amount, null, 0),
        _ => new(0, 0, MiningService.GemOf(type), amount)
    };
}

// Сила кирки после кликов: Value на момент At. ExpectedAt — DigEnergyAt, от которого её считали
public record DigEnergySpend(DateTime? ExpectedAt, double Value, DateTime At);
