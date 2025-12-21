using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Users.DTOs;

namespace SnakeClassic.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string? DisplayName,
    string? PhotoUrl,
    UserPreferencesUpdateDto? Preferences
) : IRequest<Result<UserProfileDto>>;

public record UserPreferencesUpdateDto(
    string? Theme,
    bool? SoundEnabled,
    bool? MusicEnabled,
    bool? VibrationEnabled,
    bool? NotificationsEnabled
);
