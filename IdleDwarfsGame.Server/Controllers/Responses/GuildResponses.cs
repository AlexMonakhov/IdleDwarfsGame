public class GuildMemberInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public GuildRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    // Это сам игрок, запросивший гильдию
    public bool IsMe { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

// Заявка в гильдию, которую видит глава
public class GuildJoinRequestInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Своя заявка в чужую гильдию
public class GuildOutgoingRequestInfo
{
    public Guid GuildId { get; set; }
    public string GuildName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GuildInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public GuildRole MyRole { get; set; }
    public int MaxMembers { get; set; }
    // Сначала глава, затем те, кто в сети, затем по имени
    public List<GuildMemberInfo> Members { get; set; } = new();
    // Заявки на вступление — только для главы, у остальных пусто
    public List<GuildJoinRequestInfo> Incoming { get; set; } = new();
}

public class GuildOverview
{
    public DateTime ServerTime { get; set; }
    // null — игрок не состоит в гильдии
    public GuildInfo? Guild { get; set; }
    public List<GuildOutgoingRequestInfo> Outgoing { get; set; } = new();
    public int MaxOutgoingRequests { get; set; }
}

public class GuildSearchResult
{
    public Guid GuildId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LeaderName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int MaxMembers { get; set; }
    // Игрок уже подал заявку в эту гильдию
    public bool Requested { get; set; }
}

public class GuildBossDamageInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Damage { get; set; }
}

public class GuildBossInfo
{
    public GuildBossTier Tier { get; set; }
    // Босс как участник боя (статы, умения, мощь) — для портрета
    public CombatUnitInfo Unit { get; set; } = new();
    // Уровень, под который масштабирована атака босса
    public int Level { get; set; }
    // Фон арены боя (ключ как у Arena.Background)
    public string Background { get; set; } = string.Empty;
    // Арена боя целиком: стихия (цвет карточки) и эффект
    public ArenaInfo Arena { get; set; } = new();
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public DateTime? DefeatedAt { get; set; }
    public string? KilledBy { get; set; }
    // Когда игрок снова сможет ударить; null — можно сейчас
    public DateTime? NextAttackAt { get; set; }
    // Награда за убийство уже забрана сегодня (в этой или другой гильдии)
    public bool RewardClaimed { get; set; }
    public RewardInfo Reward { get; set; } = new();
    public int MyDamage { get; set; }
    // Вклад участников, по убыванию урона
    public List<GuildBossDamageInfo> Damage { get; set; } = new();
}

public class GuildBossesOverview
{
    public DateTime ServerTime { get; set; }
    // Когда появятся новые боссы (00:00 UTC)
    public DateTime ResetAt { get; set; }
    public int AttackCooldownMinutes { get; set; }
    public List<GuildBossInfo> Bosses { get; set; } = new();
}

public class GuildBossAttackResult
{
    public GuildBossTier Tier { get; set; }
    // Сколько урона дошло до босса
    public int Damage { get; set; }
    // Этот удар добил босса
    public bool Killed { get; set; }
    public int BossHp { get; set; }
    public int MaxHp { get; set; }
    public DateTime NextAttackAt { get; set; }
    // Фон арены боя (ключ как у Arena.Background)
    public string Background { get; set; } = string.Empty;
    public ArenaInfo Arena { get; set; } = new();
    public CombatResult Combat { get; set; } = new();
}
