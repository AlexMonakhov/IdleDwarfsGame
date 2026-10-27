using IdleDwarfsGame.Server.Entities.GameEntities;
using Microsoft.EntityFrameworkCore;

public class RewardRepository(GameDbContext context) : IRewardRepository
{
    public async Task<PlayerReward?> GetUnclaimedAsync(Guid playerId, RewardSource source)
    {
        return await context.PlayerRewards
            .AsNoTracking()
            .Include(r => r.Chests)
            .Where(r => r.PlayerId == playerId && r.Source == source && r.ClaimedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<PlayerReward?> ClaimAsync(Guid playerId, Guid rewardId, DateTime now)
    {
        var reward = await context.PlayerRewards
            .AsNoTracking()
            .Include(r => r.Chests)
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
                .Where(t => t.ChestType == chest.ChestType && t.ChestLevel == chest.ChestLevel)
                .Select(t => (Guid?)t.Id)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException($"Chest drop table for {chest.ChestType} level {chest.ChestLevel} not found");

            context.PlayerChests.Add(new PlayerChest
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                ChestDropTableId = tableId,
                IsOpened = false
            });
        }
        await context.SaveChangesAsync();
    }
}
