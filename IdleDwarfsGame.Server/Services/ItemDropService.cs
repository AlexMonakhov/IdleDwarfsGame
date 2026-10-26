// Бросок таблицы дропа сундука.
// Записи делятся на группы по ItemType (гемы отдельно, герои отдельно), каждая группа бросается table.Rolls раз.
// За бросок группы выпадает не больше одной записи: шанс записи — её Probability,
// остаток до 1 — пустой бросок. Если сумма шансов группы больше 1, они делятся на сумму (тогда пустых бросков нет)
public class ItemDropService(Random random)
{
    public List<LootDrop> Roll(IChestDropTable table)
    {
        var groups = table.Entries
            .GroupBy(e => e.ItemType)
            .Select(g => g.ToList())
            .ToList();

        var drops = new List<LootDrop>();
        for (int roll = 0; roll < Math.Max(1, table.Rolls); roll++)
        {
            foreach (var group in groups)
            {
                if (Pick(group, random.NextDouble()) is not { } entry)
                    continue;

                int amount = random.Next(entry.MinAmount, Math.Max(entry.MinAmount, entry.MaxAmount) + 1);
                if (amount > 0)
                    drops.Add(new LootDrop(entry.ItemType, entry.SubType, entry.Rarity, entry.ItemLevel, amount));
            }
        }

        // Одинаковое из разных бросков складываем в одну стопку
        return drops
            .GroupBy(d => d with { Amount = 0 })
            .Select(g => g.Key with { Amount = g.Sum(d => d.Amount) })
            .ToList();
    }

    // roll — случайное число из [0, 1)
    public static DropEntry? Pick(IReadOnlyList<DropEntry> entries, double roll)
    {
        double total = entries.Sum(e => Math.Max(0, e.Probability));
        if (total <= 0)
            return null;

        double scale = Math.Max(1, total);
        double cumulative = 0;
        foreach (var entry in entries)
        {
            cumulative += Math.Max(0, entry.Probability) / scale;
            if (roll < cumulative)
                return entry;
        }

        // Нормализованные шансы в сумме дают 1, но из-за округления roll мог чуть-чуть не попасть
        return total > 1 ? entries.Last(e => e.Probability > 0) : null;
    }
}
