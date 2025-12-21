namespace SnakeClassic.Application.Features.Notifications.DTOs;

public record SendNotificationResultDto(
    bool Success,
    string? MessageId,
    string? ErrorMessage
);

public record SubscribeTopicResultDto(bool Success, string Topic);
