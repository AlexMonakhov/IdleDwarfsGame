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
// Цифры тестовые: формулы прокачки будут настраиваться отдельно
public class MiningService(
    IMiningRepository mining,
    IHeroesRepository heroes,
    IExpeditionRepository expeditions,
    TimeProvider time)
{
    public const int PlotCount = 6;
    public const int MaxLevel = 10;
    public const int DwarfUnlockLevel = 2;
    // Скорость, добыча и склад прокачиваются не выше уровня шахты × UpgradesPerLevel
    public const int UpgradesPerLevel = 3;

    // Клики приходят пачками раз в секунду; больше MaxClicksPerSecond в секунду не засчитываем
    public const int MaxClicksPerSecond = 15;
    public const int MaxClicksPerBatch = 50;

    // Цена улучшения с уровня L: BaseUpgradeGold × L^CostExponent (уровень шахты — ×LevelCostFactor)
    public const double BaseUpgradeGold = 40;
    public const double BaseUpgradeCrystals = 4;
    public const double CostExponent = 1.4;
    public const double LevelCostFactor = 3;

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

    public static int PerClick(MineType type, int yieldLevel) => type switch
    {
        MineType.Gold => 2 * yieldLevel,
        _ => yieldLevel
    };

    public static double RatePerHour(MineType type, int speedLevel, int yieldLevel)
    {
        double baseRate = type switch
        {
            MineType.Gold => 120,
            MineType.Experience => 60,
            _ => 12
        };
        return baseRate * (1 + 0.25 * (speedLevel - 1)) * (1 + 0.5 * (yieldLevel - 1));
    }

    public static int Capacity(MineType type, int storageLevel) => storageLevel * type switch
    {
        MineType.Gold => 300,
        MineType.Experience => 150,
        _ => 30
    };

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
        double factor = (upgrade == MineUpgrade.Level ? LevelCostFactor : 1) * Math.Pow(level, CostExponent);
        int gold = Round(BaseUpgradeGold * factor);
        var gem = GemOf(type);
        return type switch
        {
            MineType.Gold => new MineCost { Gold = Round(gold * 1.5) },
            MineType.Experience => new MineCost { Gold = gold, Experience = Round(gold / 2.0) },
            _ => new MineCost { Gold = gold, Gem = gem, Crystals = Round(BaseUpgradeCrystals * factor) }
        };
    }

    public static MineCost BuildCost(int minesBuilt) => new() { Gold = BuildGold * minesBuilt * minesBuilt };

    // Добыча дворфа на момент now: копится с UpdatedAt, пока не упрётся в склад
    public static double StoredAt(Mine mine, DateTime now)
    {
        if (mine.HeroId == null || now <= mine.UpdatedAt)
            return mine.Stored;

        double mined = RatePerHour(mine.Type, mine.SpeedLevel, mine.YieldLevel) * (now - mine.UpdatedAt).TotalHours;
        return Math.Max(mine.Stored, Math.Min(Capacity(mine.Type, mine.StorageLevel), mine.Stored + mined));
    }

    /* ===== Действия ===== */

    public async Task<MiningBoard> GetBoardAsync(Guid playerId)
    {
        var now = Now;
        var mines = await mining.GetMinesAsync(playerId);
        return new MiningBoard
        {
            ServerTime = now,
            PlotCount = PlotCount,
            DwarfUnlockLevel = DwarfUnlockLevel,
            MaxClicksPerSecond = MaxClicksPerSecond,
            BuildCost = BuildCost(mines.Count),
            Mines = mines.Select(m => ToInfo(m, now)).ToList(),
            Wallet = await mining.GetWalletAsync(playerId)
        };
    }

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

        // Не больше MaxClicksPerSecond за время с прошлой пачки: так автокликер не обгонит руку
        double seconds = mine.LastDigAt is { } last ? (now - last).TotalSeconds : MaxClicksPerBatch;
        int allowed = Math.Min(Math.Min(clicks, MaxClicksPerBatch), (int)Math.Ceiling(seconds * MaxClicksPerSecond));
        if (allowed <= 0)
            return new MiningDigResult { Mine = ToInfo(mine, now), Wallet = await mining.GetWalletAsync(playerId) };

        int mined = allowed * PerClick(mine.Type, mine.YieldLevel);
        int version = mine.Version;
        mine.LastDigAt = now;

        if (await mining.SaveAsync(mine, version, MiningChange.Credit(mine.Type, mined)) != MiningSaveOutcome.Saved)
            throw new MiningException(MiningError.Changed, "Mine has just changed, try again");

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
        if (mine.HeroId != heroId && (await mining.GetHeroIdsAsync(playerId)).Contains(heroId))
            throw new MiningException(MiningError.HeroBusy, "Hero already works in another mine");

        int version = mine.Version;
        Settle(mine, now);
        mine.HeroId = heroId;

        await SaveOrThrowAsync(mine, version, MiningChange.None, null);
        return await ResultAsync(playerId, mine, now);
    }

    public async Task<MiningActionResult> RemoveDwarfAsync(Guid playerId, Guid mineId)
    {
        var now = Now;
        var mine = await GetMineAsync(playerId, mineId);
        if (mine.HeroId == null)
            throw new MiningException(MiningError.NoDwarf, "There is no dwarf in this mine");

        // Добытое до ухода дворфа остаётся на складе
        int version = mine.Version;
        Settle(mine, now);
        mine.HeroId = null;

        await SaveOrThrowAsync(mine, version, MiningChange.None, null);
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

    private async Task SaveOrThrowAsync(Mine mine, int version, MiningChange change, MineCost? cost)
    {
        switch (await mining.SaveAsync(mine, version, change))
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

    private async Task<MiningActionResult> ResultAsync(Guid playerId, Mine mine, DateTime now) => new()
    {
        Mine = ToInfo(mine, now),
        Wallet = await mining.GetWalletAsync(playerId)
    };

    public static MineInfo ToInfo(Mine mine, DateTime now) => new()
    {
        Id = mine.Id,
        Plot = mine.Plot,
        Type = mine.Type,
        Gem = GemOf(mine.Type),
        Level = mine.Level,
        MaxLevel = MaxLevel,
        PerClick = PerClick(mine.Type, mine.YieldLevel),
        RatePerHour = RatePerHour(mine.Type, mine.SpeedLevel, mine.YieldLevel),
        Capacity = Capacity(mine.Type, mine.StorageLevel),
        HeroId = mine.HeroId,
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

    // Что даёт улучшение на уровне level: для уровня шахты — сам уровень
    private static double EffectOf(Mine mine, MineUpgrade upgrade, int level) => upgrade switch
    {
        MineUpgrade.Speed => RatePerHour(mine.Type, level, mine.YieldLevel),
        MineUpgrade.Yield => PerClick(mine.Type, level),
        MineUpgrade.Storage => Capacity(mine.Type, level),
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
public record MiningChange(long Gold, long Experience, GemType? Gem, int Crystals)
{
    public static readonly MiningChange None = new(0, 0, null, 0);

    public static MiningChange Credit(MineType type, int amount) => type switch
    {
        MineType.Gold => new(amount, 0, null, 0),
        MineType.Experience => new(0, amount, null, 0),
        _ => new(0, 0, MiningService.GemOf(type), amount)
    };
}
