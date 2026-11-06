using System.Data.Common;
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
            .Include(m => m.Dwarves)
            .Where(m => m.PlayerId == playerId)
            .OrderBy(m => m.Plot)
            .ToListAsync();
    }

    public async Task<Mine?> GetAsync(Guid playerId, Guid mineId)
    {
        return await context.Mines
            .AsNoTracking()
            .Include(m => m.Dwarves)
            .FirstOrDefaultAsync(m => m.Id == mineId && m.PlayerId == playerId);
    }

    public async Task<HashSet<Guid>> GetHeroIdsAsync(Guid playerId)
    {
        var ids = await context.MineDwarves
            .Where(d => context.Mines.Any(m => m.Id == d.MineId && m.PlayerId == playerId))
            .Select(d => d.HeroId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task<MiningWallet> GetWalletAsync(Guid playerId)
    {
        // Без трекинга: ресурсы меняются через ExecuteUpdate, отслеживаемые копии могли устареть
        var resources = await context.Players
            .AsNoTracking()
            .Where(p => p.Id == playerId)
            .Select(p => new
            {
                p.Gold, p.Experience, p.DigEnergy, p.DigEnergyAt,
                p.PickaxeCapacityLevel, p.PickaxeRecoveryLevel, p.PickaxeStrengthLevel
            })
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
            Crystals = crystals,
            // Как лежит в БД; на текущий момент её досчитывает MiningService.DigEnergyAt
            DigEnergy = resources?.DigEnergy ?? 0,
            DigEnergyAt = resources?.DigEnergyAt,
            PickaxeCapacityLevel = resources?.PickaxeCapacityLevel ?? 1,
            PickaxeRecoveryLevel = resources?.PickaxeRecoveryLevel ?? 1,
            PickaxeStrengthLevel = resources?.PickaxeStrengthLevel ?? 1
        };
    }

    public async Task<bool> UpgradePickaxeAsync(Guid playerId, PickaxeUpgrade upgrade, int expectedLevel, MineCost cost, DigEnergySpend settle)
    {
        // Одним условным апдейтом: уровень тот же, кирку с прошлого чтения не тратили и ресурсов хватает
        var expectedAt = settle.ExpectedAt;
        long gold = cost.Gold;
        long experience = cost.Experience;
        var players = context.Players.Where(p => p.Id == playerId && p.DigEnergyAt == expectedAt
            && p.Gold >= gold && p.Experience >= experience);

        int updated = upgrade switch
        {
            PickaxeUpgrade.Capacity => await players
                .Where(p => p.PickaxeCapacityLevel == expectedLevel)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.PickaxeCapacityLevel, expectedLevel + 1)
                    .SetProperty(p => p.Gold, p => p.Gold - gold)
                    .SetProperty(p => p.Experience, p => p.Experience - experience)
                    .SetProperty(p => p.DigEnergy, settle.Value)
                    .SetProperty(p => p.DigEnergyAt, (DateTime?)settle.At)),
            PickaxeUpgrade.Recovery => await players
                .Where(p => p.PickaxeRecoveryLevel == expectedLevel)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.PickaxeRecoveryLevel, expectedLevel + 1)
                    .SetProperty(p => p.Gold, p => p.Gold - gold)
                    .SetProperty(p => p.Experience, p => p.Experience - experience)
                    .SetProperty(p => p.DigEnergy, settle.Value)
                    .SetProperty(p => p.DigEnergyAt, (DateTime?)settle.At)),
            _ => await players
                .Where(p => p.PickaxeStrengthLevel == expectedLevel)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.PickaxeStrengthLevel, expectedLevel + 1)
                    .SetProperty(p => p.Gold, p => p.Gold - gold)
                    .SetProperty(p => p.Experience, p => p.Experience - experience)
                    .SetProperty(p => p.DigEnergy, settle.Value)
                    .SetProperty(p => p.DigEnergyAt, (DateTime?)settle.At))
        };
        return updated > 0;
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

    public async Task<MiningSaveOutcome> SaveAsync(Mine mine, int expectedVersion, MiningChange change, MineDwarfChange? dwarf = null)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Условный апдейт: из двух параллельных изменений одной шахты пройдёт только первое
        int updated = await context.Mines
            .Where(m => m.Id == mine.Id && m.PlayerId == mine.PlayerId && m.Version == expectedVersion)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Level, mine.Level)
                .SetProperty(m => m.SpeedLevel, mine.SpeedLevel)
                .SetProperty(m => m.YieldLevel, mine.YieldLevel)
                .SetProperty(m => m.StorageLevel, mine.StorageLevel)
                .SetProperty(m => m.Stored, mine.Stored)
                .SetProperty(m => m.UpdatedAt, mine.UpdatedAt)
                .SetProperty(m => m.LastDigAt, mine.LastDigAt)
                .SetProperty(m => m.DigProgress, mine.DigProgress)
                .SetProperty(m => m.Version, expectedVersion + 1));
        if (updated == 0)
            return MiningSaveOutcome.Changed;

        // Сила кирки общая на все шахты: клики в двух шахтах параллельно не должны потратить её дважды —
        // пишем, только если с прошлого чтения её никто не тратил
        if (change.Energy is { } energy)
        {
            var expectedAt = energy.ExpectedAt;
            int spent = await context.Players
                .Where(p => p.Id == mine.PlayerId && p.DigEnergyAt == expectedAt)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.DigEnergy, energy.Value)
                    .SetProperty(p => p.DigEnergyAt, (DateTime?)energy.At));
            if (spent == 0)
                return MiningSaveOutcome.Changed;
        }

        if (dwarf is { Add: true })
        {
            context.MineDwarves.Add(new MineDwarf { MineId = mine.Id, HeroId = dwarf.HeroId, Slot = dwarf.Slot });
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is DbException inner && DbErrors.IsUniqueViolation(inner))
            {
                // Уникальный индекс по HeroId: дворфа только что поставили в другую шахту
                context.ChangeTracker.Clear();
                return MiningSaveOutcome.HeroTaken;
            }
            context.ChangeTracker.Clear();
        }
        else if (dwarf is { Add: false })
        {
            await context.MineDwarves
                .Where(d => d.MineId == mine.Id && d.HeroId == dwarf.HeroId)
                .ExecuteDeleteAsync();
        }

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
