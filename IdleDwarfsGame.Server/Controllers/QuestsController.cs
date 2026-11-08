using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Ежедневные задания игрока
[Authorize]
[ApiController]
[Route("quests")]
public class QuestsController(QuestService questService) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<QuestBoard>> Get() =>
        Run(playerId => questService.GetAsync(playerId));

    [HttpPost("{questId:guid}/claim")]
    public Task<ActionResult<QuestClaimResult>> Claim(Guid questId) =>
        Run(playerId => questService.ClaimAsync(playerId, questId));

    [HttpPost("{questId:guid}/reroll")]
    public Task<ActionResult<QuestBoard>> Reroll(Guid questId) =>
        Run(playerId => questService.RerollAsync(playerId, questId));

    [HttpPost("milestones/{index:int}/claim")]
    public Task<ActionResult<QuestClaimResult>> ClaimMilestone(int index) =>
        Run(playerId => questService.ClaimMilestoneAsync(playerId, index));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (QuestException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
