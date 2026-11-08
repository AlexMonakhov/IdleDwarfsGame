using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Лавки с постоянным ассортиментом: лавка старейшин (Elder) и лавка текущего ивента (Event — вместе с ивентом недели)
[Authorize]
[ApiController]
[Route("fixed-shops")]
public class FixedShopsController(FixedShopService shopService) : ControllerBase
{
    [HttpGet("{shop}")]
    public Task<ActionResult<FixedShopOverview>> Get(FixedShopKind shop) =>
        Run(playerId => shopService.GetAsync(playerId, shop));

    [HttpPost("{shop}/{offerKey}/buy")]
    public Task<ActionResult<FixedShopBuyResult>> Buy(FixedShopKind shop, string offerKey) =>
        Run(playerId => shopService.BuyAsync(playerId, shop, offerKey));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (FixedShopException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
