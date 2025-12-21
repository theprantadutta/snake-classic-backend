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
    public async Task<ActionResult> GetGlobalLeaderboard([FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var result = await Mediator.Send(new GetGlobalLeaderboardQuery(limit, offset));
        return HandleResult(result);
    }

    [HttpGet("weekly")]
    [AllowAnonymous]
    public async Task<ActionResult> GetWeeklyLeaderboard([FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var result = await Mediator.Send(new GetWeeklyLeaderboardQuery(limit, offset));
        return HandleResult(result);
    }

    [HttpGet("daily")]
    [AllowAnonymous]
    public async Task<ActionResult> GetDailyLeaderboard([FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var result = await Mediator.Send(new GetDailyLeaderboardQuery(limit, offset));
        return HandleResult(result);
    }

    [HttpGet("friends")]
    [Authorize]
    public async Task<ActionResult> GetFriendsLeaderboard([FromQuery] int limit = 50)
    {
        var result = await Mediator.Send(new GetFriendsLeaderboardQuery(limit));
        return HandleResult(result);
    }
}
