using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.DailyChallenges.Commands.ClaimReward;
using SnakeClassic.Application.Features.DailyChallenges.Commands.UpdateProgress;
using SnakeClassic.Application.Features.DailyChallenges.DTOs;
using SnakeClassic.Application.Features.DailyChallenges.Queries.GetTodaysChallenges;
using SnakeClassic.Infrastructure.Services.BackgroundJobs;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class DailyChallengesController : BaseApiController
{
    private readonly IDailyChallengeJobService _dailyChallengeJobService;

    public DailyChallengesController(IDailyChallengeJobService dailyChallengeJobService)
    {
        _dailyChallengeJobService = dailyChallengeJobService;
    }

    /// <summary>
    /// Get today's daily challenges with user progress
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DailyChallengesResponse>> GetTodaysChallenges()
    {
        var result = await Mediator.Send(new GetTodaysChallengesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Update progress for daily challenges (called after each game)
    /// </summary>
    [HttpPost("progress")]
    public async Task<ActionResult<UpdateChallengeProgressResponse>> UpdateProgress([FromBody] UpdateChallengeProgressRequest request)
    {
        var result = await Mediator.Send(new UpdateChallengeProgressCommand(
            Type: request.Type,
            Value: request.Value,
            GameMode: request.GameMode
        ));
        return Ok(result);
    }

    /// <summary>
    /// Claim reward for a completed challenge
    /// </summary>
    [HttpPost("claim/{challengeId:guid}")]
    public async Task<ActionResult<ClaimRewardResponse>> ClaimReward(Guid challengeId)
    {
        var result = await Mediator.Send(new ClaimChallengeRewardCommand(challengeId));
        return Ok(result);
    }

    /// <summary>
    /// Generate today's challenges manually (admin/testing)
    /// </summary>
    [HttpPost("generate")]
    [AllowAnonymous]
    public async Task<ActionResult> GenerateChallenges()
    {
        await _dailyChallengeJobService.GenerateDailyChallenges();
        return Ok(new { message = "Daily challenges generated" });
    }

    /// <summary>
    /// Send test reminder notification (admin/testing)
    /// </summary>
    [HttpPost("test-reminder")]
    [AllowAnonymous]
    public async Task<ActionResult> TestReminder()
    {
        await _dailyChallengeJobService.SendMorningReminder();
        return Ok(new { message = "Test reminder sent" });
    }
}
