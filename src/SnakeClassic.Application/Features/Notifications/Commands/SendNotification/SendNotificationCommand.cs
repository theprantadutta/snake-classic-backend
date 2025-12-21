using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Notifications.DTOs;

namespace SnakeClassic.Application.Features.Notifications.Commands.SendNotification;

public record SendNotificationCommand(
    Guid? UserId,
    string? Topic,
    string Title,
    string Body,
    string? ImageUrl,
    string Priority,
    string? Route,
    Dictionary<string, string>? Data
) : IRequest<Result<SendNotificationResultDto>>;
