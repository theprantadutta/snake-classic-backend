using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Social.DTOs;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Social.Queries.GetFriends;

public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, Result<List<FriendDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFriendsQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FriendDto>>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<List<FriendDto>>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;

        var friendships = await _context.Friendships
            .AsNoTracking()
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                       (f.UserId == userId || f.FriendId == userId))
            .ToListAsync(cancellationToken);

        var friends = friendships.Select(f =>
        {
            var friend = f.UserId == userId ? f.Friend : f.User;
            return new FriendDto(
                friend.Id,
                friend.Username,
                friend.DisplayName,
                friend.PhotoUrl,
                friend.Status,
                friend.HighScore,
                friend.Level,
                f.UpdatedAt
            );
        }).OrderBy(f => f.DisplayName ?? f.Username).ToList();

        return Result<List<FriendDto>>.Success(friends);
    }
}
