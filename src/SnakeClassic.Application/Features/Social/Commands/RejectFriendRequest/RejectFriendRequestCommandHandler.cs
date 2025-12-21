using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Social.Commands.RejectFriendRequest;

public class RejectFriendRequestCommandHandler : IRequestHandler<RejectFriendRequestCommand, Result<bool>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RejectFriendRequestCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(RejectFriendRequestCommand request, CancellationToken cancellationToken)
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

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
