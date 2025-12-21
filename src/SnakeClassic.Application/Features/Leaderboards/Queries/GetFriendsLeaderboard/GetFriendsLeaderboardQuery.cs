using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Leaderboards.DTOs;

namespace SnakeClassic.Application.Features.Leaderboards.Queries.GetFriendsLeaderboard;

public record GetFriendsLeaderboardQuery(int Limit = 50) : IRequest<Result<LeaderboardResponseDto>>;
