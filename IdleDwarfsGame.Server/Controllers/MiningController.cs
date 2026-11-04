using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Шахты есть только у залогиненного игрока
[Authorize]
[ApiController]
[Route("mining")]
public class MiningController(MiningService miningService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MiningBoard>> GetBoard()
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        return Ok(await miningService.GetBoardAsync(playerId));
    }

    [HttpPost("plots/{plot:int}/build")]
    public Task<ActionResult<MiningActionResult>> Build(int plot, [FromBody] BuildMineRequest request) =>
        Run(playerId => miningService.BuildAsync(playerId, plot, request.Type));

    [HttpPost("{mineId:guid}/dig")]
    public Task<ActionResult<MiningDigResult>> Dig(Guid mineId, [FromBody] DigMineRequest request) =>
        Run(playerId => miningService.DigAsync(playerId, mineId, request.Clicks));

    [HttpPost("{mineId:guid}/upgrade")]
    public Task<ActionResult<MiningActionResult>> Upgrade(Guid mineId, [FromBody] UpgradeMineRequest request) =>
        Run(playerId => miningService.UpgradeAsync(playerId, mineId, request.Upgrade));

    [HttpPut("{mineId:guid}/dwarf")]
    public Task<ActionResult<MiningActionResult>> AssignDwarf(Guid mineId, [FromBody] AssignMineDwarfRequest request) =>
        Run(playerId => miningService.AssignDwarfAsync(playerId, mineId, request.HeroId));

    [HttpDelete("{mineId:guid}/dwarf")]
    public Task<ActionResult<MiningActionResult>> RemoveDwarf(Guid mineId) =>
        Run(playerId => miningService.RemoveDwarfAsync(playerId, mineId));

    [HttpPost("{mineId:guid}/collect")]
    public Task<ActionResult<MiningCollectResult>> Collect(Guid mineId) =>
        Run(playerId => miningService.CollectAsync(playerId, mineId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (MiningException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }

    private Guid? CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId) ? playerId : null;
}
