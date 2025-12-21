using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Scores.DTOs;

namespace SnakeClassic.Application.Features.Scores.Commands.BatchSubmitScores;

public record BatchSubmitScoresCommand(List<ScoreSubmitDto> Scores) : IRequest<Result<BatchScoreResultDto>>;
