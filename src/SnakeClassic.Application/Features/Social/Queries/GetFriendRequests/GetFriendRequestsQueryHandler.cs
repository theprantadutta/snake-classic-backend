using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Social.DTOs;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Social.Queries.GetFriendRequests;

public class GetFriendRequestsQueryHandler : IRequestHandler<GetFriendRequestsQuery, Result<FriendRequestsResponseDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFriendRequestsQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<FriendRequestsResponseDto>> Handle(GetFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<FriendRequestsResponseDto>.Unauthorized();
        }

        // Fetch data first, then project in memory
        var friendships = await _context.Friendships
            .AsNoTracking()
            .Include(f => f.User)
            .Where(f => f.FriendId == _currentUser.UserId.Value && f.Status == FriendshipStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        var pendingRequests = friendships.Select(f => new FriendRequestDto(
            f.Id,
            f.UserId,
            f.User.Username,
            f.User.DisplayName,
            f.User.PhotoUrl,
            f.CreatedAt
        )).ToList();

        return Result<FriendRequestsResponseDto>.Success(new FriendRequestsResponseDto(pendingRequests));
    }
}
