using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Экспедиции есть только у залогиненного игрока: героев отправляют из его инвентаря
[Authorize]
[ApiController]
[Route("expeditions")]
public class ExpeditionsController(ExpeditionService expeditionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ExpeditionBoard>> GetBoard()
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        return Ok(await expeditionService.GetBoardAsync(playerId));
    }

    [HttpPost("{expeditionId:guid}/start")]
    public async Task<ActionResult<ExpeditionInfo>> Start(Guid expeditionId, [FromBody] StartExpeditionRequest request)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            return Ok(await expeditionService.StartAsync(playerId, expeditionId, request.HeroIds));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{expeditionId:guid}/claim")]
    public async Task<ActionResult<ExpeditionClaimResult>> Claim(Guid expeditionId)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            return Ok(await expeditionService.ClaimAsync(playerId, expeditionId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid? CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId) ? playerId : null;
}
