using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("tower")]
public class TowerController(TowerService towerService, RewardService rewardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TowerOverview>> GetTower()
    {
        return Ok(await towerService.GetOverviewAsync(CurrentPlayerId()));
    }

    [HttpPost("{floor:int}/fight")]
    public async Task<ActionResult<TowerFightResult>> Fight(int floor)
    {
        try
        {
            return Ok(await towerService.FightAsync(CurrentPlayerId(), floor));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Пропуск до лучшего этажа прошлых недель (TowerOverview.SkipToFloor): награда за все этажи сразу на аккаунте
    [HttpPost("skip")]
    public async Task<ActionResult<TowerSkipResult>> Skip()
    {
        try
        {
            return Ok(await towerService.SkipAsync(CurrentPlayerId()));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Награда за пройденный этаж (TowerOverview.PendingRewardId) — только теперь она попадает на аккаунт
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

    // Залогиненный игрок берётся из токена, гость пока играет за демо-игрока (как в CampaignController)
    private Guid CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId)
            ? playerId
            : HeroSeedData.DemoPlayerId;
}
