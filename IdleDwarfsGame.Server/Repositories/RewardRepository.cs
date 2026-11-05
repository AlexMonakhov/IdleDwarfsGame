using IdleDwarfsGame.Server.Entities.GameEntities;
using Microsoft.EntityFrameworkCore;

public class RewardRepository(GameDbContext context) : IRewardRepository
{
    public async Task<List<PlayerReward>> GetUnclaimedAsync(Guid playerId, RewardSource source)
    {
        return await context.PlayerRewards
            .AsNoTracking()
            .Include(r => r.Chests)
            .Include(r => r.Items)
            .Where(r => r.PlayerId == playerId && r.Source == source && r.ClaimedAt == null)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.SourceId)
            .ToListAsync();
    }

    public async Task<List<PlayerReward>> ClaimAllAsync(Guid playerId, RewardSource source, DateTime now)
    {
        var pending = await GetUnclaimedAsync(playerId, source);
        var claimed = new List<PlayerReward>();
        if (pending.Count == 0)
            return claimed;

        await using var transaction = await context.Database.BeginTransactionAsync();

        foreach (var reward in pending)
        {
            // Условный апдейт по каждой награде: ту, что только что забрали в другой вкладке, второй раз не начислим
            int updated = await context.PlayerRewards
                .Where(r => r.Id == reward.Id && r.ClaimedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.ClaimedAt, now));
            if (updated == 0)
                continue;

            await RewardGrant.ApplyAsync(context, playerId, reward);
            reward.ClaimedAt = now;
            claimed.Add(reward);
        }

        await transaction.CommitAsync();
        return claimed;
    }

    public async Task<PlayerReward?> ClaimAsync(Guid playerId, Guid rewardId, DateTime now)
    {
        var reward = await context.PlayerRewards
            .AsNoTracking()
            .Include(r => r.Chests)
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == rewardId && r.PlayerId == playerId && r.ClaimedAt == null);
        if (reward == null)
            return null;

        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт защищает от двойного начисления при повторном клике
        int claimed = await context.PlayerRewards
            .Where(r => r.Id == rewardId && r.ClaimedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.ClaimedAt, now));
        if (claimed == 0)
            return null;

        await RewardGrant.ApplyAsync(context, playerId, reward);

        await transaction.CommitAsync();
        reward.ClaimedAt = now;
        return reward;
    }
}

// Начисление награды на аккаунт; вызывается внутри транзакции того, кто отмечает награду полученной
public static class RewardGrant
{
    public static async Task ApplyAsync(GameDbContext context, Guid playerId, PlayerReward reward)
    {
        await context.Players
            .Where(p => p.Id == playerId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Gold, p => p.Gold + reward.Gold)
                .SetProperty(p => p.Experience, p => p.Experience + reward.Experience));

        foreach (var chest in reward.Chests)
        {
            // Без таблицы дропа сундук не открыть — откатываем всю выдачу, а не теряем сундук молча
            var tableId = await context.ChestDropTables
                .Where(t => t.ChestType == chest.ChestType && t.ChestLevel == chest.ChestLevel && t.Element == chest.Element)
                .Select(t => (Guid?)t.Id)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException($"Chest drop table for {chest.ChestType} level {chest.ChestLevel} ({chest.Element}) not found");

            context.PlayerChests.Add(new PlayerChest
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                ChestDropTableId = tableId,
                IsOpened = false
            });
        }

        // Камни из награды (у одной награды стопки не повторяются — TowerRewards их уже сложил)
        foreach (var item in reward.Items)
            await PlayerItemStacks.AddAsync(context, playerId, new LootDrop(item.ItemType, item.SubType, item.Rarity, item.Level, item.Quantity));

        await context.SaveChangesAsync();
    }
}

// Стопки предметов в инвентаре (PlayerItems): такие же предметы уже есть — прибавляем к стопке, иначе заводим новую.
// Новая стопка только добавляется в контекст — сохраняет вызывающий
public static class PlayerItemStacks
{
    public static async Task AddAsync(GameDbContext context, Guid playerId, LootDrop item)
    {
        int stacked = await context.PlayerItems
            .Where(i => i.PlayerId == playerId && i.ItemType == item.ItemType && i.SubType == item.SubType
                && i.Rarity == item.Rarity && i.Level == item.Level)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity + item.Amount));
        if (stacked > 0)
            return;

        context.PlayerItems.Add(new PlayerItem
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            ItemType = item.ItemType,
            SubType = item.SubType,
            Rarity = item.Rarity,
            Level = item.Level,
            Quantity = item.Amount
        });
    }
}
