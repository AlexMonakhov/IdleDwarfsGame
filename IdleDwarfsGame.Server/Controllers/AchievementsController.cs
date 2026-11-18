using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Достижения и календарь входов игрока
[Authorize]
[ApiController]
public class AchievementsController(AchievementService achievementService, LoginCalendarService loginCalendarService) : ControllerBase
{
    [HttpGet("achievements")]
    public Task<ActionResult<AchievementBoard>> Get() =>
        Run(playerId => achievementService.GetAsync(playerId));

    [HttpPost("achievements/{key}/claim")]
    public Task<ActionResult<AchievementClaimResult>> Claim(string key) =>
        Run(playerId => achievementService.ClaimAsync(playerId, key));

    [HttpPost("achievements/claim-all")]
    public Task<ActionResult<AchievementClaimResult>> ClaimAll() =>
        Run(playerId => achievementService.ClaimAllAsync(playerId));

    [HttpGet("login-calendar")]
    public Task<ActionResult<LoginCalendarInfo>> GetCalendar() =>
        Run(playerId => loginCalendarService.GetAsync(playerId));

    [HttpPost("login-calendar/claim")]
    public Task<ActionResult<LoginClaimResult>> ClaimCalendar() =>
        Run(playerId => loginCalendarService.ClaimAsync(playerId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (AchievementException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
