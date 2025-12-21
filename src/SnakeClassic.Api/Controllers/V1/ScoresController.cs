using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Scores.Commands.BatchSubmitScores;
using SnakeClassic.Application.Features.Scores.Commands.SubmitScore;
using SnakeClassic.Application.Features.Scores.DTOs;
using SnakeClassic.Application.Features.Scores.Queries.GetUserScores;
using SnakeClassic.Application.Features.Scores.Queries.GetUserStats;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class ScoresController : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        var command = new SubmitScoreCommand(
            request.Score,
            request.GameDurationSeconds,
            request.FoodsEaten,
            request.GameMode,
            request.Difficulty,
            request.IdempotencyKey,
            request.GameData
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult> GetMyScores([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        var result = await Mediator.Send(new GetUserScoresQuery(limit, offset));
        return HandleResult(result);
    }

    [HttpGet("me/stats")]
    public async Task<ActionResult> GetMyStats()
    {
        var result = await Mediator.Send(new GetUserStatsQuery());
        return HandleResult(result);
    }

    [HttpPost("batch")]
    public async Task<ActionResult> BatchSubmitScores([FromBody] BatchSubmitScoresRequest request)
    {
        var scores = request.Scores.Select(s => new ScoreSubmitDto(
            s.Score,
            s.GameDurationSeconds,
            s.FoodsEaten,
            s.GameMode,
            s.Difficulty,
            s.IdempotencyKey,
            s.GameData
        )).ToList();

        var result = await Mediator.Send(new BatchSubmitScoresCommand(scores));
        return HandleResult(result);
    }
}

public record SubmitScoreRequest(
    int Score,
    int GameDurationSeconds,
    int FoodsEaten,
    string GameMode,
    string Difficulty,
    string? IdempotencyKey,
    Dictionary<string, object>? GameData
);

public record BatchSubmitScoresRequest(List<SubmitScoreRequest> Scores);
