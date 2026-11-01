using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Снаряжение залогиненного игрока
[Authorize]
[ApiController]
[Route("equipment")]
public class EquipmentController(EquipmentService equipmentService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<List<EquipmentInfo>>> GetMyEquipment()
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        return Ok(await equipmentService.GetMyAsync(playerId));
    }

    // Надеть предмет на героя. Предмет с другого героя снимается, предмет из занятого слота возвращается в инвентарь
    [HttpPost("me/{itemId:guid}/equip")]
    public async Task<ActionResult<EquipmentChangeResult>> Equip(Guid itemId, [FromBody] EquipItemRequest request)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            return Ok(await equipmentService.EquipAsync(playerId, itemId, request.HeroId, request.Slot));
        }
        catch (EquipmentException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }

    [HttpPost("me/{itemId:guid}/unequip")]
    public async Task<ActionResult<EquipmentChangeResult>> Unequip(Guid itemId)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            return Ok(await equipmentService.UnequipAsync(playerId, itemId));
        }
        catch (EquipmentException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }

    private Guid? CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId) ? playerId : null;
}
