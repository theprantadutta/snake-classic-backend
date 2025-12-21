using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Achievements.Commands.SeedAchievements;

public record SeedAchievementsCommand : IRequest<Result<SeedAchievementsResultDto>>;

public record SeedAchievementsResultDto(int Created, int Updated, int Total);
