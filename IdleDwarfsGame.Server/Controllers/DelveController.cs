using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// «Глубинный спуск» — бесконечный забег-рогалик своим отрядом (DelveRules)
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

    [HttpPost("reward")]
    public Task<ActionResult<DelveOverview>> Reward([FromBody] DelveRewardRequest request) =>
        Run(userId => delve.ChooseRewardAsync(userId, request.Index, request.HeroId));

    [HttpPost("reward/decline")]
    public Task<ActionResult<DelveDeclineResult>> DeclineReward() => Run(userId => delve.DeclineRewardAsync(userId));

    [HttpPost("order")]
    public Task<ActionResult<DelveOverview>> Order([FromBody] DelveOrderRequest request) =>
        Run(userId => delve.ReorderAsync(userId, request.HeroIds));

    [HttpPost("revive")]
    public Task<ActionResult<DelveOverview>> Revive() => Run(userId => delve.ReviveAsync(userId));

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
