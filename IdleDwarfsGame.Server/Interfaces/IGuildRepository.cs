public record GuildMemberEntry(Guid UserId, string Username, GuildRole Role, DateTime JoinedAt);
public record GuildRequestEntry(Guid UserId, string Username, DateTime CreatedAt);
public record GuildOutgoingRequestEntry(Guid GuildId, string GuildName, DateTime CreatedAt);
public record GuildSearchEntry(Guid GuildId, string Name, string LeaderName, int MemberCount);
public record GuildBossDamageEntry(Guid BossId, Guid UserId, string Username, int Damage);

public enum GuildAcceptOutcome { Accepted, RequestNotFound, AlreadyInGuild }
public enum GuildLeaveOutcome { Left, Disbanded, NotInGuild }

// Итог удара: сколько урона дошло до босса (не больше остатка HP) и добил ли его именно этот удар
public record GuildBossDamageResult(int AppliedDamage, int CurrentHp, bool Killed);

public interface IGuildRepository
{
    /// <summary>
    /// The user's membership with the guild loaded, or null if the user is not in a guild.
    /// </summary>
    Task<GuildMember?> GetMembershipAsync(Guid userId);
    Task<Guild?> GetGuildAsync(Guid guildId);
    Task<bool> NameExistsAsync(string normalizedName);
    /// <summary>
    /// Creates the guild with the leader as its first member. Returns false if the name is taken
    /// or the leader is already in a guild; the leader's join requests are removed.
    /// </summary>
    Task<bool> CreateAsync(Guild guild, GuildMember leader);

    Task<List<GuildMemberEntry>> GetMembersAsync(Guid guildId);
    Task<int> CountMembersAsync(Guid guildId);
    /// <summary>
    /// Guilds whose normalized name contains the query (all guilds for an empty query), biggest first.
    /// </summary>
    Task<List<GuildSearchEntry>> SearchAsync(string normalizedQuery, int limit);

    Task<bool> RequestExistsAsync(Guid userId, Guid guildId);
    Task<int> CountOutgoingRequestsAsync(Guid userId);
    Task<List<GuildRequestEntry>> GetIncomingRequestsAsync(Guid guildId);
    Task<List<GuildOutgoingRequestEntry>> GetOutgoingRequestsAsync(Guid userId);
    /// <summary>
    /// Returns false if the same request already exists.
    /// </summary>
    Task<bool> AddRequestAsync(GuildJoinRequest request);
    /// <summary>
    /// Returns false if there was no such request.
    /// </summary>
    Task<bool> DeleteRequestAsync(Guid userId, Guid guildId);
    /// <summary>
    /// Removes the request and makes the user a member in one transaction; the user's other requests are removed too.
    /// </summary>
    Task<GuildAcceptOutcome> AcceptRequestAsync(Guid userId, Guid guildId, DateTime now);

    /// <summary>
    /// Removes a regular member (not the leader). Returns false if there was no such member.
    /// </summary>
    Task<bool> RemoveMemberAsync(Guid guildId, Guid userId);
    /// <summary>
    /// The user leaves the guild. A leader passes the lead to the oldest member; the last member disbands the guild.
    /// </summary>
    Task<GuildLeaveOutcome> LeaveAsync(Guid userId);

    /// <summary>
    /// Bosses of the guild for the day; missing ones are created from <paramref name="fresh"/>.
    /// </summary>
    Task<List<GuildBoss>> GetOrCreateBossesAsync(Guid guildId, DateTime day, IReadOnlyList<GuildBoss> fresh);
    Task<List<GuildBossDamageEntry>> GetDamageAsync(IReadOnlyCollection<Guid> bossIds);
    /// <summary>
    /// Subtracts the damage from the boss HP (not below zero) and records the attack in one transaction.
    /// </summary>
    Task<GuildBossDamageResult> ApplyDamageAsync(Guid bossId, Guid userId, int damage, DateTime now);

    Task<Dictionary<GuildBossTier, DateTime>> GetCooldownsAsync(Guid userId);
    /// <summary>
    /// Starts the cooldown if the user can attack now. Returns null on success, otherwise when the user can attack again.
    /// </summary>
    Task<DateTime?> TryStartCooldownAsync(Guid userId, GuildBossTier tier, DateTime now, DateTime readyAt);

    Task<HashSet<GuildBossTier>> GetClaimedTiersAsync(Guid userId, DateTime day);
    /// <summary>
    /// Records the claim and grants the reward to the player in one transaction.
    /// Returns false if the user already claimed a reward for this tier on this day.
    /// </summary>
    Task<bool> ClaimBossRewardAsync(GuildBossRewardClaim claim, PlayerReward reward);
}
