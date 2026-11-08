using Microsoft.EntityFrameworkCore;

public class WorldBossRepository(GameDbContext context) : IWorldBossRepository
{
    public async Task<WorldBoss?> GetLatestAsync(DateTime eventStart)
    {
        return await context.WorldBosses
            .AsNoTracking()
            .Where(b => b.EventStart == eventStart)
            .OrderByDescending(b => b.Stage)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> TryCreateAsync(WorldBoss boss)
    {
        context.WorldBosses.Add(boss);
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Этот этап только что создал параллельный запрос
            return false;
        }
        finally
        {
            // HP меняется через ExecuteUpdate — отслеживаемая копия устарела бы
            context.ChangeTracker.Clear();
        }
    }

    public async Task<DateTime?> GetCooldownAsync(Guid playerId)
    {
        return await context.WorldBossCooldowns
            .AsNoTracking()
            .Where(c => c.PlayerId == playerId)
            .Select(c => (DateTime?)c.ReadyAt)
            .FirstOrDefaultAsync();
    }

    public async Task<DateTime?> TryStartCooldownAsync(Guid playerId, DateTime now, DateTime readyAt)
    {
        // Условный апдейт: из двух одновременных ударов откат начнёт только один
        int started = await context.WorldBossCooldowns
            .Where(c => c.PlayerId == playerId && c.ReadyAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ReadyAt, readyAt));
        if (started > 0)
            return null;

        var cooldown = new WorldBossCooldown { PlayerId = playerId, ReadyAt = readyAt };
        context.WorldBossCooldowns.Add(cooldown);
        try
        {
            await context.SaveChangesAsync();
            return null;
        }
        catch (DbUpdateException)
        {
            // Строка есть, и откат ещё идёт
            context.ChangeTracker.Clear();
            return await GetCooldownAsync(playerId) ?? readyAt;
        }
        finally
        {
            context.ChangeTracker.Clear();
        }
    }

    public async Task<WorldBossDamageResult> ApplyDamageAsync(WorldBoss boss, Guid playerId, int damage, int attackCurrency, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // UPDATE без изменений блокирует строку босса до коммита: параллельные удары читают HP по очереди
        await context.WorldBosses
            .Where(b => b.Id == boss.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.CurrentHp, b => b.CurrentHp));

        // Пока шёл бой, босса били другие: урон срезается до того, что у него осталось
        int hp = await context.WorldBosses.Where(b => b.Id == boss.Id).Select(b => b.CurrentHp).FirstAsync();
        int applied = Math.Clamp(damage, 0, hp);

        if (applied > 0)
        {
            await context.WorldBosses
                .Where(b => b.Id == boss.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.CurrentHp, b => b.CurrentHp - applied));
        }

        int updated = await context.WorldBossContributions
            .Where(c => c.BossId == boss.Id && c.PlayerId == playerId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Damage, c => c.Damage + applied)
                .SetProperty(c => c.Attacks, c => c.Attacks + 1)
                .SetProperty(c => c.LastAttackAt, now));
        if (updated == 0)
        {
            // Первый удар игрока по этому боссу; строка босса заблокирована — параллельной вставки быть не может
            context.WorldBossContributions.Add(new WorldBossContribution
            {
                BossId = boss.Id, PlayerId = playerId, Damage = applied, Attacks = 1, LastAttackAt = now
            });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        await EventWallets.AddAsync(context, playerId, boss.EventStart, attackCurrency);
        context.ChangeTracker.Clear();

        // Добил тот, чей удар опустил HP до нуля; условие на DefeatedAt — на случай двух добиваний сразу
        bool killed = false;
        if (applied > 0 && hp - applied == 0)
        {
            killed = await context.WorldBosses
                .Where(b => b.Id == boss.Id && b.DefeatedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.DefeatedAt, now)
                    .SetProperty(b => b.KilledByPlayerId, playerId)) > 0;
        }

        await transaction.CommitAsync();
        return new WorldBossDamageResult(applied, hp - applied, killed);
    }

    public async Task<List<WorldBossDamageEntry>> GetTopAsync(Guid bossId, int count)
    {
        var rows = await context.WorldBossContributions
            .AsNoTracking()
            .Where(c => c.BossId == bossId)
            .OrderByDescending(c => c.Damage)
            .Take(count)
            .Select(c => new { c.PlayerId, c.Damage })
            .ToListAsync();
        return await WithNamesAsync(rows.Select(r => (r.PlayerId, r.Damage)).ToList());
    }

    public async Task<WorldBossContribution?> GetContributionAsync(Guid bossId, Guid playerId)
    {
        return await context.WorldBossContributions
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.BossId == bossId && c.PlayerId == playerId);
    }

    public async Task<List<(WorldBoss Boss, WorldBossContribution Contribution)>> GetUnclaimedKillsAsync(Guid playerId, DateTime eventStart)
    {
        var rows = await context.WorldBossContributions
            .AsNoTracking()
            .Where(c => c.PlayerId == playerId && c.Damage > 0)
            .Join(context.WorldBosses.Where(b => b.EventStart == eventStart && b.DefeatedAt != null),
                c => c.BossId, b => b.Id, (c, b) => new { Boss = b, Contribution = c })
            .Where(x => !context.WorldBossRewardClaims.Any(r => r.BossId == x.Boss.Id && r.PlayerId == playerId))
            .OrderBy(x => x.Boss.Stage)
            .ToListAsync();
        return rows.Select(r => (r.Boss, r.Contribution)).ToList();
    }

    public async Task<bool> ClaimAsync(Guid playerId, DateTime eventStart, IReadOnlyList<WorldBossRewardClaim> claims)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        context.WorldBossRewardClaims.AddRange(claims);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Часть наград только что забрал параллельный запрос — ключ (BossId, PlayerId) не пустил второй раз
            context.ChangeTracker.Clear();
            return false;
        }
        context.ChangeTracker.Clear();

        await EventWallets.AddAsync(context, playerId, eventStart, claims.Sum(c => c.Currency));
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return true;
    }

    public async Task<List<WorldBossDamageEntry>> GetEventTopAsync(DateTime eventStart, int count)
    {
        var rows = await EventDamage(eventStart)
            .OrderByDescending(r => r.Damage)
            .Take(count)
            .ToListAsync();
        return await WithNamesAsync(rows.Select(r => (r.PlayerId, r.Damage)).ToList());
    }

    public async Task<(long Damage, int Rank)> GetEventRankAsync(DateTime eventStart, Guid playerId)
    {
        long damage = await EventDamage(eventStart).Where(r => r.PlayerId == playerId).Select(r => r.Damage).FirstOrDefaultAsync();
        if (damage <= 0)
            return (0, 0);
        int above = await EventDamage(eventStart).CountAsync(r => r.Damage > damage);
        return (damage, above + 1);
    }

    private IQueryable<EventDamageRow> EventDamage(DateTime eventStart) =>
        context.WorldBossContributions
            .Where(c => context.WorldBosses.Any(b => b.Id == c.BossId && b.EventStart == eventStart))
            .GroupBy(c => c.PlayerId)
            .Select(g => new EventDamageRow { PlayerId = g.Key, Damage = g.Sum(c => c.Damage) });

    private async Task<List<WorldBossDamageEntry>> WithNamesAsync(List<(Guid PlayerId, long Damage)> rows)
    {
        var playerIds = rows.Select(r => r.PlayerId).ToList();
        var names = await context.Users
            .AsNoTracking()
            .Where(u => playerIds.Contains(u.PlayerId))
            .ToDictionaryAsync(u => u.PlayerId, u => u.Username);
        return rows.Select(r => new WorldBossDamageEntry(r.PlayerId, names.GetValueOrDefault(r.PlayerId, string.Empty), r.Damage)).ToList();
    }

    private class EventDamageRow
    {
        public Guid PlayerId { get; set; }
        public long Damage { get; set; }
    }
}
