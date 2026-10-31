public interface IEquipmentRepository
{
    Task<List<PlayerEquipment>> GetByPlayerAsync(Guid playerId);
    Task<PlayerEquipment?> GetAsync(Guid playerId, Guid itemId);

    // Надевает предмет в слот героя одной транзакцией; то, что там лежало, возвращается в инвентарь
    Task EquipAsync(PlayerEquipment item, Guid heroId, EquipmentSlot slot);
    Task UnequipAsync(PlayerEquipment item);
}
