using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Заходы в игру: вкладка открывает сессию, продлевает её пингами и закрывает при уходе
[Authorize]
[ApiController]
[Route("sessions")]
public class SessionsController(SessionService sessionService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SessionStarted>> Start()
    {
        if (CurrentUserId() is not { } userId)
            return Unauthorized();

        return Ok(await sessionService.StartAsync(userId, Request.Headers.UserAgent.ToString()));
    }

    // 404 — сессия уже закрыта: клиент открывает новую
    [HttpPost("{sessionId:guid}/heartbeat")]
    public async Task<ActionResult<SessionHeartbeat>> Heartbeat(Guid sessionId)
    {
        if (CurrentUserId() is not { } userId)
            return Unauthorized();

        var heartbeat = await sessionService.HeartbeatAsync(userId, sessionId);
        return heartbeat == null ? NotFound() : Ok(heartbeat);
    }

    [HttpPost("{sessionId:guid}/end")]
    public async Task<ActionResult> End(Guid sessionId)
    {
        if (CurrentUserId() is not { } userId)
            return Unauthorized();

        return await sessionService.EndAsync(userId, sessionId) ? NoContent() : NotFound();
    }

    private Guid? CurrentUserId() =>
        Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId) ? userId : null;
}
