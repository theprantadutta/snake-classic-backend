namespace SnakeClassic.Application.Features.Auth.DTOs;

public record AuthResponse(
    Guid UserId,
    string AccessToken,
    string? Email,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    bool IsNewUser
);
