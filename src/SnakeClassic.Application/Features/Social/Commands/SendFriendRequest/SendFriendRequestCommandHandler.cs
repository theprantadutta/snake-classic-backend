using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Social.Commands.SendFriendRequest;

public class SendFriendRequestCommandHandler
    : IRequestHandler<SendFriendRequestCommand, Result<SendFriendRequestResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SendFriendRequestCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SendFriendRequestResultDto>> Handle(
        SendFriendRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<SendFriendRequestResultDto>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;

        // Resolve friend ID from either FriendUserId or FriendUsername
        Guid friendId;
        if (request.FriendUserId.HasValue)
        {
            friendId = request.FriendUserId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(request.FriendUsername))
        {
            var friend = await _context.Users
                .FirstOrDefaultAsync(u => u.Username != null &&
                    u.Username.ToLower() == request.FriendUsername.ToLower(), cancellationToken);
            if (friend == null)
            {
                return Result<SendFriendRequestResultDto>.NotFound("User not found");
            }
            friendId = friend.Id;
        }
        else
        {
            return Result<SendFriendRequestResultDto>.Failure("Either friend_user_id or friend_username must be provided");
        }

        if (userId == friendId)
        {
            return Result<SendFriendRequestResultDto>.Failure("Cannot send friend request to yourself");
        }

        // Check if friend exists
        var friendExists = await _context.Users.AnyAsync(u => u.Id == friendId, cancellationToken);
        if (!friendExists)
        {
            return Result<SendFriendRequestResultDto>.NotFound("User not found");
        }

        // Check for existing friendship
        var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.UserId == friendId && f.FriendId == userId), cancellationToken);

        if (existingFriendship != null)
        {
            return existingFriendship.Status switch
            {
                FriendshipStatus.Accepted => Result<SendFriendRequestResultDto>.Failure("Already friends"),
                FriendshipStatus.Pending => Result<SendFriendRequestResultDto>.Failure("Friend request already pending"),
                FriendshipStatus.Blocked => Result<SendFriendRequestResultDto>.Failure("Cannot send friend request"),
                _ => Result<SendFriendRequestResultDto>.Failure("Friendship already exists")
            };
        }

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = friendId,
            Status = FriendshipStatus.Pending
        };

        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SendFriendRequestResultDto>.Success(
            new SendFriendRequestResultDto(true, "Friend request sent"));
    }
}
