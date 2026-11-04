using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class MiningRepository(GameDbContext context) : IMiningRepository
{
    // Кристаллы шахт — обычные гемы 1-го уровня: одна стопка на камень
    private const Rarity CrystalRarity = Rarity.Common;
    private const int CrystalLevel = 1;

    public async Task<List<Mine>> GetMinesAsync(Guid playerId)
    {
        return await context.Mines
            .AsNoTracking()
            .Where(m => m.PlayerId == playerId)
            .OrderBy(m => m.Plot)
            .ToListAsync();
    }

    public async Task<Mine?> GetAsync(Guid playerId, Guid mineId)
    {
        return await context.Mines
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mineId && m.PlayerId == playerId);
    }

    public async Task<HashSet<Guid>> GetHeroIdsAsync(Guid playerId)
    {
        var ids = await context.Mines
            .Where(m => m.PlayerId == playerId && m.HeroId != null)
            .Select(m => m.HeroId!.Value)
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task<MiningWallet> GetWalletAsync(Guid playerId)
    {
        // Без трекинга: ресурсы меняются через ExecuteUpdate, отслеживаемые копии могли устареть
        var resources = await context.Players
            .AsNoTracking()
            .Where(p => p.Id == playerId)
            .Select(p => new { p.Gold, p.Experience })
            .FirstOrDefaultAsync();

        var stacks = await context.PlayerItems
            .AsNoTracking()
            .Where(i => i.PlayerId == playerId && i.ItemType == ItemType.Gem
                && i.Rarity == CrystalRarity && i.Level == CrystalLevel && i.Quantity > 0)
            .Select(i => new { i.SubType, i.Quantity })
            .ToListAsync();

        var crystals = new Dictionary<GemType, int>();
        foreach (var stack in stacks)
        {
            if (Enum.TryParse<GemType>(stack.SubType, out var gem))
                crystals[gem] = stack.Quantity;
        }

        return new MiningWallet
        {
            Gold = resources?.Gold ?? 0,
            Experience = resources?.Experience ?? 0,
            Crystals = crystals
        };
    }

    public async Task<MiningSaveOutcome> BuildAsync(Mine mine, MiningChange change)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        context.Mines.Add(mine);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Уникальный индекс PlayerId + Plot: место заняли параллельным запросом
            context.ChangeTracker.Clear();
            return MiningSaveOutcome.Changed;
        }
        context.Entry(mine).State = EntityState.Detached;

        if (!await ApplyAsync(mine.PlayerId, change))
            return MiningSaveOutcome.NotEnoughResources;

        await transaction.CommitAsync();
        return MiningSaveOutcome.Saved;
    }

    public async Task<MiningSaveOutcome> SaveAsync(Mine mine, int expectedVersion, MiningChange change)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        int updated;
        try
        {
            // Условный апдейт: из двух параллельных изменений одной шахты пройдёт только первое
            updated = await context.Mines
                .Where(m => m.Id == mine.Id && m.PlayerId == mine.PlayerId && m.Version == expectedVersion)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.Level, mine.Level)
                    .SetProperty(m => m.SpeedLevel, mine.SpeedLevel)
                    .SetProperty(m => m.YieldLevel, mine.YieldLevel)
                    .SetProperty(m => m.StorageLevel, mine.StorageLevel)
                    .SetProperty(m => m.HeroId, mine.HeroId)
                    .SetProperty(m => m.Stored, mine.Stored)
                    .SetProperty(m => m.UpdatedAt, mine.UpdatedAt)
                    .SetProperty(m => m.LastDigAt, mine.LastDigAt)
                    .SetProperty(m => m.Version, expectedVersion + 1));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            // Уникальный индекс по HeroId: дворфа только что поставили в другую шахту
            return MiningSaveOutcome.HeroTaken;
        }
        if (updated == 0)
            return MiningSaveOutcome.Changed;

        if (!await ApplyAsync(mine.PlayerId, change))
            return MiningSaveOutcome.NotEnoughResources;

        await transaction.CommitAsync();
        mine.Version = expectedVersion + 1;
        return MiningSaveOutcome.Saved;
    }

    // false — списать нечем; транзакция вызывающего тогда не коммитится и откатывает всё
    private async Task<bool> ApplyAsync(Guid playerId, MiningChange change)
    {
        if (change.Gold != 0 || change.Experience != 0)
        {
            int paid = await context.Players
                .Where(p => p.Id == playerId && p.Gold + change.Gold >= 0 && p.Experience + change.Experience >= 0)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Gold, p => p.Gold + change.Gold)
                    .SetProperty(p => p.Experience, p => p.Experience + change.Experience));
            if (paid == 0)
                return false;
        }

        if (change.Gem is not { } gem || change.Crystals == 0)
            return true;

        string subType = gem.ToString();
        int stacked = await context.PlayerItems
            .Where(i => i.PlayerId == playerId && i.ItemType == ItemType.Gem && i.SubType == subType
                && i.Rarity == CrystalRarity && i.Level == CrystalLevel && i.Quantity + change.Crystals >= 0)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity + change.Crystals));
        if (stacked > 0)
            return true;
        if (change.Crystals < 0)
            return false;

        // Таких кристаллов у игрока ещё не было — заводим стопку
        context.PlayerItems.Add(new PlayerItem
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            ItemType = ItemType.Gem,
            SubType = subType,
            Rarity = CrystalRarity,
            Level = CrystalLevel,
            Quantity = change.Crystals
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return true;
    }
}
