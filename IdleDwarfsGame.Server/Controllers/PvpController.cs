using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// PvP — между аккаунтами, поэтому игрок берётся из sub токена (UserId), а не из playerId
[Authorize]
[ApiController]
[Route("pvp")]
public class PvpController(PvpService pvpService, RankedPvpService rankedService) : ControllerBase
{
    // Соперники, очки и таблицы сезонов, история боёв
    [HttpGet("me")]
    public Task<ActionResult<PvpOverview>> GetOverview() =>
        Run(userId => pvpService.GetOverviewAsync(userId));

    [HttpPost("opponents/{opponentId:guid}/fight")]
    public Task<ActionResult<PvpFightResult>> Fight(Guid opponentId) =>
        Run(userId => pvpService.FightAsync(userId, opponentId));

    // Рейтинговая арена: закреплённый отряд, соперники по рейтингу, свои сезоны и история
    [HttpGet("ranked/me")]
    public Task<ActionResult<RankedOverview>> GetRankedOverview() =>
        Run(userId => rankedService.GetOverviewAsync(userId));

    // Закрепить текущий отряд аккаунта (снимок) — раз в сутки
    [HttpPost("ranked/squad")]
    public Task<ActionResult<RankedOverview>> LockRankedSquad() =>
        Run(userId => rankedService.LockSquadAsync(userId));

    [HttpPost("ranked/opponents/{opponentId:guid}/fight")]
    public Task<ActionResult<PvpFightResult>> RankedFight(Guid opponentId) =>
        Run(userId => rankedService.FightAsync(userId, opponentId));

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
