using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Social.DTOs;

public record FriendDto(
    Guid UserId,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    UserStatus Status,
    int HighScore,
    int Level,
    DateTime FriendsSince
);

public record FriendRequestDto(
    Guid RequestId,
    Guid FromUserId,
    string? FromUsername,
    string? FromDisplayName,
    string? FromPhotoUrl,
    DateTime SentAt
);

public record FriendsResponseDto(
    List<FriendDto> Friends
);

public record FriendRequestsResponseDto(
    List<FriendRequestDto> Requests
);
