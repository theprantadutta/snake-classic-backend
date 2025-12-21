using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Auth.DTOs;

public record UserDto(
    Guid Id,
    string? Email,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    UserStatus Status,
    int HighScore,
    int Level,
    int Coins,
    bool IsAnonymous,
    DateTime CreatedAt,
    DateTime? LastActiveAt
);
