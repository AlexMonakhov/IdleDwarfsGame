using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("player")]
public class PlayerController(IPlayerRepository playerRepository) : ControllerBase
{
    // Золото и опыт аккаунта (шапка интерфейса)
    [HttpGet("me/resources")]
    public async Task<ActionResult<PlayerResources>> GetMyResources()
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        var resources = await playerRepository.GetResourcesAsync(playerId);
        return resources == null ? Unauthorized() : Ok(resources);
    }
}
