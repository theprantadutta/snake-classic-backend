using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.DailyChallenges.DTOs;

namespace SnakeClassic.Application.Features.DailyChallenges.Commands.ClaimReward;

public class ClaimChallengeRewardCommandHandler : IRequestHandler<ClaimChallengeRewardCommand, ClaimRewardResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ClaimChallengeRewardCommandHandler> _logger;

    public ClaimChallengeRewardCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUserService,
        ILogger<ClaimChallengeRewardCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ClaimRewardResponse> Handle(ClaimChallengeRewardCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return new ClaimRewardResponse(
                Success: false,
                CoinsEarned: 0,
                XpEarned: 0,
                BonusCoins: 0,
                Message: "User not authenticated"
            );
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get the user's challenge progress
        var userChallenge = await _context.UserDailyChallenges
            .Include(uc => uc.Challenge)
            .FirstOrDefaultAsync(uc =>
                uc.UserId == userId.Value &&
                uc.ChallengeId == request.ChallengeId,
                cancellationToken);

        if (userChallenge == null)
        {
            return new ClaimRewardResponse(
                Success: false,
                CoinsEarned: 0,
                XpEarned: 0,
                BonusCoins: 0,
                Message: "Challenge progress not found"
            );
        }

        // Validate the challenge is from today
        if (userChallenge.Challenge.ChallengeDate != today)
        {
            return new ClaimRewardResponse(
                Success: false,
                CoinsEarned: 0,
                XpEarned: 0,
                BonusCoins: 0,
                Message: "Challenge has expired"
            );
        }

        if (!userChallenge.IsCompleted)
        {
            return new ClaimRewardResponse(
                Success: false,
                CoinsEarned: 0,
                XpEarned: 0,
                BonusCoins: 0,
                Message: "Challenge not completed yet"
            );
        }

        if (userChallenge.ClaimedReward)
        {
            return new ClaimRewardResponse(
                Success: false,
                CoinsEarned: 0,
                XpEarned: 0,
                BonusCoins: 0,
                Message: "Reward already claimed"
            );
        }

        // Mark as claimed
        userChallenge.ClaimedReward = true;
        userChallenge.UpdatedAt = DateTime.UtcNow;

        // Add coins to user
        var user = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
        if (user != null)
        {
            user.Coins += userChallenge.Challenge.CoinReward;
            user.UpdatedAt = DateTime.UtcNow;
        }

        // Check if all challenges completed for bonus
        var allTodaysChallengeIds = await _context.DailyChallenges
            .Where(c => c.ChallengeDate == today)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var userChallengesForToday = await _context.UserDailyChallenges
            .Where(uc => uc.UserId == userId.Value && allTodaysChallengeIds.Contains(uc.ChallengeId))
            .ToListAsync(cancellationToken);

        var allCompleted = userChallengesForToday.Count == allTodaysChallengeIds.Count &&
                          userChallengesForToday.All(uc => uc.IsCompleted);
        var allClaimed = userChallengesForToday.All(uc => uc.ClaimedReward);

        // Give bonus if this is the last claim and all are completed
        var bonusCoins = 0;
        if (allCompleted && allClaimed && user != null)
        {
            bonusCoins = 25;
            user.Coins += bonusCoins;
            _logger.LogInformation(
                "User {UserId} completed all daily challenges! Bonus: {Bonus} coins",
                userId.Value, bonusCoins);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} claimed reward for challenge {ChallengeId}: {Coins} coins, {Xp} XP",
            userId.Value, request.ChallengeId,
            userChallenge.Challenge.CoinReward, userChallenge.Challenge.XpReward);

        return new ClaimRewardResponse(
            Success: true,
            CoinsEarned: userChallenge.Challenge.CoinReward,
            XpEarned: userChallenge.Challenge.XpReward,
            BonusCoins: bonusCoins,
            Message: bonusCoins > 0 ? "All challenges completed! Bonus awarded!" : null
        );
    }
}
