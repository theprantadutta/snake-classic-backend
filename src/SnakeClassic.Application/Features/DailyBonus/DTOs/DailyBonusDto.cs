namespace SnakeClassic.Application.Features.DailyBonus.DTOs;

public record DayRewardDto(
    int Day,
    int Coins,
    string? BonusItem,
    bool Claimed
);

public record TodayRewardDto(
    int Day,
    int Coins,
    string? BonusItem
);

public record DailyBonusStatusDto(
    bool CanClaim,
    int CurrentStreak,
    DateTime? LastClaimDate,
    TodayRewardDto TodayReward,
    List<DayRewardDto> WeekRewards
);

public record ClaimDailyBonusResultDto(
    bool Success,
    TodayRewardDto Reward,
    int NewBalance,
    int NewStreak,
    DateTime NextClaimAvailableAt
);

public record ClaimDailyBonusErrorDto(
    bool Success,
    string Error,
    string Message,
    DateTime NextClaimAvailableAt
);
