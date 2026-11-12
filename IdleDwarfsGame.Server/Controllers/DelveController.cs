using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// «Глубинный спуск» — забег-рогалик по карте недели (концепт)
[Authorize]
[ApiController]
[Route("delve")]
public class DelveController(DelveService delve) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<DelveOverview>> Get() => Run(userId => delve.GetAsync(userId));

    [HttpPost("start")]
    public Task<ActionResult<DelveOverview>> Start() => Run(userId => delve.StartAsync(userId));

    [HttpPost("move")]
    public Task<ActionResult<DelveMoveResult>> Move([FromBody] DelveMoveRequest request) =>
        Run(userId => delve.MoveAsync(userId, request.Column));

    [HttpPost("relic")]
    public Task<ActionResult<DelveOverview>> Relic([FromBody] DelveRelicRequest request) =>
        Run(userId => delve.ChooseRelicAsync(userId, request.Key));

    [HttpPost("camp")]
    public Task<ActionResult<DelveOverview>> Camp([FromBody] DelveCampRequest request) =>
        Run(userId => delve.CampAsync(userId, request.Action, request.HeroId));

    [HttpPost("retreat")]
    public Task<ActionResult<DelveOverview>> Retreat() => Run(userId => delve.RetreatAsync(userId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            return Unauthorized();
        try
        {
            return Ok(await action(userId));
        }
        catch (DelveException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }
}
