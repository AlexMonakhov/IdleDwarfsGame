using DwarfMiningIdleGame.Server.Entities.GameEntities;
using DwarfMiningIdleGame.Server.Enums;

public interface IPlayerChestRepository
{
    Task<PlayerChest?> GetPlayerChestAsync(Guid playerId, Guid chestId);
    Task<bool> PlayerHasChestAsync(Guid playerId, Guid chestId);
    Task MarkChestAsOpenedAsync(PlayerChest chest);
    Task<IChestDropTable?> GetChestDropTableByLevelAndTypeAsync(int chestLevel, ChestType chestType);
    Task AddChestToPlayerAsync(PlayerChest chest);
}
