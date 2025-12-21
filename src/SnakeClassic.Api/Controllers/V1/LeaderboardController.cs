using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Leaderboards.Queries.GetDailyLeaderboard;
using SnakeClassic.Application.Features.Leaderboards.Queries.GetFriendsLeaderboard;
using SnakeClassic.Application.Features.Leaderboards.Queries.GetGlobalLeaderboard;
using SnakeClassic.Application.Features.Leaderboards.Queries.GetWeeklyLeaderboard;

namespace SnakeClassic.Api.Controllers.V1;

public class LeaderboardController : BaseApiController
{
    [HttpGet("global")]
    [AllowAnonymous]
    public async Task<ActionResult> GetGlobalLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var offset = (page - 1) * pageSize;
        var result = await Mediator.Send(new GetGlobalLeaderboardQuery(pageSize, offset));
        return HandleResult(result);
    }

    [HttpGet("weekly")]
    [AllowAnonymous]
    public async Task<ActionResult> GetWeeklyLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var offset = (page - 1) * pageSize;
        var result = await Mediator.Send(new GetWeeklyLeaderboardQuery(pageSize, offset));
        return HandleResult(result);
    }

    [HttpGet("daily")]
    [AllowAnonymous]
    public async Task<ActionResult> GetDailyLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var offset = (page - 1) * pageSize;
        var result = await Mediator.Send(new GetDailyLeaderboardQuery(pageSize, offset));
        return HandleResult(result);
    }

    [HttpGet("friends")]
    [Authorize]
    public async Task<ActionResult> GetFriendsLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await Mediator.Send(new GetFriendsLeaderboardQuery(pageSize));
        return HandleResult(result);
    }
}
