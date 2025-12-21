using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Users.DTOs;

public record UserProfileDto(
    Guid Id,
    string? Email,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    UserStatus Status,
    int HighScore,
    int Level,
    int Coins,
    int TotalGamesPlayed,
    bool IsAnonymous,
    DateTime CreatedAt,
    DateTime? LastActiveAt,
    UserPreferencesDto? Preferences
);

public record UserPreferencesDto(
    string Theme,
    bool SoundEnabled,
    bool MusicEnabled,
    bool VibrationEnabled,
    bool NotificationsEnabled
);

public record UserSearchResultDto(
    Guid Id,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    int HighScore,
    int Level
);
