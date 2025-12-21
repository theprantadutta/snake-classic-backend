using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Purchases.DTOs;

namespace SnakeClassic.Application.Features.Purchases.Queries.GetPremiumContent;

public class GetPremiumContentQueryHandler
    : IRequestHandler<GetPremiumContentQuery, Result<PremiumContentDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    public GetPremiumContentQueryHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<PremiumContentDto>> Handle(
        GetPremiumContentQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<PremiumContentDto>.Unauthorized();
        }

        var premiumContent = await _context.UserPremiumContents
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.UserId == _currentUser.UserId.Value, cancellationToken);

        // Check for active subscription
        var hasActiveSubscription = await _context.Purchases
            .AnyAsync(p =>
                p.UserId == _currentUser.UserId.Value &&
                p.IsSubscription &&
                p.IsVerified &&
                (p.ExpiresAt == null || p.ExpiresAt > _dateTime.UtcNow), cancellationToken);

        return Result<PremiumContentDto>.Success(new PremiumContentDto(
            PremiumTier: premiumContent?.PremiumTier ?? "free",
            OwnedThemes: premiumContent?.OwnedThemes ?? new List<string>(),
            OwnedPowerups: premiumContent?.OwnedPowerups ?? new List<string>(),
            OwnedCosmetics: premiumContent?.OwnedCosmetics ?? new List<string>(),
            IsSubscriptionActive: hasActiveSubscription
        ));
    }
}
