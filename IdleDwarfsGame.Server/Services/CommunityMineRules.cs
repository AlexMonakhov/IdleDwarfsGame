// Правила ивента «Общие шахты» (WorldEvents). Месторождения появляются по расписанию: два — в начале ивента,
// дальше по одному каждые SpawnInterval, но открытых одновременно не больше MaxOpen (следующее ждёт, пока закроется какое-то).
// Вид идёт по кругу (Rotation), размер вида — SizeOf. Руды — OrePerActivePlayer × размер на каждого игрока, активного
// за последние сутки: одному игроку медное месторождение — на несколько часов, сотне — тоже не на пять минут.
//
// Дворф копает DwarfRate руды в час: больше у высокого ранга и прокачки, рудокоп — ×MinerBonus (профессия вне боя).
// Игрок ставит на месторождение до MaxDwarvesPerPlayer дворфов. Медь — только для дворфов до CopperMaxRank ранга:
// у новичков своё место, где сильные отряды их не вытеснят.
// Улучшение скорости — для всех: +SpeedPerLevel к общей добыче за уровень, платит улучшивший золотом
// (цена одна для всех и зависит от вида месторождения), а в его вклад идёт UpgradeShare руды месторождения.
// Так улучшать выгодно себе, а не только соседям. Один игрок — не больше MaxUpgradesPerPlayer уровней месторождения.
//
// Награда — самородки (валюта ивента) за закрытое месторождение каждому, чей вклад не меньше MinShare:
// RewardBase × размер плюс доля RewardPool × размер по вкладу; невыработанное к концу ивента — пропорционально выработанному.
// При десяти активных игроках за ивент закрывается ~12 месторождений — ~2 000–2 500 самородков игроку, с заданиями — герой ивента
public static class CommunityMineRules
{
    public static readonly TimeSpan SpawnInterval = TimeSpan.FromHours(3);
    public const int StartDeposits = 2;
    public const int MaxOpen = 4;

    public static readonly DepositKind[] Rotation =
        [DepositKind.Copper, DepositKind.Iron, DepositKind.Silver, DepositKind.Copper, DepositKind.Gold, DepositKind.Iron, DepositKind.Mithril];

    public static DepositKind KindOf(int index) => Rotation[index % Rotation.Length];

    public static int SizeOf(DepositKind kind) => kind switch
    {
        DepositKind.Copper => 1,
        DepositKind.Iron => 2,
        DepositKind.Silver => 2,
        DepositKind.Gold => 4,
        _ => 6
    };

    public const int CopperMaxRank = 2;

    // Когда по расписанию появляется месторождение index (без учёта лимита открытых)
    public static DateTime ScheduledAt(DateTime eventStart, int index) =>
        eventStart + SpawnInterval * Math.Max(0, index - (StartDeposits - 1));

    public const double OrePerActivePlayer = 300;

    public static double OreOf(DepositKind kind, int activePlayers) => OrePerActivePlayer * SizeOf(kind) * Math.Max(1, activePlayers);

    /* ===== Дворфы ===== */

    public const int MaxDwarvesPerPlayer = 5;
    public const double BaseDwarfRate = 10;
    public const double RatePerRank = 0.5;
    public const double RatePerUpgradeLevel = 0.02;
    public const double MinerBonus = 1.5;

    // Руда в час без улучшения месторождения: ранг 1 — 10, рудокоп 4-го ранга на 50-м уровне — ~74
    public static double DwarfRate(Hero hero)
    {
        double rate = BaseDwarfRate
            * (1 + RatePerRank * (Math.Max(1, hero.Level) - 1))
            * (1 + RatePerUpgradeLevel * (Math.Max(1, hero.UpgradeLevel) - 1));
        return hero.Profession == HeroProfession.Miner ? rate * MinerBonus : rate;
    }

    public static bool Allows(DepositKind kind, Hero hero) => kind != DepositKind.Copper || hero.Level <= CopperMaxRank;

    /* ===== Улучшение ===== */

    public const int MaxSpeedLevel = 10;
    public const double SpeedPerLevel = 0.15;
    public const double UpgradeShare = 0.03;
    public const double UpgradeBaseStages = 5;
    public const double UpgradeCostGrowth = 1.5;
    public const int MinUpgradeGold = 100;

    // Один игрок покупает не больше стольких уровней одного месторождения: до максимума его доводят минимум трое,
    // и за улучшения одному достаётся не больше MaxUpgradesPerPlayer × UpgradeShare руды
    public const int MaxUpgradesPerPlayer = 3;

    // Цена улучшения — одна для всех и зависит от месторождения: в золоте уровня кампании, на котором обычно
    // копают такой рудой (под ранг дворфов, CampaignRewards.ChestLevelFor). Медь — новичкам, мифрил — сильным игрокам
    public static int ReferenceStageOf(DepositKind kind) => kind switch
    {
        DepositKind.Copper => 40,
        DepositKind.Iron => 100,
        DepositKind.Silver => 150,
        DepositKind.Gold => 250,
        _ => 375
    };

    private static readonly Dictionary<DepositKind, int> StageGold = Enum.GetValues<DepositKind>().ToDictionary(
        kind => kind,
        kind => CampaignRewards.FixedFor(new CampaignStage
        {
            Id = ReferenceStageOf(kind),
            Type = StageType.Mobs,
            EnemyLevel = CampaignGenerator.EnemyLevelFor(ReferenceStageOf(kind))
        }).Gold);

    public static double SpeedFactor(int level) => 1 + SpeedPerLevel * (Math.Clamp(level, 1, MaxSpeedLevel) - 1);

    // Цена следующего уровня месторождения; null — уровень максимальный
    // 1→2 / все девять уровней: медь 900 / ~67 тыс., железо ~7.3 тыс. / ~550 тыс., серебро ~17 тыс. / ~1.3 млн,
    // золото ~64 тыс. / ~4.8 млн, мифрил ~225 тыс. / ~17 млн
    public static int? UpgradeGold(DepositKind kind, int level)
    {
        if (level >= MaxSpeedLevel)
            return null;
        double stages = UpgradeBaseStages * Math.Pow(UpgradeCostGrowth, Math.Max(1, level) - 1);
        return Math.Max(MinUpgradeGold, (int)Math.Round(StageGold[kind] * stages));
    }

    /* ===== Награда ===== */

    public const double MinShare = 0.01;
    public const int RewardBase = 40;
    public const int RewardPool = 250;

    public const int LeaderboardSize = 10;

    public static double ContributionOf(DepositWorker worker) => worker.Mined + worker.UpgradePoints;

    // Доля игрока — от всей руды месторождения: так вклад за улучшения не размывает долю копавших
    public static double ShareOf(DepositWorker worker, CommunityDeposit deposit) =>
        deposit.OreTotal > 0 ? Math.Clamp(ContributionOf(worker) / deposit.OreTotal, 0, 1) : 0;

    public static int RewardFor(CommunityDeposit deposit, DepositWorker worker)
    {
        double share = ShareOf(worker, deposit);
        if (share < MinShare)
            return 0;
        int size = SizeOf(deposit.Kind);
        double done = deposit.OreTotal > 0 ? Math.Clamp(deposit.OreMined / deposit.OreTotal, 0, 1) : 0;
        return (int)Math.Round((RewardBase * size + RewardPool * size * share) * done);
    }

    /* ===== Добыча ===== */

    // Засчитать добытое с UpdatedAt до min(now, closeAt) по нынешнему составу: руда месторождения и доли игроков.
    // rates — руда в час дворфов каждого игрока без улучшения. Игроки без строки вклада получают её здесь.
    // Возвращает true, если месторождение выработано
    public static bool Settle(CommunityDeposit deposit, List<DepositWorker> workers, IReadOnlyDictionary<Guid, double> rates,
        DateTime now, DateTime closeAt)
    {
        if (deposit.ClosedAt != null || deposit.ExhaustedAt != null)
            return deposit.ExhaustedAt != null;

        var until = now < closeAt ? now : closeAt;
        if (until <= deposit.UpdatedAt)
            return false;

        double hours = (until - deposit.UpdatedAt).TotalHours;
        double speed = SpeedFactor(deposit.SpeedLevel);
        double totalRate = rates.Values.Sum() * speed;
        double remaining = Math.Max(0, deposit.OreTotal - deposit.OreMined);

        if (totalRate <= 0 || remaining <= 0)
        {
            deposit.UpdatedAt = until;
            return remaining <= 0;
        }

        double potential = totalRate * hours;
        double mined = Math.Min(remaining, potential);
        double ratio = mined / potential;
        foreach (var (playerId, rate) in rates)
        {
            if (rate <= 0)
                continue;
            var worker = workers.FirstOrDefault(w => w.PlayerId == playerId);
            if (worker == null)
            {
                worker = new DepositWorker { DepositId = deposit.Id, PlayerId = playerId };
                workers.Add(worker);
            }
            worker.Mined += rate * speed * hours * ratio;
        }

        bool exhausted = potential >= remaining - 1e-9;
        if (exhausted)
        {
            deposit.OreMined = deposit.OreTotal;
            deposit.ExhaustedAt = deposit.UpdatedAt.AddHours(remaining / totalRate);
        }
        else
        {
            deposit.OreMined += mined;
        }
        deposit.UpdatedAt = until;
        return exhausted;
    }

    // Когда месторождение выработается при нынешней скорости; null — никто не копает
    public static DateTime? EtaOf(CommunityDeposit deposit, double baseRate)
    {
        double rate = baseRate * SpeedFactor(deposit.SpeedLevel);
        double remaining = deposit.OreTotal - deposit.OreMined;
        if (deposit.ClosedAt != null || rate <= 0 || remaining <= 0)
            return null;
        return deposit.UpdatedAt.AddHours(remaining / rate);
    }
}
