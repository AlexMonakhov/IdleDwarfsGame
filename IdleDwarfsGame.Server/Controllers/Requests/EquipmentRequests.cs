public class EquipItemRequest
{
    public Guid HeroId { get; set; }
    // null — сервер сам выберет подходящий слот
    public EquipmentSlot? Slot { get; set; }
}
