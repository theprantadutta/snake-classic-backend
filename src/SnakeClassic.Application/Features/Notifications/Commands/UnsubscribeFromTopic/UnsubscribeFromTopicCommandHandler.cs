using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Notifications.DTOs;

namespace SnakeClassic.Application.Features.Notifications.Commands.UnsubscribeFromTopic;

public class UnsubscribeFromTopicCommandHandler
    : IRequestHandler<UnsubscribeFromTopicCommand, Result<SubscribeTopicResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly IFirebaseMessagingService _messaging;
    private readonly ICurrentUserService _currentUser;

    public UnsubscribeFromTopicCommandHandler(
        IAppDbContext context,
        IFirebaseMessagingService messaging,
        ICurrentUserService currentUser)
    {
        _context = context;
        _messaging = messaging;
        _currentUser = currentUser;
    }

    public async Task<Result<SubscribeTopicResultDto>> Handle(
        UnsubscribeFromTopicCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<SubscribeTopicResultDto>.Unauthorized();
        }

        try
        {
            await _messaging.UnsubscribeFromTopicAsync(request.Token, request.Topic);

            // Update token's subscribed topics
            var fcmToken = await _context.FcmTokens
                .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

            if (fcmToken != null)
            {
                fcmToken.SubscribedTopics.Remove(request.Topic);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result<SubscribeTopicResultDto>.Success(
                new SubscribeTopicResultDto(true, request.Topic));
        }
        catch (Exception ex)
        {
            return Result<SubscribeTopicResultDto>.Failure(ex.Message);
        }
    }
}
