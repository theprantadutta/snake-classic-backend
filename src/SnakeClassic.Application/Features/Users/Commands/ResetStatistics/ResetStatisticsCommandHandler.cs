using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Application.Features.Users.Commands.ResetStatistics;

public class ResetStatisticsCommandHandler : IRequestHandler<ResetStatisticsCommand, Result<ResetStatisticsResult>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ResetStatisticsCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ResetStatisticsResult>> Handle(ResetStatisticsCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ResetStatisticsResult>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;

        // Get the user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result<ResetStatisticsResult>.NotFound("User not found");
        }

        // Delete all user's scores
        var userScores = await _context.Scores
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

        var scoresDeleted = userScores.Count;
        _context.Scores.RemoveRange(userScores);

        // Reset user's game statistics
        user.HighScore = 0;
        user.TotalScore = 0;
        user.TotalGamesPlayed = 0;

        // Reset user achievements (progress only, not unlocked status for now)
        var userAchievements = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var achievement in userAchievements)
        {
            achievement.CurrentProgress = 0;
            achievement.IsUnlocked = false;
            achievement.UnlockedAt = null;
            achievement.RewardClaimed = false;
            achievement.ClaimedAt = null;
            achievement.UpdatedAt = DateTime.UtcNow;
        }

        var achievementsReset = userAchievements.Count;

        // Reset user daily challenges
        var userDailyChallenges = await _context.UserDailyChallenges
            .Where(udc => udc.UserId == userId)
            .ToListAsync(cancellationToken);

        var dailyChallengesReset = userDailyChallenges.Count;
        _context.UserDailyChallenges.RemoveRange(userDailyChallenges);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ResetStatisticsResult>.Success(new ResetStatisticsResult(
            Success: true,
            ScoresDeleted: scoresDeleted,
            DailyChallengesReset: dailyChallengesReset,
            AchievementsReset: achievementsReset
        ));
    }
}
