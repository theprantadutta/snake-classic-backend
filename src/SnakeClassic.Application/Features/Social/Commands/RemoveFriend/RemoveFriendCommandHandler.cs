using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Social.Commands.RemoveFriend;

public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, Result<bool>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RemoveFriendCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<bool>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;

        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.UserId == userId && f.FriendId == request.FriendId) ||
                 (f.UserId == request.FriendId && f.FriendId == userId)), cancellationToken);

        if (friendship == null)
        {
            return Result<bool>.NotFound("Friendship not found");
        }

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
