using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Notifications.Commands.SendNotification;
using SnakeClassic.Application.Features.Notifications.Commands.SubscribeToTopic;
using SnakeClassic.Application.Features.Notifications.Commands.UnsubscribeFromTopic;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class NotificationsController : BaseApiController
{
    [HttpPost("topics/subscribe")]
    public async Task<ActionResult> SubscribeToTopic([FromBody] TopicSubscriptionRequest request)
    {
        var result = await Mediator.Send(new SubscribeToTopicCommand(request.FcmToken, request.Topic));
        return HandleResult(result);
    }

    [HttpPost("topics/unsubscribe")]
    public async Task<ActionResult> UnsubscribeFromTopic([FromBody] TopicSubscriptionRequest request)
    {
        var result = await Mediator.Send(new UnsubscribeFromTopicCommand(request.FcmToken, request.Topic));
        return HandleResult(result);
    }

    [HttpPost("send-individual")]
    public async Task<ActionResult> SendIndividualNotification([FromBody] SendIndividualNotificationRequest request)
    {
        var command = new SendNotificationCommand(
            request.UserId,
            null,
            request.Title,
            request.Body,
            request.ImageUrl,
            request.Priority ?? "normal",
            request.Route,
            request.Data
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("send-topic")]
    public async Task<ActionResult> SendTopicNotification([FromBody] SendTopicNotificationRequest request)
    {
        var command = new SendNotificationCommand(
            null,
            request.Topic,
            request.Title,
            request.Body,
            request.ImageUrl,
            request.Priority ?? "normal",
            request.Route,
            request.Data
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record TopicSubscriptionRequest(string FcmToken, string Topic);

public record SendIndividualNotificationRequest(
    Guid UserId,
    string Title,
    string Body,
    string? ImageUrl,
    string? Priority,
    string? Route,
    Dictionary<string, string>? Data
);

public record SendTopicNotificationRequest(
    string Topic,
    string Title,
    string Body,
    string? ImageUrl,
    string? Priority,
    string? Route,
    Dictionary<string, string>? Data
);
