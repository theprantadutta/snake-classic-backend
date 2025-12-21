using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Scores.DTOs;

public record ScoreDto(
    Guid Id,
    int ScoreValue,
    int GameDurationSeconds,
    int FoodsEaten,
    GameMode GameMode,
    Difficulty Difficulty,
    DateTime CreatedAt
);

public record ScoreSubmitDto(
    int Score,
    int GameDurationSeconds,
    int FoodsEaten,
    string GameMode,
    string Difficulty,
    string? IdempotencyKey,
    Dictionary<string, object>? GameData
);

public record UserStatsDto(
    int TotalGamesPlayed,
    int HighScore,
    int TotalScore,
    int TotalFoodsEaten,
    int TotalPlayTimeSeconds,
    double AverageScore,
    int CurrentStreak,
    int BestStreak
);

public record BatchScoreResultDto(
    int Processed,
    int Skipped,
    List<string> SkippedKeys
);
