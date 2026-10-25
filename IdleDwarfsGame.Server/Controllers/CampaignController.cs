using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("campaign")]
public class CampaignController(CampaignService campaignService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CampaignOverview>> GetCampaign()
    {
        return Ok(await campaignService.GetOverviewAsync(CurrentPlayerId()));
    }

    [HttpPost("{stageId:int}/fight")]
    public async Task<ActionResult<CampaignFightResult>> Fight(int stageId)
    {
        try
        {
            return Ok(await campaignService.FightAsync(CurrentPlayerId(), stageId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Залогиненный игрок берётся из токена, гость пока играет за демо-игрока (как в CombatController)
    private Guid CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId)
            ? playerId
            : HeroSeedData.DemoPlayerId;
}
