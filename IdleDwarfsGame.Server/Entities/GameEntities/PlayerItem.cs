// Стопка одинаковых предметов в инвентаре игрока (пока — гемы из сундуков).
// Одна строка на (игрок, тип, подтип, редкость, уровень), новые штуки прибавляются к Quantity
public class PlayerItem
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }

    public ItemType ItemType { get; set; }
    public string SubType { get; set; } = string.Empty;
    public Rarity Rarity { get; set; }
    public int Level { get; set; } = 1;

    public int Quantity { get; set; }
}
