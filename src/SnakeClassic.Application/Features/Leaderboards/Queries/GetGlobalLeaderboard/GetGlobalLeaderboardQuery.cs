using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Leaderboards.DTOs;

namespace SnakeClassic.Application.Features.Leaderboards.Queries.GetGlobalLeaderboard;

public record GetGlobalLeaderboardQuery(int Limit = 100, int Offset = 0) : IRequest<Result<LeaderboardResponseDto>>;
