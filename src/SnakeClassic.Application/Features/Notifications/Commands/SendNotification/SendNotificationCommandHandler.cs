using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Notifications.DTOs;

namespace SnakeClassic.Application.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommandHandler
    : IRequestHandler<SendNotificationCommand, Result<SendNotificationResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly IFirebaseMessagingService _messaging;

    public SendNotificationCommandHandler(IAppDbContext context, IFirebaseMessagingService messaging)
    {
        _context = context;
        _messaging = messaging;
    }

    public async Task<Result<SendNotificationResultDto>> Handle(
        SendNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var payload = new NotificationPayload
        {
            Title = request.Title,
            Body = request.Body,
            ImageUrl = request.ImageUrl,
            Priority = request.Priority,
            Route = request.Route,
            Data = request.Data
        };

        try
        {
            string? messageId = null;

            if (!string.IsNullOrEmpty(request.Topic))
            {
                messageId = await _messaging.SendToTopicAsync(payload, request.Topic);
            }
            else if (request.UserId.HasValue)
            {
                // Get user's FCM tokens
                var tokens = await _context.FcmTokens
                    .Where(t => t.UserId == request.UserId.Value)
                    .Select(t => t.Token)
                    .ToListAsync(cancellationToken);

                if (tokens.Any())
                {
                    var result = await _messaging.SendMulticastAsync(payload, tokens);
                    messageId = $"sent_to_{result.SuccessCount}_devices";
                }
                else
                {
                    return Result<SendNotificationResultDto>.Failure("User has no registered devices");
                }
            }
            else
            {
                return Result<SendNotificationResultDto>.Failure("Either userId or topic must be specified");
            }

            return Result<SendNotificationResultDto>.Success(new SendNotificationResultDto(
                Success: true,
                MessageId: messageId,
                ErrorMessage: null
            ));
        }
        catch (Exception ex)
        {
            return Result<SendNotificationResultDto>.Failure(ex.Message);
        }
    }
}
