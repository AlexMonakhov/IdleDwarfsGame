using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class GuildDepthPlaceRequest
{
    public Guid HeroId { get; set; }
}

// «Гонка глубины» — недельное соревнование гильдий. Игрок — из sub токена (UserId), как и в гильдиях
[Authorize]
[ApiController]
[Route("guilds/depth")]
public class GuildDepthController(GuildDepthService depth) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<GuildDepthOverview>> Get() => Run(userId => depth.GetAsync(userId));

    // Поставить (или заменить) своего дворфа
    [HttpPost("place")]
    public Task<ActionResult<GuildDepthOverview>> Place([FromBody] GuildDepthPlaceRequest request) =>
        Run(userId => depth.PlaceAsync(userId, request.HeroId));

    [HttpPost("recall")]
    public Task<ActionResult<GuildDepthOverview>> Recall() => Run(userId => depth.RecallAsync(userId));

    [HttpPost("collect")]
    public Task<ActionResult<GuildDepthOverview>> Collect() => Run(userId => depth.CollectAsync(userId));

    [HttpPost("upgrade-pick")]
    public Task<ActionResult<GuildDepthOverview>> UpgradePick() => Run(userId => depth.UpgradePickAsync(userId));

    [HttpPost("upgrade-shaft")]
    public Task<ActionResult<GuildDepthOverview>> UpgradeShaft() => Run(userId => depth.UpgradeShaftAsync(userId));

    [HttpPost("claim")]
    public Task<ActionResult<GuildDepthOverview>> Claim() => Run(userId => depth.ClaimAsync(userId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            return Unauthorized();
        try
        {
            return Ok(await action(userId));
        }
        catch (GuildDepthException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
