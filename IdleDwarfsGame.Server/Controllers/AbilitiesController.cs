using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Извлечение, вставка и слияние способностей, слияние дворфов — только у залогиненного игрока
[Authorize]
[ApiController]
[Route("abilities")]
public class AbilitiesController(AbilityService abilityService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<List<AbilityInfo>>> GetMyAbilities()
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        return Ok(await abilityService.GetMyAsync(playerId));
    }

    // Дворф распыляется, выбранная способность остаётся у игрока
    [HttpPost("extract")]
    public Task<ActionResult<AbilityActionResult>> Extract([FromBody] ExtractAbilityRequest request) =>
        Run(playerId => abilityService.ExtractAsync(playerId, request.HeroId, request.Kind, request.Key));

    [HttpPost("{abilityId:guid}/insert")]
    public Task<ActionResult<AbilityActionResult>> Insert(Guid abilityId, [FromBody] InsertAbilityRequest request) =>
        Run(playerId => abilityService.InsertAsync(playerId, abilityId, request.HeroId));

    [HttpPost("combine")]
    public Task<ActionResult<AbilityActionResult>> Combine([FromBody] CombineAbilitiesRequest request) =>
        Run(playerId => abilityService.CombineAsync(playerId, request.FirstId, request.SecondId));

    // Пять дворфов одного ранга — один рангом выше
    [HttpPost("fuse")]
    public Task<ActionResult<AbilityActionResult>> Fuse([FromBody] FuseHeroesRequest request) =>
        Run(playerId => abilityService.FuseAsync(playerId, request.HeroIds));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (AbilityException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }

    private Guid? CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId) ? playerId : null;
}
