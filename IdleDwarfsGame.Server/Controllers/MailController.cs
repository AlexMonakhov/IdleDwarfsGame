using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Почта игрока: список писем, отметка «прочитано», получение вложений
[Authorize]
[ApiController]
[Route("mail")]
public class MailController(MailService mailService) : ControllerBase
{
    [HttpGet("me")]
    public Task<ActionResult<List<MailInfo>>> GetMine() =>
        Run(playerId => mailService.GetMailAsync(playerId));

    [HttpPost("me/{mailId:guid}/read")]
    public Task<ActionResult<MailInfo>> Read(Guid mailId) =>
        Run(playerId => mailService.MarkReadAsync(playerId, mailId));

    [HttpPost("me/{mailId:guid}/claim")]
    public Task<ActionResult<MailClaimResult>> Claim(Guid mailId) =>
        Run(playerId => mailService.ClaimAsync(playerId, mailId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (MailException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
