using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Social.Commands.AcceptFriendRequest;

public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, Result<bool>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    public AcceptFriendRequestCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<bool>> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<bool>.Unauthorized();
        }

        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                f.Id == request.RequestId &&
                f.FriendId == _currentUser.UserId.Value &&
                f.Status == FriendshipStatus.Pending, cancellationToken);

        if (friendship == null)
        {
            return Result<bool>.NotFound("Friend request not found");
        }

        friendship.Status = FriendshipStatus.Accepted;
        friendship.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
