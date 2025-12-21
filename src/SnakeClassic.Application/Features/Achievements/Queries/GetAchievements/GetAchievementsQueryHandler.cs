using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Achievements.DTOs;

namespace SnakeClassic.Application.Features.Achievements.Queries.GetAchievements;

public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, Result<List<AchievementDto>>>
{
    private readonly IAppDbContext _context;

    public GetAchievementsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AchievementDto>>> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var achievements = await _context.Achievements
            .AsNoTracking()
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Tier)
            .Select(a => new AchievementDto(
                a.Id,
                a.AchievementId,
                a.Name,
                a.Description,
                a.Category,
                a.Tier,
                a.Icon,
                a.XpReward,
                a.CoinReward,
                a.RequirementType,
                a.RequirementValue
            ))
            .ToListAsync(cancellationToken);

        return Result<List<AchievementDto>>.Success(achievements);
    }
}
