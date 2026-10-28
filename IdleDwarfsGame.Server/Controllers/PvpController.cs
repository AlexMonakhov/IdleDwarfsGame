using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// PvP — между аккаунтами, поэтому игрок берётся из sub токена (UserId), а не из playerId
[Authorize]
[ApiController]
[Route("pvp")]
public class PvpController(PvpService pvpService) : ControllerBase
{
    // Соперники, очки и таблицы сезонов, история боёв
    [HttpGet("me")]
    public Task<ActionResult<PvpOverview>> GetOverview() =>
        Run(userId => pvpService.GetOverviewAsync(userId));

    [HttpPost("opponents/{opponentId:guid}/fight")]
    public Task<ActionResult<PvpFightResult>> Fight(Guid opponentId) =>
        Run(userId => pvpService.FightAsync(userId, opponentId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            return Unauthorized();

        try
        {
            return Ok(await action(userId));
        }
        catch (PvpException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
