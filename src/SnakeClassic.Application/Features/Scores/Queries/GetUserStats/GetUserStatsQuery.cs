using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Scores.DTOs;

namespace SnakeClassic.Application.Features.Scores.Queries.GetUserStats;

public record GetUserStatsQuery : IRequest<Result<UserStatsDto>>;
