using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Social.Commands.AcceptFriendRequest;
using SnakeClassic.Application.Features.Social.Commands.RejectFriendRequest;
using SnakeClassic.Application.Features.Social.Commands.RemoveFriend;
using SnakeClassic.Application.Features.Social.Commands.SendFriendRequest;
using SnakeClassic.Application.Features.Social.Queries.GetFriendRequests;
using SnakeClassic.Application.Features.Social.Queries.GetFriends;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class SocialController : BaseApiController
{
    [HttpGet("friends")]
    public async Task<ActionResult> GetFriends()
    {
        var result = await Mediator.Send(new GetFriendsQuery());
        return HandleResult(result);
    }

    [HttpGet("requests")]
    public async Task<ActionResult> GetFriendRequests()
    {
        var result = await Mediator.Send(new GetFriendRequestsQuery());
        return HandleResult(result);
    }

    [HttpPost("friends/request")]
    public async Task<ActionResult> SendFriendRequest([FromBody] SendFriendRequestDto request)
    {
        var result = await Mediator.Send(new SendFriendRequestCommand(request.FriendId));
        return HandleResult(result);
    }

    [HttpPost("accept/{requestId:guid}")]
    public async Task<ActionResult> AcceptFriendRequest(Guid requestId)
    {
        var result = await Mediator.Send(new AcceptFriendRequestCommand(requestId));
        return HandleResult(result);
    }

    [HttpPost("reject/{requestId:guid}")]
    public async Task<ActionResult> RejectFriendRequest(Guid requestId)
    {
        var result = await Mediator.Send(new RejectFriendRequestCommand(requestId));
        return HandleResult(result);
    }

    [HttpDelete("{friendId:guid}")]
    public async Task<ActionResult> RemoveFriend(Guid friendId)
    {
        var result = await Mediator.Send(new RemoveFriendCommand(friendId));
        return HandleResult(result);
    }
}

public record SendFriendRequestDto(Guid FriendId);
