using IdleDwarfsGame.Server.Enums;

// Таблица дропа сундука (тип + уровень + стихия). Как бросается — см. ItemDropService
public class ChestDropTable : IChestDropTable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int ChestLevel { get; init; }
    public ChestType ChestType { get; init; }
    // Стихия сундука: у сундуков бесконечности своя таблица на каждую стихию, у остальных — Basic (без стихии)
    public HeroElement Element { get; init; } = HeroElement.Basic;
    // Сколько раз бросается каждая группа записей (по ItemType)
    public int Rolls { get; init; } = 1;
    public IReadOnlyCollection<DropEntry> Entries { get; init; } = new List<DropEntry>();
}
