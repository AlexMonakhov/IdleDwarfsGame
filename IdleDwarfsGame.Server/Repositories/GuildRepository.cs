using Microsoft.EntityFrameworkCore;

public class GuildRepository(GameDbContext context) : IGuildRepository
{
    public async Task<GuildMember?> GetMembershipAsync(Guid userId)
    {
        return await context.GuildMembers
            .AsNoTracking()
            .Include(m => m.Guild)
            .FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public async Task<Guild?> GetGuildAsync(Guid guildId)
    {
        return await context.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
    }

    public async Task<bool> NameExistsAsync(string normalizedName)
    {
        return await context.Guilds.AnyAsync(g => g.NormalizedName == normalizedName);
    }

    public async Task<bool> CreateAsync(Guild guild, GuildMember leader)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        context.Guilds.Add(guild);
        context.GuildMembers.Add(leader);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Имя только что заняли или игрок уже вступил в другую гильдию (уникальное имя, ключ UserId)
            context.ChangeTracker.Clear();
            return false;
        }

        // Состоит в гильдии — заявки в другие больше не нужны
        await context.GuildJoinRequests
            .Where(r => r.UserId == leader.UserId)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
        // Участники и гильдии удаляются через ExecuteDelete, мимо трекера — не держим копии
        context.Entry(guild).State = EntityState.Detached;
        context.Entry(leader).State = EntityState.Detached;
        return true;
    }

    public async Task<List<GuildMemberEntry>> GetMembersAsync(Guid guildId)
    {
        return await context.GuildMembers
            .AsNoTracking()
            .Where(m => m.GuildId == guildId)
            .Select(m => new GuildMemberEntry(m.UserId, m.User.Username, m.Role, m.JoinedAt))
            .ToListAsync();
    }

    public async Task<int> CountMembersAsync(Guid guildId)
    {
        return await context.GuildMembers.CountAsync(m => m.GuildId == guildId);
    }

    public async Task<List<GuildSearchEntry>> SearchAsync(string normalizedQuery, int limit)
    {
        var guilds = context.Guilds.AsNoTracking();
        if (!string.IsNullOrEmpty(normalizedQuery))
            guilds = guilds.Where(g => g.NormalizedName.Contains(normalizedQuery));

        var rows = await guilds
            .Select(g => new
            {
                g.Id,
                g.Name,
                LeaderName = context.GuildMembers
                    .Where(m => m.GuildId == g.Id && m.Role == GuildRole.Leader)
                    .Select(m => m.User.Username)
                    .FirstOrDefault(),
                MemberCount = context.GuildMembers.Count(m => m.GuildId == g.Id)
            })
            .OrderByDescending(g => g.MemberCount)
            .ThenBy(g => g.Name)
            .Take(limit)
            .ToListAsync();

        return rows.Select(g => new GuildSearchEntry(g.Id, g.Name, g.LeaderName ?? string.Empty, g.MemberCount)).ToList();
    }

    public async Task<bool> RequestExistsAsync(Guid userId, Guid guildId)
    {
        return await context.GuildJoinRequests.AnyAsync(r => r.UserId == userId && r.GuildId == guildId);
    }

    public async Task<int> CountOutgoingRequestsAsync(Guid userId)
    {
        return await context.GuildJoinRequests.CountAsync(r => r.UserId == userId);
    }

    public async Task<List<GuildRequestEntry>> GetIncomingRequestsAsync(Guid guildId)
    {
        return await context.GuildJoinRequests
            .AsNoTracking()
            .Where(r => r.GuildId == guildId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GuildRequestEntry(r.UserId, r.User.Username, r.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<GuildOutgoingRequestEntry>> GetOutgoingRequestsAsync(Guid userId)
    {
        return await context.GuildJoinRequests
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GuildOutgoingRequestEntry(r.GuildId, r.Guild.Name, r.CreatedAt))
            .ToListAsync();
    }

    public async Task<bool> AddRequestAsync(GuildJoinRequest request)
    {
        await context.GuildJoinRequests.AddAsync(request);
        try
        {
            await context.SaveChangesAsync();
            // Удаляются заявки через ExecuteDelete, мимо трекера — отслеживаемая копия помешала бы подать заявку снова
            context.Entry(request).State = EntityState.Detached;
            return true;
        }
        catch (DbUpdateException)
        {
            // Такую же заявку только что подал параллельный запрос (ключ UserId + GuildId)
            context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> DeleteRequestAsync(Guid userId, Guid guildId)
    {
        int deleted = await context.GuildJoinRequests
            .Where(r => r.UserId == userId && r.GuildId == guildId)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }

    public async Task<GuildAcceptOutcome> AcceptRequestAsync(Guid userId, Guid guildId, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Удаление заявки и есть проверка: из двух одновременных принятий пройдёт только первое
        int accepted = await context.GuildJoinRequests
            .Where(r => r.UserId == userId && r.GuildId == guildId)
            .ExecuteDeleteAsync();
        if (accepted == 0)
            return GuildAcceptOutcome.RequestNotFound;

        // Игрок успел вступить в другую гильдию — заявка устарела, убираем её
        if (await context.GuildMembers.AnyAsync(m => m.UserId == userId))
        {
            await transaction.CommitAsync();
            return GuildAcceptOutcome.AlreadyInGuild;
        }

        await context.GuildJoinRequests
            .Where(r => r.UserId == userId)
            .ExecuteDeleteAsync();

        var member = new GuildMember { UserId = userId, GuildId = guildId, Role = GuildRole.Member, JoinedAt = now };
        context.GuildMembers.Add(member);
        try
        {
            await context.SaveChangesAsync();
            context.Entry(member).State = EntityState.Detached;
        }
        catch (DbUpdateException)
        {
            // Параллельно вступил в другую гильдию (ключ UserId)
            context.ChangeTracker.Clear();
            return GuildAcceptOutcome.AlreadyInGuild;
        }

        await transaction.CommitAsync();
        return GuildAcceptOutcome.Accepted;
    }

    public async Task<bool> RemoveMemberAsync(Guid guildId, Guid userId)
    {
        int removed = await context.GuildMembers
            .Where(m => m.GuildId == guildId && m.UserId == userId && m.Role != GuildRole.Leader)
            .ExecuteDeleteAsync();
        return removed > 0;
    }

    public async Task<GuildLeaveOutcome> LeaveAsync(Guid userId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var member = await context.GuildMembers.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null)
            return GuildLeaveOutcome.NotInGuild;

        await context.GuildMembers.Where(m => m.UserId == userId).ExecuteDeleteAsync();

        // Главой становится тот, кто дольше всех в гильдии; если никого не осталось — гильдия распускается
        var heirId = await context.GuildMembers
            .Where(m => m.GuildId == member.GuildId)
            .OrderBy(m => m.JoinedAt)
            .Select(m => (Guid?)m.UserId)
            .FirstOrDefaultAsync();

        if (heirId == null)
        {
            // Участники, заявки и боссы удаляются каскадом
            await context.Guilds.Where(g => g.Id == member.GuildId).ExecuteDeleteAsync();
            await transaction.CommitAsync();
            return GuildLeaveOutcome.Disbanded;
        }

        if (member.Role == GuildRole.Leader)
        {
            await context.GuildMembers
                .Where(m => m.UserId == heirId)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.Role, GuildRole.Leader));
        }

        await transaction.CommitAsync();
        return GuildLeaveOutcome.Left;
    }

    public async Task<List<GuildBoss>> GetOrCreateBossesAsync(Guid guildId, DateTime day, IReadOnlyList<GuildBoss> fresh)
    {
        var bosses = await LoadBossesAsync(guildId, day);
        var missing = fresh.Where(f => bosses.All(b => b.Tier != f.Tier)).ToList();
        if (missing.Count == 0)
            return bosses;

        context.GuildBosses.AddRange(missing);
        try
        {
            await context.SaveChangesAsync();
            // HP меняется через ExecuteUpdate — отслеживаемая копия устарела бы
            foreach (var boss in missing)
                context.Entry(boss).State = EntityState.Detached;
        }
        catch (DbUpdateException)
        {
            // Боссов на этот день только что создал параллельный запрос — просто перечитываем
            context.ChangeTracker.Clear();
        }

        return await LoadBossesAsync(guildId, day);
    }

    private async Task<List<GuildBoss>> LoadBossesAsync(Guid guildId, DateTime day)
    {
        return await context.GuildBosses
            .AsNoTracking()
            .Where(b => b.GuildId == guildId && b.Day == day)
            .ToListAsync();
    }

    public async Task<List<GuildBossDamageEntry>> GetDamageAsync(IReadOnlyCollection<Guid> bossIds)
    {
        var rows = await context.GuildBossAttacks
            .AsNoTracking()
            .Where(a => bossIds.Contains(a.BossId))
            .GroupBy(a => new { a.BossId, a.UserId })
            .Select(g => new { g.Key.BossId, g.Key.UserId, Damage = g.Sum(a => a.Damage) })
            .ToListAsync();

        var userIds = rows.Select(r => r.UserId).Distinct().ToList();
        var names = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        return rows
            .Select(r => new GuildBossDamageEntry(r.BossId, r.UserId, names.GetValueOrDefault(r.UserId, string.Empty), r.Damage))
            .ToList();
    }

    public async Task<GuildBossDamageResult> ApplyDamageAsync(Guid bossId, Guid userId, int damage, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Пока шёл бой, босса могли бить другие: урон срезается до того, что у него осталось
        int hp = await context.GuildBosses
            .Where(b => b.Id == bossId)
            .Select(b => b.CurrentHp)
            .FirstAsync();
        int applied = Math.Clamp(damage, 0, hp);

        if (applied > 0)
        {
            await context.GuildBosses
                .Where(b => b.Id == bossId)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.CurrentHp, b => b.CurrentHp - applied));
        }

        var attack = new GuildBossAttack { Id = Guid.NewGuid(), BossId = bossId, UserId = userId, Damage = applied, AttackedAt = now };
        context.GuildBossAttacks.Add(attack);
        await context.SaveChangesAsync();
        context.Entry(attack).State = EntityState.Detached;

        // Добил тот, чей удар опустил HP до нуля; условие на DefeatedAt — на случай двух добиваний сразу
        bool killed = false;
        if (applied > 0 && hp - applied == 0)
        {
            killed = await context.GuildBosses
                .Where(b => b.Id == bossId && b.DefeatedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.DefeatedAt, now)
                    .SetProperty(b => b.KilledByUserId, userId)) > 0;
        }

        await transaction.CommitAsync();
        return new GuildBossDamageResult(applied, hp - applied, killed);
    }

    public async Task<Dictionary<GuildBossTier, DateTime>> GetCooldownsAsync(Guid userId)
    {
        return await context.GuildBossCooldowns
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToDictionaryAsync(c => c.Tier, c => c.ReadyAt);
    }

    public async Task<DateTime?> TryStartCooldownAsync(Guid userId, GuildBossTier tier, DateTime now, DateTime readyAt)
    {
        // Условный апдейт: из двух одновременных ударов откат начнёт только один
        int started = await context.GuildBossCooldowns
            .Where(c => c.UserId == userId && c.Tier == tier && c.ReadyAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ReadyAt, readyAt));
        if (started > 0)
            return null;

        // Строки ещё нет — первый удар по боссу этого уровня
        var cooldown = new GuildBossCooldown { UserId = userId, Tier = tier, ReadyAt = readyAt };
        context.GuildBossCooldowns.Add(cooldown);
        try
        {
            await context.SaveChangesAsync();
            context.Entry(cooldown).State = EntityState.Detached;
            return null;
        }
        catch (DbUpdateException)
        {
            // Строка есть, и откат ещё идёт
            context.ChangeTracker.Clear();
        }

        return await context.GuildBossCooldowns
            .Where(c => c.UserId == userId && c.Tier == tier)
            .Select(c => (DateTime?)c.ReadyAt)
            .FirstOrDefaultAsync() ?? readyAt;
    }

    public async Task<HashSet<GuildBossTier>> GetClaimedTiersAsync(Guid userId, DateTime day)
    {
        var tiers = await context.GuildBossRewardClaims
            .Where(c => c.UserId == userId && c.Day == day)
            .Select(c => c.Tier)
            .ToListAsync();
        return tiers.ToHashSet();
    }

    public async Task<bool> ClaimBossRewardAsync(GuildBossRewardClaim claim, PlayerReward reward)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Ключ (UserId, Tier, Day) и есть защита: вторая награда за тот же уровень в те же сутки не запишется,
        // даже если игрок перешёл в другую гильдию или нажал дважды
        reward.ClaimedAt = claim.ClaimedAt;
        context.GuildBossRewardClaims.Add(claim);
        context.PlayerRewards.Add(reward);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();
            return false;
        }

        await RewardGrant.ApplyAsync(context, reward.PlayerId, reward);

        await transaction.CommitAsync();
        // Отслеживаемая копия не дала бы даже попытаться записать повторную награду — та упала бы не на ключе в БД
        context.Entry(claim).State = EntityState.Detached;
        context.Entry(reward).State = EntityState.Detached;
        foreach (var chest in reward.Chests)
            context.Entry(chest).State = EntityState.Detached;
        return true;
    }
}
