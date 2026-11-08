using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class DepositSendRequest
{
    public List<Guid>? HeroIds { get; set; }
}

// Ивент «Общие шахты»: карта месторождений на весь сервер, дворфы игроков, улучшения для всех и награды по доле
[Authorize]
[ApiController]
[Route("events/community-mines")]
public class CommunityMinesController(CommunityMineService communityMines) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<CommunityMinesOverview>> Get() =>
        Run(playerId => communityMines.GetAsync(playerId));

    [HttpPost("{depositId:guid}/dwarves")]
    public Task<ActionResult<CommunityMinesOverview>> Send(Guid depositId, [FromBody] DepositSendRequest request) =>
        Run(playerId => communityMines.SendAsync(playerId, depositId, request.HeroIds));

    [HttpPost("{depositId:guid}/dwarves/{heroId:guid}/recall")]
    public Task<ActionResult<CommunityMinesOverview>> Recall(Guid depositId, Guid heroId) =>
        Run(playerId => communityMines.RecallAsync(playerId, depositId, heroId));

    [HttpPost("{depositId:guid}/upgrade")]
    public Task<ActionResult<CommunityMinesOverview>> Upgrade(Guid depositId) =>
        Run(playerId => communityMines.UpgradeAsync(playerId, depositId));

    [HttpPost("claim")]
    public Task<ActionResult<CommunityMinesClaimResult>> Claim() =>
        Run(playerId => communityMines.ClaimAsync(playerId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (CommunityMineException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
