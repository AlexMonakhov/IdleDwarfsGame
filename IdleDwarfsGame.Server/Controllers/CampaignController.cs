using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("campaign")]
public class CampaignController(CampaignService campaignService, RewardService rewardService) : ControllerBase
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

    // Награда за пройденный уровень (CampaignOverview.PendingRewardId) — только теперь она попадает на аккаунт
    [HttpPost("rewards/{rewardId:guid}/claim")]
    public async Task<ActionResult<RewardClaimResult>> ClaimReward(Guid rewardId)
    {
        try
        {
            return Ok(await rewardService.ClaimAsync(CurrentPlayerId(), rewardId));
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
