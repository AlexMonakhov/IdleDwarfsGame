// Экземпляр предмета у игрока. Что он умеет, описано в коде (EquipmentCatalog, ключ ItemKey) — в БД только чей он и где надет.
// В отличие от стопок PlayerItem каждая штука — своя строка: её надевают на конкретного героя
public class PlayerEquipment
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string ItemKey { get; set; } = string.Empty;

    // Оба null — предмет лежит в инвентаре
    public Guid? HeroId { get; set; }
    public EquipmentSlot? Slot { get; set; }
}
