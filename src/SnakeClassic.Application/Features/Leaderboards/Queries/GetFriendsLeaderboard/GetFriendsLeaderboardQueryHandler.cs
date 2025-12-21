using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Leaderboards.DTOs;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Leaderboards.Queries.GetFriendsLeaderboard;

public class GetFriendsLeaderboardQueryHandler : IRequestHandler<GetFriendsLeaderboardQuery, Result<LeaderboardResponseDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFriendsLeaderboardQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<LeaderboardResponseDto>> Handle(GetFriendsLeaderboardQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<LeaderboardResponseDto>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;

        // Get friend IDs (accepted friendships)
        var friendIds = await _context.Friendships
            .AsNoTracking()
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                       (f.UserId == userId || f.FriendId == userId))
            .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
            .ToListAsync(cancellationToken);

        // Include current user in leaderboard
        friendIds.Add(userId);

        var entries = await _context.Users
            .AsNoTracking()
            .Where(u => friendIds.Contains(u.Id))
            .OrderByDescending(u => u.HighScore)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var leaderboardEntries = entries.Select((u, index) => new LeaderboardEntryDto(
            index + 1,
            u.Id,
            u.Username,
            u.DisplayName,
            u.PhotoUrl,
            u.HighScore,
            u.Level,
            null
        )).ToList();

        var currentUserRank = leaderboardEntries.FindIndex(e => e.UserId == userId) + 1;

        return Result<LeaderboardResponseDto>.Success(new LeaderboardResponseDto(
            leaderboardEntries,
            currentUserRank > 0 ? currentUserRank : null,
            friendIds.Count
        ));
    }
}
