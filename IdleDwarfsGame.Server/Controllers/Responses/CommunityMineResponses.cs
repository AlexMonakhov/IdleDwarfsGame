public class DepositDwarfInfo
{
    public Guid HeroId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; }
    public HeroElement Element { get; set; }
    public HeroProfession? Profession { get; set; }
    // Руда в час без улучшения месторождения
    public double Rate { get; set; }
}

public class DepositContributorInfo
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Contribution { get; set; }
    public double Share { get; set; }
}

public class DepositInfo
{
    public Guid Id { get; set; }
    public int Index { get; set; }
    public DepositKind Kind { get; set; }
    public int Size { get; set; }
    // Медь — только для дворфов до этого ранга; null — любые
    public int? MaxRank { get; set; }

    public double OreTotal { get; set; }
    // На момент ServerTime обзора; дальше фронт досчитывает по Rate
    public double OreMined { get; set; }
    // Руда в час со всеми дворфами и улучшением
    public double Rate { get; set; }
    public int SpeedLevel { get; set; }
    public int MaxSpeedLevel { get; set; }
    public double SpeedFactor { get; set; }
    public double NextSpeedFactor { get; set; }

    public int Dwarves { get; set; }
    public int Participants { get; set; }
    public DateTime? EtaAt { get; set; }
    public DateTime SpawnedAt { get; set; }
    public DateTime? ExhaustedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public List<DepositDwarfInfo> MyDwarves { get; set; } = new();
    public double MyContribution { get; set; }
    public double MyShare { get; set; }
    // Сколько самородков игрок получит, если месторождение закроется сейчас с его нынешней долей
    public int MyReward { get; set; }
    // Цена следующего улучшения (одна для всех, зависит от вида месторождения); null — максимум
    public int? UpgradeGold { get; set; }
    // Руда во вклад за улучшение
    public double UpgradePoints { get; set; }
    // Сколько уровней купил игрок и сколько можно одному
    public int MyUpgrades { get; set; }
    public int MaxUpgradesPerPlayer { get; set; }

    public List<DepositContributorInfo> Top { get; set; } = new();
}

public class DepositPendingReward
{
    public Guid DepositId { get; set; }
    public int Index { get; set; }
    public DepositKind Kind { get; set; }
    public double Contribution { get; set; }
    public double Share { get; set; }
    public int Currency { get; set; }
}

public class CommunityMinesOverview
{
    public WorldEventInfo Event { get; set; } = new();
    // Копать можно: идёт ивент общих шахт
    public bool Open { get; set; }
    public List<DepositInfo> Deposits { get; set; } = new();
    // Когда по расписанию появится следующее месторождение; null — больше не будет
    public DateTime? NextSpawnAt { get; set; }
    public int MaxOpen { get; set; }
    public int MaxDwarvesPerPlayer { get; set; }
    public double MinShare { get; set; }

    public List<DepositPendingReward> Pending { get; set; } = new();

    // Вклад за весь ивент
    public List<DepositContributorInfo> Leaderboard { get; set; } = new();
    public double MyContribution { get; set; }
    // 0 — игрок ещё ничего не накопал в этом ивенте
    public int MyRank { get; set; }

    public DateTime ServerTime { get; set; }
    public PlayerResources Resources { get; set; } = new();
}

public class CommunityMinesClaimResult
{
    public int Currency { get; set; }
    public CommunityMinesOverview Overview { get; set; } = new();
}
