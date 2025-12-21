using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.Users.Commands.RegisterFcmToken;

public class RegisterFcmTokenCommandHandler : IRequestHandler<RegisterFcmTokenCommand, Result<bool>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    public RegisterFcmTokenCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<bool>> Handle(RegisterFcmTokenCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<bool>.Unauthorized();
        }

        // Check if token already exists
        var existingToken = await _context.FcmTokens
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (existingToken != null)
        {
            // Update existing token's user and last used time
            existingToken.UserId = _currentUser.UserId.Value;
            existingToken.Platform = request.Platform;
            existingToken.LastUsedAt = _dateTime.UtcNow;
            if (request.SubscribedTopics != null)
            {
                existingToken.SubscribedTopics = request.SubscribedTopics;
            }
        }
        else
        {
            // Create new token
            var fcmToken = new FcmToken
            {
                UserId = _currentUser.UserId.Value,
                Token = request.Token,
                Platform = request.Platform,
                SubscribedTopics = request.SubscribedTopics ?? new List<string>(),
                LastUsedAt = _dateTime.UtcNow
            };
            _context.FcmTokens.Add(fcmToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
