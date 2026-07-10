using DwarfMiningIdleGame.Server.Enums;

public class ChestDropTable : IChestDropTable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int ChestLevel { get; init; }
    public ChestType ChestType { get; init; }
    public IReadOnlyCollection<DropEntry> Entries { get; init; } = new List<DropEntry>();
}