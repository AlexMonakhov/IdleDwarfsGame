public enum GuildRole { Member, Leader }

// Гильдия — между аккаунтами (User), как и друзья. Глава — участник с ролью Leader, он всегда один
public class Guild
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Уникальность имени без учёта регистра
    public string NormalizedName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Участник гильдии. Ключ — UserId: игрок состоит не больше чем в одной гильдии даже при параллельных запросах
public class GuildMember
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GuildId { get; set; }
    public Guild Guild { get; set; } = null!;

    public GuildRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

// Заявка на вступление. Живёт, пока глава её не примет или не отклонит, либо игрок не отзовёт.
// Ключ (UserId, GuildId): повторно в ту же гильдию не подать; в разные — можно, до GuildService.MaxOutgoingRequests
public class GuildJoinRequest
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GuildId { get; set; }
    public Guild Guild { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
