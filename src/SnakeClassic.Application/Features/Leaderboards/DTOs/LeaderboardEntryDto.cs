namespace SnakeClassic.Application.Features.Leaderboards.DTOs;

public record LeaderboardEntryDto(
    int Rank,
    Guid UserId,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    int Score,
    int Level,
    DateTime? AchievedAt
);

public record LeaderboardResponseDto(
    List<LeaderboardEntryDto> Entries,
    int? CurrentUserRank,
    int TotalPlayers
);
