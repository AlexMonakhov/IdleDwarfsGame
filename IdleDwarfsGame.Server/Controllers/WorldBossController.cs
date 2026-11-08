using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Ивент «Мировые боссы»: босс на весь сервер, удар отрядом игрока, награды за убитых боссов
[Authorize]
[ApiController]
[Route("events/world-boss")]
public class WorldBossController(WorldBossService worldBosses) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<WorldBossOverview>> Get() =>
        Run(playerId => worldBosses.GetAsync(playerId));

    [HttpPost("attack")]
    public Task<ActionResult<WorldBossAttackResult>> Attack() =>
        Run(playerId => worldBosses.AttackAsync(playerId));

    [HttpPost("claim")]
    public Task<ActionResult<WorldBossClaimResult>> Claim() =>
        Run(playerId => worldBosses.ClaimAsync(playerId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (WorldBossException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
