using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Друзья — между аккаунтами, поэтому игрок берётся из sub токена (UserId), а не из playerId
[Authorize]
[ApiController]
[Route("friends")]
public class FriendsController(FriendService friendService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FriendsOverview>> GetOverview()
    {
        if (CurrentUserId() is not { } userId)
            return Unauthorized();

        return Ok(await friendService.GetOverviewAsync(userId));
    }

    [HttpPost("requests")]
    public Task<ActionResult<SendFriendRequestResult>> SendRequest([FromBody] SendFriendRequestRequest request) =>
        Run(userId => friendService.SendRequestAsync(userId, request.Username));

    [HttpPost("requests/{fromUserId:guid}/accept")]
    public Task<ActionResult> AcceptRequest(Guid fromUserId) =>
        Run(userId => friendService.AcceptRequestAsync(userId, fromUserId));

    [HttpPost("requests/{fromUserId:guid}/decline")]
    public Task<ActionResult> DeclineRequest(Guid fromUserId) =>
        Run(userId => friendService.DeclineRequestAsync(userId, fromUserId));

    // Отозвать свою заявку
    [HttpDelete("requests/{toUserId:guid}")]
    public Task<ActionResult> CancelRequest(Guid toUserId) =>
        Run(userId => friendService.CancelRequestAsync(userId, toUserId));

    [HttpDelete("{friendId:guid}")]
    public Task<ActionResult> RemoveFriend(Guid friendId) =>
        Run(userId => friendService.RemoveFriendAsync(userId, friendId));

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (CurrentUserId() is not { } userId)
            return Unauthorized();

        try
        {
            return Ok(await action(userId));
        }
        catch (FriendException ex)
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
        catch (FriendException ex)
        {
            return BadRequest(new { code = ex.Code.ToString(), message = ex.Message });
        }
    }

    private Guid? CurrentUserId() =>
        Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId) ? userId : null;
}
