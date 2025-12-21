using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.BattlePass.DTOs;

namespace SnakeClassic.Application.Features.BattlePass.Queries.GetProgress;

public class GetBattlePassProgressQueryHandler
    : IRequestHandler<GetBattlePassProgressQuery, Result<UserBattlePassProgressDto?>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    public GetBattlePassProgressQueryHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<UserBattlePassProgressDto?>> Handle(
        GetBattlePassProgressQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UserBattlePassProgressDto?>.Unauthorized();
        }

        var season = await _context.BattlePassSeasons
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (season == null)
        {
            return Result<UserBattlePassProgressDto?>.Success(null);
        }

        var progress = await _context.UserBattlePassProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.UserId == _currentUser.UserId.Value &&
                p.SeasonId == season.Id, cancellationToken);

        // Calculate XP needed for next level (example: 100 base + 50 per level)
        var xpToNextLevel = progress != null
            ? 100 + (progress.CurrentLevel * 50)
            : 100;

        var daysRemaining = (season.EndDate - _dateTime.UtcNow).Days;
        if (daysRemaining < 0) daysRemaining = 0;

        return Result<UserBattlePassProgressDto?>.Success(new UserBattlePassProgressDto(
            SeasonId: season.Id,
            SeasonName: season.Name,
            HasPremium: progress?.HasPremium ?? false,
            CurrentLevel: progress?.CurrentLevel ?? 0,
            CurrentXp: progress?.CurrentXp ?? 0,
            XpToNextLevel: xpToNextLevel,
            ClaimedFreeRewards: progress?.ClaimedFreeRewards ?? new List<int>(),
            ClaimedPremiumRewards: progress?.ClaimedPremiumRewards ?? new List<int>(),
            DaysRemaining: daysRemaining
        ));
    }
}
