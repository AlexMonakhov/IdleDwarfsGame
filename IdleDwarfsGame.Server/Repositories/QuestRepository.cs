using Microsoft.EntityFrameworkCore;

public class QuestRepository(GameDbContext context) : IQuestRepository
{
    public async Task<DailyQuestState?> GetStateAsync(Guid playerId)
    {
        return await context.DailyQuestStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.PlayerId == playerId);
    }

    public async Task<List<PlayerQuest>> GetQuestsAsync(Guid playerId, DateTime day)
    {
        return await context.PlayerQuests
            .AsNoTracking()
            .Where(q => q.PlayerId == playerId && q.Day == day)
            .OrderBy(q => q.Slot)
            .ToListAsync();
    }

    public async Task<PlayerQuest?> GetQuestAsync(Guid playerId, Guid questId)
    {
        return await context.PlayerQuests
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == questId && q.PlayerId == playerId);
    }

    public async Task<QuestSaveOutcome> StartDayAsync(DailyQuestState state, int? expectedVersion, List<PlayerQuest> quests, DateTime keepActivityFrom)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if (!await SaveStateAsync(state, expectedVersion))
            return QuestSaveOutcome.Changed;

        await context.PlayerQuests.Where(q => q.PlayerId == state.PlayerId).ExecuteDeleteAsync();
        await context.PlayerDailyActivities
            .Where(a => a.PlayerId == state.PlayerId && a.Day < keepActivityFrom)
            .ExecuteDeleteAsync();

        context.PlayerQuests.AddRange(quests);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return QuestSaveOutcome.Saved;
    }

    public async Task<QuestSaveOutcome> RerollAsync(DailyQuestState state, Guid questId, PlayerQuest replacement, bool useFreeReroll, long rainbowCost)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Бесплатная замена — одна на сутки: флаг ставится условным апдейтом вместе с версией доски
        int updated = await context.DailyQuestStates
            .Where(s => s.PlayerId == state.PlayerId && s.Day == state.Day && s.Version == state.Version
                && (!useFreeReroll || !s.FreeRerollUsed))
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.FreeRerollUsed, x => x.FreeRerollUsed || useFreeReroll)
                .SetProperty(x => x.Version, state.Version + 1));
        if (updated == 0)
            return QuestSaveOutcome.Changed;

        if (rainbowCost > 0)
        {
            int paid = await context.Players
                .Where(p => p.Id == state.PlayerId && p.RainbowCoins >= rainbowCost)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.RainbowCoins, p => p.RainbowCoins - rainbowCost));
            if (paid == 0)
                return QuestSaveOutcome.NotEnoughResources;
        }

        int removed = await context.PlayerQuests
            .Where(q => q.Id == questId && q.PlayerId == state.PlayerId && q.ClaimedAt == null)
            .ExecuteDeleteAsync();
        if (removed == 0)
            return QuestSaveOutcome.Changed;

        context.PlayerQuests.Add(replacement);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await transaction.CommitAsync();
        return QuestSaveOutcome.Saved;
    }

    public async Task<bool> ClaimQuestAsync(PlayerQuest quest, DateTime? eventStart, DateTime now)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт защищает от двойного начисления при повторном клике
        int claimed = await context.PlayerQuests
            .Where(q => q.Id == quest.Id && q.ClaimedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(q => q.ClaimedAt, now));
        if (claimed == 0)
            return false;

        await GrantAsync(quest.PlayerId, new PlayerReward { Gold = quest.Gold, Experience = quest.Experience },
            quest.ElderSeals, quest.EventCurrency, eventStart);

        await transaction.CommitAsync();
        return true;
    }

    public async Task<QuestSaveOutcome> ClaimMilestoneAsync(DailyQuestState state, int index, PlayerReward reward, int elderSeals,
        int eventCurrency, DateTime? eventStart)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        int bit = 1 << index;
        int updated = await context.DailyQuestStates
            .Where(s => s.PlayerId == state.PlayerId && s.Day == state.Day && s.Version == state.Version
                && (s.MilestonesClaimed & bit) == 0)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.MilestonesClaimed, x => x.MilestonesClaimed | bit)
                .SetProperty(x => x.Version, state.Version + 1));
        if (updated == 0)
            return QuestSaveOutcome.Changed;

        await GrantAsync(state.PlayerId, reward, elderSeals, eventCurrency, eventStart);

        await transaction.CommitAsync();
        return QuestSaveOutcome.Saved;
    }

    private async Task GrantAsync(Guid playerId, PlayerReward reward, int elderSeals, int eventCurrency, DateTime? eventStart)
    {
        // Золото, опыт и сундуки — так же, как у наград за бои
        await RewardGrant.ApplyAsync(context, playerId, reward);

        if (elderSeals > 0)
        {
            await context.Players
                .Where(p => p.Id == playerId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ElderSeals, p => p.ElderSeals + elderSeals));
        }

        if (eventCurrency > 0 && eventStart is { } start)
            await EventWallets.AddAsync(context, playerId, start, eventCurrency);

        context.ChangeTracker.Clear();
    }

    private async Task<bool> SaveStateAsync(DailyQuestState state, int? expectedVersion)
    {
        if (expectedVersion is { } version)
        {
            // Условный апдейт: из двух параллельных сборок доски пройдёт только первая
            int updated = await context.DailyQuestStates
                .Where(s => s.PlayerId == state.PlayerId && s.Version == version)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Day, state.Day)
                    .SetProperty(x => x.FreeRerollUsed, state.FreeRerollUsed)
                    .SetProperty(x => x.MilestonesClaimed, state.MilestonesClaimed)
                    .SetProperty(x => x.Version, version + 1));
            state.Version = version + 1;
            return updated > 0;
        }

        // Первый заход: ключ PlayerId не даст завести доску дважды
        context.DailyQuestStates.Add(state);
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        finally
        {
            context.ChangeTracker.Clear();
        }
    }
}

// Начисление ивентовой валюты; вызывается внутри транзакции того, кто её выдаёт
public static class EventWallets
{
    public static async Task AddAsync(GameDbContext context, Guid playerId, DateTime eventStart, long amount)
    {
        if (amount <= 0)
            return;

        if (await IncrementAsync(context, playerId, eventStart, amount))
            return;

        // Первая валюта ивента. Параллельная первая выдача упрётся в ключ — тогда прибавляем к созданному ею кошельку
        var wallet = new PlayerEventWallet { PlayerId = playerId, EventStart = eventStart, Currency = amount, Earned = amount };
        context.PlayerEventWallets.Add(wallet);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            context.Entry(wallet).State = EntityState.Detached;
            if (!await IncrementAsync(context, playerId, eventStart, amount))
                throw;
        }
        context.Entry(wallet).State = EntityState.Detached;
    }

    private static async Task<bool> IncrementAsync(GameDbContext context, Guid playerId, DateTime eventStart, long amount) =>
        await context.PlayerEventWallets
            .Where(w => w.PlayerId == playerId && w.EventStart == eventStart)
            .ExecuteUpdateAsync(s => s
                .SetProperty(w => w.Currency, w => w.Currency + amount)
                .SetProperty(w => w.Earned, w => w.Earned + amount)) > 0;
}
