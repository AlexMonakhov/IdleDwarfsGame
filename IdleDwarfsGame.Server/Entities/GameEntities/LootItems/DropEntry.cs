public class DropEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public double Probability { get; init; }      // 0.8 = 80%
    public int MinAmount { get; init; }
    public int MaxAmount { get; init; }

    public ItemType ItemType { get; init; }
    public string SubType { get; init; } = string.Empty;
    public Rarity Rarity { get; init; }

    public int ItemLevel { get; init; }
}