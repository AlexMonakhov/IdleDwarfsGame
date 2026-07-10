using Microsoft.AspNetCore.Mvc;
using DwarfMiningIdleGame.Server.Enums;
using DwarfMiningIdleGame.Server.Entities.GameEntities;

namespace DwarfMiningIdleGame.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerInventoryController(ILogger<PlayerInventoryController> logger) : ControllerBase
    {


        [HttpGet(Name = "GetInventory")]
        public IEnumerable<ILootItem> Get()
        {
            return null;
        }

        [HttpPost("open-chest")]
        public async Task<ActionResult<IEnumerable<ILootItem>>> Post(
            [FromQuery] int level,
            [FromQuery] ChestType chestType,
            [FromServices] IItemDropService dropService)
        {
            try
            {
                var result = await dropService.OpenChestAsync(level, chestType);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("open-player-chest")]
        public async Task<ActionResult<IEnumerable<ILootItem>>> OpenPlayerChest([FromBody] OpenPlayerChestRequest request,
            [FromServices] IPlayerChestRepository chestRepository,
            [FromServices] IItemDropService dropService)
        {
            // Проверка наличия сундука у игрока
            var playerChest = await chestRepository.GetPlayerChestAsync(request.UserId, request.ChestId);
            if (playerChest == null)
                return NotFound("Chest not found or already opened.");

            // Открытие сундука
            var loot = await dropService.OpenChestAsync(
                playerChest.ChestDropTable.ChestLevel, 
                playerChest.ChestDropTable.ChestType);

            // Отметить сундук как открытый
            await chestRepository.MarkChestAsOpenedAsync(playerChest);

            return Ok(loot);
        }

        [HttpPost("add-chest")]
        public async Task<ActionResult> AddChestToPlayer([FromBody] AddChestToPlayerRequest request,
            [FromServices] IPlayerChestRepository chestRepository)
        {
            try
            {
                var chest = new PlayerChest
                {
                    Id = Guid.NewGuid(),
                    PlayerId = request.PlayerId,
                    ChestDropTableId = request.ChestDropTableId,
                    IsOpened = false
                };

                await chestRepository.AddChestToPlayerAsync(chest);
                return Ok(new { chestId = chest.Id, message = "Chest added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
