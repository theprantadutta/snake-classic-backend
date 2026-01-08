using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Users.Commands.ResetStatistics;

public record ResetStatisticsCommand() : IRequest<Result<ResetStatisticsResult>>;

public record ResetStatisticsResult(
    bool Success,
    int ScoresDeleted,
    int DailyChallengesReset,
    int AchievementsReset
);
