using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Магазины: рынок, подпольный рынок (PvP) и лавка гильдии. Полка у каждого игрока своя
[Authorize]
[ApiController]
[Route("shop")]
public class ShopController(ShopService shopService) : ControllerBase
{
    [HttpGet("{shop}")]
    public Task<ActionResult<ShopOverview>> Get(ShopKind shop) =>
        Run(playerId => shopService.GetAsync(playerId, shop));

    [HttpPost("{shop}/reroll")]
    public Task<ActionResult<ShopOverview>> Reroll(ShopKind shop) =>
        Run(playerId => shopService.RerollAsync(playerId, shop));

    [HttpPost("offers/{offerId:guid}/lock")]
    public Task<ActionResult<ShopOverview>> Lock(Guid offerId, [FromBody] ShopLockRequest request) =>
        Run(playerId => shopService.SetLockedAsync(playerId, offerId, request.Locked));

    [HttpPost("offers/{offerId:guid}/buy")]
    public Task<ActionResult<ShopBuyResult>> Buy(Guid offerId) =>
        Run(playerId => shopService.BuyAsync(playerId, offerId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (ShopException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
