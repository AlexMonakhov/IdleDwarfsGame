using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Социальные действия: помощь в шахте друга, наёмники из отряда друга, ставки на «Бой дня».
// Игрок — из sub токена (UserId), как в друзьях и гильдиях
[Authorize]
[ApiController]
public class SocialController(FriendHelpService friendHelp, ArenaBetService bets) : ControllerBase
{
    [HttpGet("friends/{friendId:guid}/mines")]
    public Task<ActionResult<FriendMinesOverview>> FriendMines(Guid friendId) =>
        Run(userId => friendHelp.GetMinesAsync(userId, friendId));

    [HttpPost("friends/{friendId:guid}/mines/{mineId:guid}/help")]
    public Task<ActionResult<FriendHelpResult>> HelpMine(Guid friendId, Guid mineId) =>
        Run(userId => friendHelp.HelpMineAsync(userId, friendId, mineId));

    [HttpGet("friends/mercenaries")]
    public Task<ActionResult<MercenaryOverview>> Mercenaries() =>
        Run(userId => friendHelp.GetMercenariesAsync(userId));

    [HttpPost("campaign/{stageId:int}/fight-with-mercenary")]
    public Task<ActionResult<MercenaryFightResult>> FightWithMercenary(int stageId, [FromBody] MercenaryFightRequest request) =>
        Run(userId => friendHelp.FightCampaignAsync(userId, stageId, request.LenderUserId, request.HeroId, request.Slot));

    [HttpGet("pvp/bets")]
    public Task<ActionResult<ArenaBetOverview>> Bets() => Run(userId => bets.GetAsync(userId));

    [HttpPost("pvp/bets")]
    public Task<ActionResult<ArenaBetOverview>> PlaceBet([FromBody] ArenaBetRequest request) =>
        Run(userId => bets.PlaceAsync(userId, request.Side, request.Amount));

    [HttpPost("pvp/bets/claim")]
    public Task<ActionResult<ArenaBetOverview>> ClaimBets() => Run(userId => bets.ClaimAsync(userId));

    [HttpGet("pvp/bets/{windowStart:datetime}/replay")]
    public Task<ActionResult<ArenaBetReplay>> Replay(DateTime windowStart) => Run(_ => bets.ReplayAsync(windowStart.ToUniversalTime()));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            return Unauthorized();
        try
        {
            return Ok(await action(userId));
        }
        catch (FriendHelpException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
        catch (ArenaBetException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Проверки кампании (уровень закрыт) — текстом, как в CampaignController
            return BadRequest(new { code = (string?)null, message = ex.Message });
        }
    }
}
