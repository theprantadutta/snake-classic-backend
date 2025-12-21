using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Achievements.Commands.ClaimReward;

public record ClaimAchievementRewardCommand(string AchievementId) : IRequest<Result<ClaimRewardResultDto>>;

public record ClaimRewardResultDto(
    bool Success,
    int XpAwarded,
    int CoinsAwarded
);
