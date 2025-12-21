using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.BattlePass.DTOs;

namespace SnakeClassic.Application.Features.BattlePass.Commands.ClaimReward;

public class ClaimBattlePassRewardCommandHandler
    : IRequestHandler<ClaimBattlePassRewardCommand, Result<ClaimRewardResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ClaimBattlePassRewardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ClaimRewardResultDto>> Handle(
        ClaimBattlePassRewardCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ClaimRewardResultDto>.Unauthorized();
        }

        var season = await _context.BattlePassSeasons
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (season == null)
        {
            return Result<ClaimRewardResultDto>.Failure("No active battle pass season");
        }

        var progress = await _context.UserBattlePassProgresses
            .FirstOrDefaultAsync(p =>
                p.UserId == _currentUser.UserId.Value &&
                p.SeasonId == season.Id, cancellationToken);

        if (progress == null)
        {
            return Result<ClaimRewardResultDto>.Failure("No battle pass progress found");
        }

        if (progress.CurrentLevel < request.Level)
        {
            return Result<ClaimRewardResultDto>.Failure("Level not reached yet");
        }

        if (request.IsPremium && !progress.HasPremium)
        {
            return Result<ClaimRewardResultDto>.Failure("Premium pass required");
        }

        var claimedList = request.IsPremium ? progress.ClaimedPremiumRewards : progress.ClaimedFreeRewards;
        if (claimedList.Contains(request.Level))
        {
            return Result<ClaimRewardResultDto>.Failure("Reward already claimed");
        }

        // Award coins (simplified - in real app, check LevelsConfig for actual rewards)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);
        var coinReward = request.IsPremium ? 100 : 50;
        if (user != null)
        {
            user.Coins += coinReward;
        }

        claimedList.Add(request.Level);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ClaimRewardResultDto>.Success(new ClaimRewardResultDto(
            Success: true,
            RewardType: "coins",
            RewardAmount: coinReward,
            Message: $"Claimed {coinReward} coins for level {request.Level}"
        ));
    }
}
