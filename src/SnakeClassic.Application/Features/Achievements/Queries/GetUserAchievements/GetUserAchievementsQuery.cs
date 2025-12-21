using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Achievements.DTOs;

namespace SnakeClassic.Application.Features.Achievements.Queries.GetUserAchievements;

public record GetUserAchievementsQuery : IRequest<Result<UserAchievementsResponseDto>>;
