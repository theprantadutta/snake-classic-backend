using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Achievements.DTOs;

namespace SnakeClassic.Application.Features.Achievements.Queries.GetAchievements;

public record GetAchievementsQuery : IRequest<Result<List<AchievementDto>>>;
