using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Scores.DTOs;

namespace SnakeClassic.Application.Features.Scores.Queries.GetUserScores;

public record GetUserScoresQuery(int Limit = 50, int Offset = 0) : IRequest<Result<List<ScoreDto>>>;
