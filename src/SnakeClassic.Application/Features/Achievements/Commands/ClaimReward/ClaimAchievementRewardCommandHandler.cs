using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Application.Features.Achievements.Commands.ClaimReward;

public class ClaimAchievementRewardCommandHandler
    : IRequestHandler<ClaimAchievementRewardCommand, Result<ClaimRewardResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ClaimAchievementRewardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ClaimRewardResultDto>> Handle(
        ClaimAchievementRewardCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ClaimRewardResultDto>.Unauthorized();
        }

        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.AchievementId == request.AchievementId, cancellationToken);

        if (achievement == null)
        {
            return Result<ClaimRewardResultDto>.NotFound("Achievement not found");
        }

        var userAchievement = await _context.UserAchievements
            .FirstOrDefaultAsync(ua =>
                ua.UserId == _currentUser.UserId.Value &&
                ua.AchievementId == achievement.Id, cancellationToken);

        if (userAchievement == null || !userAchievement.IsUnlocked)
        {
            return Result<ClaimRewardResultDto>.Failure("Achievement not unlocked");
        }

        if (userAchievement.RewardClaimed)
        {
            return Result<ClaimRewardResultDto>.Failure("Reward already claimed");
        }

        // Award coins to user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);
        if (user != null)
        {
            user.Coins += achievement.CoinReward;
        }

        userAchievement.RewardClaimed = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ClaimRewardResultDto>.Success(new ClaimRewardResultDto(
            Success: true,
            XpAwarded: achievement.XpReward,
            CoinsAwarded: achievement.CoinReward
        ));
    }
}
