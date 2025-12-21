using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.DailyBonus.DTOs;

namespace SnakeClassic.Application.Features.DailyBonus.Queries.GetDailyBonusStatus;

public class GetDailyBonusStatusQueryHandler : IRequestHandler<GetDailyBonusStatusQuery, Result<DailyBonusStatusDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    // Weekly rewards configuration
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

    public GetDailyBonusStatusQueryHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<DailyBonusStatusDto>> Handle(GetDailyBonusStatusQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<DailyBonusStatusDto>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);

        var bonus = await _context.DailyLoginBonuses
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        int currentStreak;
        bool canClaim;
        DateTime? lastClaimDateTime = null;

        if (bonus == null)
        {
            // New user - never claimed before
            currentStreak = 1;
            canClaim = true;
        }
        else
        {
            lastClaimDateTime = bonus.LastClaimDate.HasValue
                ? bonus.LastClaimDate.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            if (bonus.LastClaimDate == today)
            {
                // Already claimed today
                canClaim = false;
                currentStreak = bonus.CurrentStreak;
            }
            else if (bonus.LastClaimDate == today.AddDays(-1))
            {
                // Claimed yesterday - can continue streak
                canClaim = true;
                currentStreak = bonus.CurrentStreak + 1;
                if (currentStreak > 7) currentStreak = ((currentStreak - 1) % 7) + 1;
            }
            else
            {
                // Missed a day - reset streak
                canClaim = true;
                currentStreak = 1;
            }
        }

        // Calculate today's reward day (1-7 cycle)
        var rewardDay = ((currentStreak - 1) % 7) + 1;
        var (todayCoins, todayBonusItem) = WeeklyRewards[rewardDay - 1];

        var todayReward = new TodayRewardDto(rewardDay, todayCoins, todayBonusItem);

        // Build week rewards with claimed status
        var weekRewards = new List<DayRewardDto>();
        for (int day = 1; day <= 7; day++)
        {
            var (coins, bonusItem) = WeeklyRewards[day - 1];
            bool claimed;

            if (bonus == null)
            {
                claimed = false;
            }
            else if (bonus.LastClaimDate == today)
            {
                // Already claimed today
                claimed = day <= currentStreak;
            }
            else
            {
                // Haven't claimed today
                claimed = day < rewardDay;
            }

            weekRewards.Add(new DayRewardDto(day, coins, bonusItem, claimed));
        }

        return Result<DailyBonusStatusDto>.Success(new DailyBonusStatusDto(
            canClaim,
            currentStreak,
            lastClaimDateTime,
            todayReward,
            weekRewards
        ));
    }
}
