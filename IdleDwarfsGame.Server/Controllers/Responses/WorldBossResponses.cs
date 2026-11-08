public class WorldBossDamageInfo
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Damage { get; set; }
}

public class WorldBossInfo
{
    public Guid Id { get; set; }
    public int Stage { get; set; }
    // Портрет и статы — той же фабрикой, что и в бою
    public CombatUnitInfo Unit { get; set; } = new();
    public int Level { get; set; }
    public string Background { get; set; } = string.Empty;
    public ArenaInfo? Arena { get; set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public DateTime? DefeatedAt { get; set; }
    public string? KilledBy { get; set; }

    public long MyDamage { get; set; }
    // Сколько клыков игрок получит, если босса убьют сейчас (по его нынешнему урону)
    public int MyKillReward { get; set; }
    public List<WorldBossDamageInfo> Top { get; set; } = new();
}

// Награда за убитого босса, которую игрок ещё не забрал
public class WorldBossPendingReward
{
    public Guid BossId { get; set; }
    public int Stage { get; set; }
    public long Damage { get; set; }
    public bool LastHit { get; set; }
    public int Currency { get; set; }
}

public class WorldBossOverview
{
    public WorldEventInfo Event { get; set; } = new();
    // Бить можно: идёт ивент мировых боссов
    public bool Open { get; set; }
    // null — на этой неделе другой ивент (или боссов ещё не было, а ивент уже кончился)
    public WorldBossInfo? Boss { get; set; }

    public DateTime? NextAttackAt { get; set; }
    public int AttackCooldownMinutes { get; set; }
    public int AttackCurrency { get; set; }
    public int KillBase { get; set; }
    public int SharePool { get; set; }
    public int LastHitBonus { get; set; }

    public List<WorldBossPendingReward> Pending { get; set; } = new();

    // Урон за весь ивент
    public List<WorldBossDamageInfo> Leaderboard { get; set; } = new();
    public long MyEventDamage { get; set; }
    // 0 — игрок ещё не бил боссов в этом ивенте
    public int MyRank { get; set; }

    public DateTime ServerTime { get; set; }
}

public class WorldBossAttackResult
{
    public int Stage { get; set; }
    public int Damage { get; set; }
    public bool Killed { get; set; }
    public int BossHp { get; set; }
    public int MaxHp { get; set; }
    // Клыки за удар
    public int Currency { get; set; }
    public DateTime NextAttackAt { get; set; }
    public string Background { get; set; } = string.Empty;
    public ArenaInfo? Arena { get; set; }
    public CombatResult Combat { get; set; } = new();
}

public class WorldBossClaimResult
{
    public int Currency { get; set; }
    public WorldBossOverview Overview { get; set; } = new();
}
