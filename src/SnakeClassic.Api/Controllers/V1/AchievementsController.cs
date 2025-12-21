using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Achievements.Commands.ClaimReward;
using SnakeClassic.Application.Features.Achievements.Commands.UpdateProgress;
using SnakeClassic.Application.Features.Achievements.Queries.GetAchievements;
using SnakeClassic.Application.Features.Achievements.Queries.GetUserAchievements;

namespace SnakeClassic.Api.Controllers.V1;

public class AchievementsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetAllAchievements()
    {
        var result = await Mediator.Send(new GetAchievementsQuery());
        return HandleResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetMyAchievements()
    {
        var result = await Mediator.Send(new GetUserAchievementsQuery());
        return HandleResult(result);
    }

    [HttpPost("progress")]
    [Authorize]
    public async Task<ActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        var result = await Mediator.Send(new UpdateAchievementProgressCommand(
            request.AchievementId,
            request.ProgressIncrement
        ));
        return HandleResult(result);
    }

    [HttpPost("claim")]
    [Authorize]
    public async Task<ActionResult> ClaimReward([FromBody] ClaimRewardRequest request)
    {
        var result = await Mediator.Send(new ClaimAchievementRewardCommand(request.AchievementId));
        return HandleResult(result);
    }
}

public record UpdateProgressRequest(string AchievementId, int ProgressIncrement);
public record ClaimRewardRequest(string AchievementId);
