using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Гильдии — между аккаунтами, поэтому игрок берётся из sub токена (UserId), а не из playerId
[Authorize]
[ApiController]
[Route("guilds")]
public class GuildsController(GuildService guildService) : ControllerBase
{
    // Своя гильдия (null, если её нет) и свои заявки
    [HttpGet("me")]
    public Task<ActionResult<GuildOverview>> GetOverview() =>
        Run(userId => guildService.GetOverviewAsync(userId));

    // Пустой запрос — самые большие гильдии
    [HttpGet("search")]
    public Task<ActionResult<List<GuildSearchResult>>> Search([FromQuery] string? query) =>
        Run(userId => guildService.SearchAsync(userId, query));

    [HttpPost]
    public Task<ActionResult<GuildOverview>> Create([FromBody] CreateGuildRequest request) =>
        Run(userId => guildService.CreateAsync(userId, request.Name));

    [HttpPost("leave")]
    public Task<ActionResult> Leave() =>
        Run(userId => guildService.LeaveAsync(userId));

    // Заявка на вступление
    [HttpPost("{guildId:guid}/requests")]
    public Task<ActionResult> SendRequest(Guid guildId) =>
        Run(userId => guildService.SendRequestAsync(userId, guildId));

    // Отозвать свою заявку
    [HttpDelete("{guildId:guid}/requests")]
    public Task<ActionResult> CancelRequest(Guid guildId) =>
        Run(userId => guildService.CancelRequestAsync(userId, guildId));

    // Дальше — действия главы над заявками и участниками своей гильдии
    [HttpPost("requests/{fromUserId:guid}/accept")]
    public Task<ActionResult> AcceptRequest(Guid fromUserId) =>
        Run(userId => guildService.AcceptRequestAsync(userId, fromUserId));

    [HttpPost("requests/{fromUserId:guid}/decline")]
    public Task<ActionResult> DeclineRequest(Guid fromUserId) =>
        Run(userId => guildService.DeclineRequestAsync(userId, fromUserId));

    [HttpDelete("members/{memberId:guid}")]
    public Task<ActionResult> Kick(Guid memberId) =>
        Run(userId => guildService.KickAsync(userId, memberId));

    // ===== Боссы =====
    [HttpGet("bosses")]
    public Task<ActionResult<GuildBossesOverview>> GetBosses() =>
        Run(userId => guildService.GetBossesAsync(userId));

    [HttpPost("bosses/{tier}/attack")]
    public Task<ActionResult<GuildBossAttackResult>> Attack(GuildBossTier tier) =>
        Run(userId => guildService.AttackAsync(userId, tier));

    // Награда за убитого босса — сразу на аккаунт
    [HttpPost("bosses/{tier}/claim")]
    public Task<ActionResult<RewardClaimResult>> ClaimReward(GuildBossTier tier) =>
        Run(userId => guildService.ClaimBossRewardAsync(userId, tier));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (CurrentUserId() is not { } userId)
            return Unauthorized();

        try
        {
            return Ok(await action(userId));
        }
        catch (GuildException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }

    private async Task<ActionResult> Run(Func<Guid, Task> action)
    {
        if (CurrentUserId() is not { } userId)
            return Unauthorized();

        try
        {
            await action(userId);
            return NoContent();
        }
        catch (GuildException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }

    private Guid? CurrentUserId() =>
        Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId) ? userId : null;
}
