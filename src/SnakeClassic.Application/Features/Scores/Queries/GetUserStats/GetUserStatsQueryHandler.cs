using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Scores.DTOs;

namespace SnakeClassic.Application.Features.Scores.Queries.GetUserStats;

public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, Result<UserStatsDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserStatsQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserStatsDto>> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UserStatsDto>.Unauthorized();
        }

        var scores = await _context.Scores
            .AsNoTracking()
            .Where(s => s.UserId == _currentUser.UserId.Value)
            .ToListAsync(cancellationToken);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);

        if (user == null)
        {
            return Result<UserStatsDto>.NotFound("User not found");
        }

        var totalGamesPlayed = scores.Count;
        var highScore = user.HighScore;
        var totalScore = scores.Sum(s => s.ScoreValue);
        var totalFoodsEaten = scores.Sum(s => s.FoodsEaten);
        var totalPlayTimeSeconds = scores.Sum(s => s.GameDurationSeconds);
        var averageScore = totalGamesPlayed > 0 ? (double)totalScore / totalGamesPlayed : 0;

        // Calculate streaks (simplified - consecutive days with games)
        var currentStreak = 0;
        var bestStreak = 0;

        if (scores.Any())
        {
            var gameDates = scores
                .Select(s => s.CreatedAt.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            if (gameDates.Contains(today) || gameDates.Contains(yesterday))
            {
                currentStreak = 1;
                var checkDate = gameDates.Contains(today) ? today : yesterday;

                for (int i = 1; i < gameDates.Count; i++)
                {
                    var prevDate = checkDate.AddDays(-i);
                    if (gameDates.Contains(prevDate))
                    {
                        currentStreak++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Calculate best streak
            var streak = 1;
            for (int i = 1; i < gameDates.Count; i++)
            {
                if ((gameDates[i - 1] - gameDates[i]).Days == 1)
                {
                    streak++;
                    bestStreak = Math.Max(bestStreak, streak);
                }
                else
                {
                    streak = 1;
                }
            }
            bestStreak = Math.Max(bestStreak, streak);
        }

        return Result<UserStatsDto>.Success(new UserStatsDto(
            TotalGamesPlayed: totalGamesPlayed,
            HighScore: highScore,
            TotalScore: totalScore,
            TotalFoodsEaten: totalFoodsEaten,
            TotalPlayTimeSeconds: totalPlayTimeSeconds,
            AverageScore: Math.Round(averageScore, 2),
            CurrentStreak: currentStreak,
            BestStreak: bestStreak
        ));
    }
}
