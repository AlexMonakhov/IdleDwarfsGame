using System.Text.RegularExpressions;

// Коды ошибок уходят на фронт строкой — по ним он показывает переведённый текст (guild.errors.<code>)
public enum GuildError
{
    NotInGuild,
    AlreadyInGuild,
    GuildNotFound,
    InvalidName,
    NameTaken,
    RequestAlreadySent,
    RequestNotFound,
    TooManyRequests,
    GuildFull,
    NotLeader,
    MemberNotFound,
    CannotKickYourself,
    BossDefeated,
    BossAlive,
    OnCooldown,
    RewardAlreadyClaimed,
    NoSquad
}

public class GuildException(GuildError code, string message) : InvalidOperationException(message)
{
    public GuildError Code { get; } = code;
}

// Гильдии между аккаунтами (User), как и друзья. Вступление — по заявке, которую принимает глава.
//
// Боссы гильдии (GuildBosses): HP общее на гильдию и обновляется каждые сутки в 00:00 UTC.
// Каждый участник бьёт босса каждого уровня не чаще раза в GuildBosses.AttackCooldown. Когда босса добили,
// награду может забрать любой участник — но не больше одной награды за босса каждого уровня в сутки на аккаунт,
// чтобы нельзя было собирать награды, переходя из гильдии в гильдию. Откат ударов тоже привязан к аккаунту
public class GuildService(
    IGuildRepository guilds,
    IUserRepository users,
    IPlayerRepository players,
    IHeroesRepository heroes,
    IEntityFactory entityFactory,
    CombatService combatService,
    SessionService sessions,
    TimeProvider time
)
{
    public const int MinNameLength = 3;
    public const int MaxNameLength = 24;
    public const int MaxMembers = 20;
    // Сколько заявок в разные гильдии можно держать одновременно
    public const int MaxOutgoingRequests = 5;
    public const int SearchLimit = 20;

    // Буквы любого алфавита, цифры, пробел, _ и -
    private static readonly Regex NamePattern = new(@"^[\p{L}\p{N}_\- ]+$");
    private static readonly Regex Spaces = new(@"\s+");

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    /* ===== Гильдия ===== */

    public async Task<GuildOverview> GetOverviewAsync(Guid userId)
    {
        var membership = await guilds.GetMembershipAsync(userId);
        var overview = new GuildOverview { ServerTime = Now, MaxOutgoingRequests = MaxOutgoingRequests };

        if (membership == null)
        {
            overview.Outgoing = (await guilds.GetOutgoingRequestsAsync(userId))
                .Select(r => new GuildOutgoingRequestInfo { GuildId = r.GuildId, GuildName = r.GuildName, CreatedAt = AsUtc(r.CreatedAt) })
                .ToList();
            return overview;
        }

        var members = await guilds.GetMembersAsync(membership.GuildId);
        var presence = await sessions.GetPresenceAsync(members.Select(m => m.UserId).ToList());
        var incoming = membership.Role == GuildRole.Leader
            ? await guilds.GetIncomingRequestsAsync(membership.GuildId)
            : new List<GuildRequestEntry>();

        overview.Guild = new GuildInfo
        {
            Id = membership.GuildId,
            Name = membership.Guild.Name,
            CreatedAt = AsUtc(membership.Guild.CreatedAt),
            MyRole = membership.Role,
            MaxMembers = MaxMembers,
            Members = members
                .Select(m =>
                {
                    var seen = presence.GetValueOrDefault(m.UserId);
                    return new GuildMemberInfo
                    {
                        UserId = m.UserId,
                        Username = m.Username,
                        Role = m.Role,
                        JoinedAt = AsUtc(m.JoinedAt),
                        IsMe = m.UserId == userId,
                        IsOnline = seen?.IsOnline ?? false,
                        LastSeenAt = seen?.LastSeenAt
                    };
                })
                .OrderByDescending(m => m.Role == GuildRole.Leader)
                .ThenByDescending(m => m.IsOnline)
                .ThenBy(m => m.Username, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Incoming = incoming
                .Select(r => new GuildJoinRequestInfo { UserId = r.UserId, Username = r.Username, CreatedAt = AsUtc(r.CreatedAt) })
                .ToList()
        };
        return overview;
    }

    public async Task<List<GuildSearchResult>> SearchAsync(Guid userId, string? query)
    {
        var found = await guilds.SearchAsync(UserRepository.Normalize(query ?? string.Empty), SearchLimit);
        var requested = (await guilds.GetOutgoingRequestsAsync(userId)).Select(r => r.GuildId).ToHashSet();

        return found
            .Select(g => new GuildSearchResult
            {
                GuildId = g.GuildId,
                Name = g.Name,
                LeaderName = g.LeaderName,
                MemberCount = g.MemberCount,
                MaxMembers = MaxMembers,
                Requested = requested.Contains(g.GuildId)
            })
            .ToList();
    }

    public async Task<GuildOverview> CreateAsync(Guid userId, string name)
    {
        name = Spaces.Replace(name.Trim(), " ");
        if (name.Length is < MinNameLength or > MaxNameLength || !NamePattern.IsMatch(name))
            throw new GuildException(GuildError.InvalidName, $"Guild name must be {MinNameLength}-{MaxNameLength} letters, digits, spaces, _ or -");

        if (await guilds.GetMembershipAsync(userId) != null)
            throw new GuildException(GuildError.AlreadyInGuild, "You are already in a guild");

        var normalized = UserRepository.Normalize(name);
        if (await guilds.NameExistsAsync(normalized))
            throw new GuildException(GuildError.NameTaken, "This guild name is taken");

        var now = Now;
        var guild = new Guild { Id = Guid.NewGuid(), Name = name, NormalizedName = normalized, CreatedAt = now };
        var leader = new GuildMember { UserId = userId, GuildId = guild.Id, Role = GuildRole.Leader, JoinedAt = now };

        if (!await guilds.CreateAsync(guild, leader))
        {
            // Параллельный запрос успел раньше: либо игрок уже в гильдии, либо имя заняли
            if (await guilds.GetMembershipAsync(userId) != null)
                throw new GuildException(GuildError.AlreadyInGuild, "You are already in a guild");
            throw new GuildException(GuildError.NameTaken, "This guild name is taken");
        }

        return await GetOverviewAsync(userId);
    }

    public async Task SendRequestAsync(Guid userId, Guid guildId)
    {
        if (await guilds.GetMembershipAsync(userId) != null)
            throw new GuildException(GuildError.AlreadyInGuild, "You are already in a guild");
        if (await guilds.GetGuildAsync(guildId) == null)
            throw new GuildException(GuildError.GuildNotFound, "Guild not found");
        if (await guilds.RequestExistsAsync(userId, guildId))
            throw new GuildException(GuildError.RequestAlreadySent, "Request is already sent");
        if (await guilds.CountMembersAsync(guildId) >= MaxMembers)
            throw new GuildException(GuildError.GuildFull, "The guild is full");
        if (await guilds.CountOutgoingRequestsAsync(userId) >= MaxOutgoingRequests)
            throw new GuildException(GuildError.TooManyRequests, $"You can't have more than {MaxOutgoingRequests} pending requests");

        var request = new GuildJoinRequest { UserId = userId, GuildId = guildId, CreatedAt = Now };
        if (!await guilds.AddRequestAsync(request))
            throw new GuildException(GuildError.RequestAlreadySent, "Request is already sent");
    }

    public async Task CancelRequestAsync(Guid userId, Guid guildId)
    {
        if (!await guilds.DeleteRequestAsync(userId, guildId))
            throw new GuildException(GuildError.RequestNotFound, "Request not found");
    }

    // Глава leaderId принимает заявку игрока userId
    public async Task AcceptRequestAsync(Guid leaderId, Guid userId)
    {
        var guildId = await RequireLeaderAsync(leaderId);
        if (!await guilds.RequestExistsAsync(userId, guildId))
            throw new GuildException(GuildError.RequestNotFound, "Request not found");
        if (await guilds.CountMembersAsync(guildId) >= MaxMembers)
            throw new GuildException(GuildError.GuildFull, "The guild is full");

        switch (await guilds.AcceptRequestAsync(userId, guildId, Now))
        {
            case GuildAcceptOutcome.RequestNotFound:
                throw new GuildException(GuildError.RequestNotFound, "Request not found");
            case GuildAcceptOutcome.AlreadyInGuild:
                throw new GuildException(GuildError.AlreadyInGuild, "This player has already joined another guild");
        }
    }

    public async Task DeclineRequestAsync(Guid leaderId, Guid userId)
    {
        var guildId = await RequireLeaderAsync(leaderId);
        if (!await guilds.DeleteRequestAsync(userId, guildId))
            throw new GuildException(GuildError.RequestNotFound, "Request not found");
    }

    public async Task KickAsync(Guid leaderId, Guid userId)
    {
        var guildId = await RequireLeaderAsync(leaderId);
        if (userId == leaderId)
            throw new GuildException(GuildError.CannotKickYourself, "You can't kick yourself, leave the guild instead");
        if (!await guilds.RemoveMemberAsync(guildId, userId))
            throw new GuildException(GuildError.MemberNotFound, "This player is not in your guild");
    }

    public async Task LeaveAsync(Guid userId)
    {
        if (await guilds.LeaveAsync(userId) == GuildLeaveOutcome.NotInGuild)
            throw new GuildException(GuildError.NotInGuild, "You are not in a guild");
    }

    /* ===== Боссы ===== */

    public async Task<GuildBossesOverview> GetBossesAsync(Guid userId)
    {
        var now = Now;
        var day = GuildBosses.DayOf(now);
        var membership = await RequireMembershipAsync(userId);

        var bosses = await EnsureBossesAsync(membership.GuildId, day);
        var cooldowns = await guilds.GetCooldownsAsync(userId);
        var claimed = await guilds.GetClaimedTiersAsync(userId, day);
        var damage = await guilds.GetDamageAsync(bosses.Select(b => b.Id).ToList());
        var templates = (await heroes.GetByPlayerAsync(HeroSeedData.EnemyPlayerId)).ToDictionary(h => h.Id);

        return new GuildBossesOverview
        {
            ServerTime = now,
            ResetAt = day + GuildBosses.Lifetime,
            AttackCooldownMinutes = (int)GuildBosses.AttackCooldown.TotalMinutes,
            Bosses = GuildBosses.All
                .Select(definition =>
                {
                    var boss = bosses.First(b => b.Tier == definition.Tier);
                    var bossDamage = damage
                        .Where(d => d.BossId == boss.Id)
                        .OrderByDescending(d => d.Damage)
                        .ToList();
                    var readyAt = cooldowns.TryGetValue(definition.Tier, out var ready) && ready > now ? AsUtc(ready) : (DateTime?)null;

                    return new GuildBossInfo
                    {
                        Tier = definition.Tier,
                        Unit = BossUnit(templates[definition.TemplateHeroId], definition, boss.CurrentHp),
                        Level = definition.Level,
                        Background = definition.Background,
                        MaxHp = boss.MaxHp,
                        CurrentHp = boss.CurrentHp,
                        DefeatedAt = boss.DefeatedAt is { } defeatedAt ? AsUtc(defeatedAt) : null,
                        KilledBy = bossDamage.FirstOrDefault(d => d.UserId == boss.KilledByUserId)?.Username,
                        NextAttackAt = readyAt,
                        RewardClaimed = claimed.Contains(definition.Tier),
                        Reward = RewardInfo.From(GuildBosses.RewardFor(definition.Tier, Guid.Empty, now)),
                        MyDamage = bossDamage.Where(d => d.UserId == userId).Sum(d => d.Damage),
                        Damage = bossDamage
                            .Select(d => new GuildBossDamageInfo { UserId = d.UserId, Username = d.Username, Damage = d.Damage })
                            .ToList()
                    };
                })
                .ToList()
        };
    }

    public async Task<GuildBossAttackResult> AttackAsync(Guid userId, GuildBossTier tier)
    {
        var now = Now;
        var membership = await RequireMembershipAsync(userId);
        var definition = GuildBosses.Get(tier);

        var boss = (await EnsureBossesAsync(membership.GuildId, GuildBosses.DayOf(now))).First(b => b.Tier == tier);
        if (boss.DefeatedAt != null || boss.CurrentHp <= 0)
            throw new GuildException(GuildError.BossDefeated, "The boss is already defeated");

        var user = await users.GetByIdAsync(userId)
            ?? throw new GuildException(GuildError.NotInGuild, "Player not found");
        // Без отряда бой не начнётся — проверяем до отката, чтобы не сжечь удар впустую
        if ((await heroes.GetSquadAsync(user.PlayerId)).Count == 0)
            throw new GuildException(GuildError.NoSquad, "Your squad is empty");
        var template = await heroes.GetByIdAsync(definition.TemplateHeroId)
            ?? throw new InvalidOperationException($"Guild boss template {definition.TemplateHeroId} not found");

        var readyAt = now + GuildBosses.AttackCooldown;
        if (await guilds.TryStartCooldownAsync(userId, tier, now, readyAt) is { } blockedUntil)
            throw new GuildException(GuildError.OnCooldown, $"You can attack this boss again at {AsUtc(blockedUntil):HH:mm} UTC");

        var scenario = new GuildBossScenario(entityFactory, template, definition, boss.CurrentHp);
        var combat = await combatService.StartGuildBossFight(user.PlayerId, scenario);
        var result = await guilds.ApplyDamageAsync(boss.Id, userId, boss.CurrentHp - scenario.BossHpLeft, now);

        return new GuildBossAttackResult
        {
            Tier = tier,
            Damage = result.AppliedDamage,
            Killed = result.Killed,
            BossHp = result.CurrentHp,
            MaxHp = boss.MaxHp,
            NextAttackAt = readyAt,
            Background = definition.Background,
            Combat = combat
        };
    }

    public async Task<RewardClaimResult> ClaimBossRewardAsync(Guid userId, GuildBossTier tier)
    {
        var now = Now;
        var day = GuildBosses.DayOf(now);
        var membership = await RequireMembershipAsync(userId);

        var boss = (await EnsureBossesAsync(membership.GuildId, day)).First(b => b.Tier == tier);
        if (boss.DefeatedAt == null)
            throw new GuildException(GuildError.BossAlive, "The boss is not defeated yet");

        var user = await users.GetByIdAsync(userId)
            ?? throw new GuildException(GuildError.NotInGuild, "Player not found");

        var reward = GuildBosses.RewardFor(tier, user.PlayerId, now);
        var claim = new GuildBossRewardClaim
        {
            UserId = userId,
            Tier = tier,
            Day = day,
            GuildId = membership.GuildId,
            BossId = boss.Id,
            RewardId = reward.Id,
            ClaimedAt = now
        };
        if (!await guilds.ClaimBossRewardAsync(claim, reward))
            throw new GuildException(GuildError.RewardAlreadyClaimed, "You have already claimed the reward for this boss today");

        return new RewardClaimResult
        {
            Reward = RewardInfo.From(reward),
            Resources = await players.GetResourcesAsync(user.PlayerId) ?? new PlayerResources()
        };
    }

    private async Task<List<GuildBoss>> EnsureBossesAsync(Guid guildId, DateTime day)
    {
        var fresh = GuildBosses.All
            .Select(d => new GuildBoss
            {
                Id = Guid.NewGuid(),
                GuildId = guildId,
                Tier = d.Tier,
                Day = day,
                MaxHp = d.MaxHp,
                CurrentHp = d.MaxHp
            })
            .ToList();
        return await guilds.GetOrCreateBossesAsync(guildId, day, fresh);
    }

    // Портрет босса собирается той же фабрикой, что и в бою, чтобы статы совпадали
    private CombatUnitInfo BossUnit(Hero template, GuildBossDefinition definition, int currentHp)
    {
        var world = new World();
        new GuildBossScenario(entityFactory, template, definition, currentHp).SpawnBoss(world);
        return CombatSnapshot.Units(world).Single();
    }

    private async Task<GuildMember> RequireMembershipAsync(Guid userId) =>
        await guilds.GetMembershipAsync(userId)
            ?? throw new GuildException(GuildError.NotInGuild, "You are not in a guild");

    private async Task<Guid> RequireLeaderAsync(Guid userId)
    {
        var membership = await RequireMembershipAsync(userId);
        if (membership.Role != GuildRole.Leader)
            throw new GuildException(GuildError.NotLeader, "Only the guild leader can do this");
        return membership.GuildId;
    }

    // SQLite возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
