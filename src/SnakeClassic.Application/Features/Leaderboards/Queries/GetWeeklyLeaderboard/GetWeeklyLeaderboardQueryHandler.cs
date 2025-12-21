using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Leaderboards.DTOs;

namespace SnakeClassic.Application.Features.Leaderboards.Queries.GetWeeklyLeaderboard;

public class GetWeeklyLeaderboardQueryHandler : IRequestHandler<GetWeeklyLeaderboardQuery, Result<LeaderboardResponseDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    public GetWeeklyLeaderboardQueryHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<LeaderboardResponseDto>> Handle(GetWeeklyLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var now = _dateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;

        var weeklyScores = await _context.Scores
            .AsNoTracking()
            .Where(s => s.CreatedAt >= startOfWeek)
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                BestScore = g.Max(s => s.ScoreValue),
                AchievedAt = g.OrderByDescending(s => s.ScoreValue).First().CreatedAt
            })
            .OrderByDescending(x => x.BestScore)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var userIds = weeklyScores.Select(x => x.UserId).ToList();
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var entries = weeklyScores.Select((x, index) => new LeaderboardEntryDto(
            request.Offset + index + 1,
            x.UserId,
            users.TryGetValue(x.UserId, out var user) ? user.Username : null,
            user?.DisplayName,
            user?.PhotoUrl,
            x.BestScore,
            user?.Level ?? 1,
            x.AchievedAt
        )).ToList();

        var totalPlayers = await _context.Scores
            .Where(s => s.CreatedAt >= startOfWeek)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        int? currentUserRank = null;
        if (_currentUser.IsAuthenticated && _currentUser.UserId.HasValue)
        {
            var userBestScore = await _context.Scores
                .Where(s => s.UserId == _currentUser.UserId.Value && s.CreatedAt >= startOfWeek)
                .MaxAsync(s => (int?)s.ScoreValue, cancellationToken);

            if (userBestScore.HasValue)
            {
                var playersAbove = await _context.Scores
                    .Where(s => s.CreatedAt >= startOfWeek)
                    .GroupBy(s => s.UserId)
                    .CountAsync(g => g.Max(s => s.ScoreValue) > userBestScore.Value, cancellationToken);
                currentUserRank = playersAbove + 1;
            }
        }

        return Result<LeaderboardResponseDto>.Success(new LeaderboardResponseDto(
            entries,
            currentUserRank,
            totalPlayers
        ));
    }
}
