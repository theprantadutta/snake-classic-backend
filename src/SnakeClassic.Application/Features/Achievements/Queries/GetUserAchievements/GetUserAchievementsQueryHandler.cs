using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Achievements.DTOs;

namespace SnakeClassic.Application.Features.Achievements.Queries.GetUserAchievements;

public class GetUserAchievementsQueryHandler : IRequestHandler<GetUserAchievementsQuery, Result<List<UserAchievementDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserAchievementsQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<UserAchievementDto>>> Handle(GetUserAchievementsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<List<UserAchievementDto>>.Unauthorized();
        }

        var userAchievements = await _context.UserAchievements
            .AsNoTracking()
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == _currentUser.UserId.Value)
            .Select(ua => new UserAchievementDto(
                ua.Id,
                new AchievementDto(
                    ua.Achievement.Id,
                    ua.Achievement.AchievementId,
                    ua.Achievement.Name,
                    ua.Achievement.Description,
                    ua.Achievement.Category,
                    ua.Achievement.Tier,
                    ua.Achievement.Icon,
                    ua.Achievement.XpReward,
                    ua.Achievement.CoinReward,
                    ua.Achievement.RequirementType,
                    ua.Achievement.RequirementValue
                ),
                ua.CurrentProgress,
                ua.IsUnlocked,
                ua.UnlockedAt,
                ua.RewardClaimed
            ))
            .ToListAsync(cancellationToken);

        return Result<List<UserAchievementDto>>.Success(userAchievements);
    }
}
