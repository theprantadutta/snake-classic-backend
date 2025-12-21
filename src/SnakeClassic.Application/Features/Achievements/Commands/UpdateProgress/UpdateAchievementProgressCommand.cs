using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Achievements.DTOs;

namespace SnakeClassic.Application.Features.Achievements.Commands.UpdateProgress;

public record UpdateAchievementProgressCommand(
    string AchievementId,
    int Progress
) : IRequest<Result<UpdateProgressResultDto>>;
