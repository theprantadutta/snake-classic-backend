using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.DailyBonus.DTOs;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.DailyBonus.Commands.ClaimDailyBonus;

public class ClaimDailyBonusCommandHandler : IRequestHandler<ClaimDailyBonusCommand, Result<ClaimDailyBonusResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    // Weekly rewards configuration (same as in query handler)
    private static readonly (int coins, string? bonusItem)[] WeeklyRewards = new[]
    {
        (10, (string?)null),           // Day 1
        (15, (string?)null),           // Day 2
        (20, "Speed Boost"),           // Day 3
        (25, (string?)null),           // Day 4
        (30, "2x XP Boost"),           // Day 5
        (40, (string?)null),           // Day 6
        (50, "Premium Theme")          // Day 7
    };

    public ClaimDailyBonusCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<ClaimDailyBonusResultDto>> Handle(ClaimDailyBonusCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ClaimDailyBonusResultDto>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var tomorrow = today.AddDays(1);
        var nextClaimAt = tomorrow.ToDateTime(TimeOnly.MinValue);

        var bonus = await _context.DailyLoginBonuses
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result<ClaimDailyBonusResultDto>.NotFound("User not found");
        }

        int newStreak;
        int rewardDay;

        if (bonus == null)
        {
            // First time claiming
            newStreak = 1;
            rewardDay = 1;

            bonus = new DailyLoginBonus
            {
                UserId = userId,
                CurrentStreak = newStreak,
                LastClaimDate = today,
                TotalClaims = 1
            };
            _context.DailyLoginBonuses.Add(bonus);
        }
        else
        {
            // Check if already claimed today
            if (bonus.LastClaimDate == today)
            {
                return Result<ClaimDailyBonusResultDto>.Failure(
                    "You have already claimed today's bonus", 400);
            }

            // Calculate new streak
            if (bonus.LastClaimDate == today.AddDays(-1))
            {
                // Claimed yesterday - continue streak
                newStreak = bonus.CurrentStreak + 1;
            }
            else
            {
                // Missed a day - reset streak
                newStreak = 1;
            }

            // Update bonus record
            bonus.CurrentStreak = newStreak;
            bonus.LastClaimDate = today;
            bonus.TotalClaims++;
            bonus.UpdatedAt = _dateTime.UtcNow;

            rewardDay = ((newStreak - 1) % 7) + 1;
        }

        // Get reward for current day
        var (coins, bonusItem) = WeeklyRewards[rewardDay - 1];

        // Add coins to user balance
        user.Coins += coins;

        await _context.SaveChangesAsync(cancellationToken);

        var reward = new TodayRewardDto(rewardDay, coins, bonusItem);

        return Result<ClaimDailyBonusResultDto>.Success(new ClaimDailyBonusResultDto(
            true,
            reward,
            user.Coins,
            newStreak,
            nextClaimAt
        ));
    }
}
