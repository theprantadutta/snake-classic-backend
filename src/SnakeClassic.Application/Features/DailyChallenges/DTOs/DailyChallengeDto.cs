using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.DailyChallenges.DTOs;

public record DailyChallengeDto(
    Guid Id,
    string Title,
    string Description,
    ChallengeType Type,
    ChallengeDifficulty Difficulty,
    int TargetValue,
    int CurrentProgress,
    bool IsCompleted,
    int CoinReward,
    int XpReward,
    string? RequiredGameMode,
    bool ClaimedReward
);

public record DailyChallengesResponse(
    List<DailyChallengeDto> Challenges,
    int CompletedCount,
    int TotalCount,
    bool AllCompleted,
    int BonusCoins
);

public record UpdateChallengeProgressRequest(
    ChallengeType Type,
    int Value,
    string? GameMode
);

public record UpdateChallengeProgressResponse(
    List<DailyChallengeDto> UpdatedChallenges,
    List<Guid> NewlyCompletedIds
);

public record ClaimChallengeRewardRequest(
    Guid ChallengeId
);

public record ClaimRewardResponse(
    bool Success,
    int CoinsEarned,
    int XpEarned,
    int BonusCoins,
    string? Message
);
