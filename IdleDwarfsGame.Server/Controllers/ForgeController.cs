using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Кузница: переплавка предметов и сплав камней в более редкие
[Authorize]
[ApiController]
[Route("forge")]
public class ForgeController(ForgeService forgeService) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<ForgeOverview>> Get() =>
        Run(playerId => forgeService.GetAsync(playerId));

    [HttpPost("items")]
    public Task<ActionResult<ForgeItemResult>> FuseItems([FromBody] ForgeItemsRequest request) =>
        Run(playerId => forgeService.FuseItemsAsync(playerId, request.EquipmentIds));

    [HttpPost("gems")]
    public Task<ActionResult<ForgeGemResult>> FuseGems([FromBody] ForgeGemsRequest request) =>
        Run(playerId => forgeService.FuseGemsAsync(playerId, request.Gem, request.Rarity, request.Times));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (ForgeException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
