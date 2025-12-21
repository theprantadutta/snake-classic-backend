namespace SnakeClassic.Application.Features.BattlePass.DTOs;

public record BattlePassSeasonDto(
    Guid Id,
    string SeasonId,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    int MaxLevel,
    decimal Price,
    bool IsActive,
    List<Dictionary<string, object>>? LevelsConfig
);

public record UserBattlePassProgressDto(
    Guid SeasonId,
    string SeasonName,
    bool HasPremium,
    int CurrentLevel,
    int CurrentXp,
    int XpToNextLevel,
    List<int> ClaimedFreeRewards,
    List<int> ClaimedPremiumRewards,
    int DaysRemaining
);

public record AddXpResultDto(
    int NewLevel,
    int NewXp,
    int XpToNextLevel,
    bool LeveledUp,
    int LevelsGained
);

public record ClaimRewardResultDto(
    bool Success,
    string? RewardType,
    int? RewardAmount,
    string? Message
);
