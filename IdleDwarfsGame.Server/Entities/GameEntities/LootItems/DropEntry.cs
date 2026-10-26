public class DropEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    // Шанс записи за один бросок своей группы: 0.2 = 20%. Остаток до 1 — «ничего не выпало»,
    // если сумма по группе больше 1 — шансы делятся на сумму
    public double Probability { get; init; }
    public int MinAmount { get; init; }
    public int MaxAmount { get; init; }

    public ItemType ItemType { get; init; }
    public string SubType { get; init; } = string.Empty;
    public Rarity Rarity { get; init; }

    public int ItemLevel { get; init; }
}
