using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Achievements.DTOs;

public record AchievementDto(
    Guid Id,
    string AchievementId,
    string Name,
    string? Description,
    AchievementCategory Category,
    AchievementTier Tier,
    string? Icon,
    int XpReward,
    int CoinReward,
    RequirementType RequirementType,
    int RequirementValue
);

public record UserAchievementDto(
    Guid Id,
    AchievementDto Achievement,
    int CurrentProgress,
    bool IsUnlocked,
    DateTime? UnlockedAt,
    bool RewardClaimed
);

public record UpdateProgressResultDto(
    bool Success,
    bool NewlyUnlocked,
    int CurrentProgress,
    int RequiredProgress
);
