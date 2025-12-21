using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Scores.DTOs;

namespace SnakeClassic.Application.Features.Scores.Commands.SubmitScore;

public record SubmitScoreCommand(
    int Score,
    int GameDurationSeconds,
    int FoodsEaten,
    string GameMode,
    string Difficulty,
    string? IdempotencyKey,
    Dictionary<string, object>? GameData
) : IRequest<Result<ScoreDto>>;
