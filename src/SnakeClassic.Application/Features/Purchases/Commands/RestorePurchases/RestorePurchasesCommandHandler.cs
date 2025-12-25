using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.Purchases.Commands.RestorePurchases;

public class RestorePurchasesCommandHandler
    : IRequestHandler<RestorePurchasesCommand, Result<RestorePurchasesResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RestorePurchasesCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<RestorePurchasesResultDto>> Handle(
        RestorePurchasesCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<RestorePurchasesResultDto>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;
        var restoredProducts = new List<string>();

        // Get all verified purchases for this user
        var purchases = await _context.Purchases
            .Where(p => p.UserId == userId && p.IsVerified)
            .ToListAsync(cancellationToken);

        if (!purchases.Any())
        {
            return Result<RestorePurchasesResultDto>.Success(new RestorePurchasesResultDto(
                Success: true,
                RestoredCount: 0,
                RestoredProducts: restoredProducts,
                Message: "No previous purchases found"
            ));
        }

        // Get or create premium content record
        var premiumContent = await _context.UserPremiumContents
            .FirstOrDefaultAsync(pc => pc.UserId == userId, cancellationToken);

        if (premiumContent == null)
        {
            premiumContent = new UserPremiumContent { UserId = userId };
            _context.UserPremiumContents.Add(premiumContent);
        }

        // Restore each purchase
        foreach (var purchase in purchases)
        {
            // Skip expired subscriptions
            if (purchase.IsSubscription && purchase.ExpiresAt.HasValue && purchase.ExpiresAt.Value < DateTime.UtcNow)
            {
                continue;
            }

            // Restore content based on what was originally unlocked
            if (purchase.ContentUnlocked != null)
            {
                foreach (var content in purchase.ContentUnlocked)
                {
                    // Skip coin grants on restore (consumables shouldn't be re-granted)
                    if (content.StartsWith("coins:"))
                    {
                        continue;
                    }

                    // Skip tournament entries on restore (consumables)
                    if (content.StartsWith("tournament_entry:"))
                    {
                        continue;
                    }

                    RestoreContent(premiumContent, purchase.ProductId, content);
                    if (!restoredProducts.Contains(content))
                    {
                        restoredProducts.Add(content);
                    }
                }
            }
        }

        premiumContent.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RestorePurchasesResultDto>.Success(new RestorePurchasesResultDto(
            Success: true,
            RestoredCount: restoredProducts.Count,
            RestoredProducts: restoredProducts,
            Message: restoredProducts.Any()
                ? $"Successfully restored {restoredProducts.Count} items"
                : "All purchases already applied"
        ));
    }

    private static void RestoreContent(UserPremiumContent premiumContent, string productId, string content)
    {
        // Theme restoration
        if (content.EndsWith("_theme") || productId.EndsWith("_theme"))
        {
            if (!premiumContent.OwnedThemes.Contains(content))
            {
                premiumContent.OwnedThemes.Add(content);
            }
        }
        // Power-up pack restoration
        else if (content.Contains("powerups") || content.Contains("powerup"))
        {
            if (!premiumContent.OwnedPowerups.Contains(content))
            {
                premiumContent.OwnedPowerups.Add(content);
            }
        }
        // Subscription restoration
        else if (content == "premium_subscription")
        {
            premiumContent.PremiumTier = "premium";
            premiumContent.SubscriptionActive = true;
        }
        // Battle pass restoration
        else if (content == "battle_pass")
        {
            premiumContent.BattlePassActive = true;
        }
        // Cosmetics (skins and trails) restoration
        else
        {
            if (!premiumContent.OwnedCosmetics.Contains(content))
            {
                premiumContent.OwnedCosmetics.Add(content);
            }
        }
    }
}
