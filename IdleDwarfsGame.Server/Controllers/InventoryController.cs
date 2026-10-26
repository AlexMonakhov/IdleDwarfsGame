using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Сундуки и гемы залогиненного игрока
[Authorize]
[ApiController]
[Route("inventory")]
public class InventoryController(ChestService chestService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<InventoryInfo>> GetMyInventory()
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        return Ok(await chestService.GetInventoryAsync(playerId));
    }

    [HttpPost("me/chests/{chestId:guid}/open")]
    public async Task<ActionResult<ChestOpenResult>> OpenChest(Guid chestId)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            return Ok(await chestService.OpenAsync(playerId, chestId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid? CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId) ? playerId : null;
}
