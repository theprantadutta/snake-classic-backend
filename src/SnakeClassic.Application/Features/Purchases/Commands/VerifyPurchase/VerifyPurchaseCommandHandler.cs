using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Purchases.DTOs;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.Purchases.Commands.VerifyPurchase;

public class VerifyPurchaseCommandHandler
    : IRequestHandler<VerifyPurchaseCommand, Result<VerifyPurchaseResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    // Product ID constants matching Flutter frontend ProductIds class
    private static class ProductIds
    {
        // Premium Themes
        public static readonly HashSet<string> Themes = new()
        {
            "crystal_theme", "cyberpunk_theme", "space_theme",
            "ocean_theme", "desert_theme", "forest_theme"
        };
        public const string ThemesBundle = "premium_themes_bundle";

        // Snake Coins (Consumable) - coin amounts
        public static readonly Dictionary<string, int> CoinPacks = new()
        {
            { "coin_pack_small", 100 },
            { "coin_pack_medium", 550 },   // 500 + 50 bonus
            { "coin_pack_large", 1400 },   // 1200 + 200 bonus
            { "coin_pack_mega", 3000 }     // 2500 + 500 bonus
        };

        // Premium Power-ups
        public static readonly HashSet<string> PowerupPacks = new()
        {
            "mega_powerups_pack", "exclusive_powerups_pack", "premium_powerups_bundle"
        };

        // Snake Skins (individual cosmetics)
        public static readonly HashSet<string> SnakeSkins = new()
        {
            "golden", "rainbow", "galaxy", "dragon", "electric",
            "fire", "ice", "shadow", "neon", "crystal", "cosmic"
        };

        // Trail Effects
        public static readonly HashSet<string> TrailEffects = new()
        {
            "trail_particle", "trail_glow", "trail_rainbow", "trail_fire",
            "trail_electric", "trail_star", "trail_cosmic", "trail_neon",
            "trail_shadow", "trail_crystal", "trail_dragon"
        };

        // Cosmetic Bundles - contains skins + trails
        public static readonly Dictionary<string, (string[] Skins, string[] Trails)> CosmeticBundles = new()
        {
            {
                "starter_pack",
                (new[] { "golden", "fire" }, new[] { "trail_particle", "trail_glow" })
            },
            {
                "elemental_pack",
                (new[] { "fire", "ice", "electric" }, new[] { "trail_fire", "trail_electric" })
            },
            {
                "cosmic_collection",
                (new[] { "galaxy", "cosmic", "crystal" }, new[] { "trail_cosmic", "trail_crystal" })
            },
            {
                "ultimate_collection",
                (SnakeSkins.ToArray(), TrailEffects.ToArray())
            }
        };

        // Subscriptions
        public const string ProMonthly = "snake_classic_pro_monthly";
        public const string ProYearly = "snake_classic_pro_yearly";
        public const string BattlePass = "battle_pass_season";

        // Tournament Entries (Consumable)
        public static readonly HashSet<string> TournamentEntries = new()
        {
            "tournament_bronze", "tournament_silver", "tournament_gold",
            "championship_entry", "tournament_vip_entry"
        };
    }

    public VerifyPurchaseCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<VerifyPurchaseResultDto>> Handle(
        VerifyPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<VerifyPurchaseResultDto>.Unauthorized();
        }

        var userId = _currentUser.UserId.Value;

        // Check for duplicate transaction
        var existingPurchase = await _context.Purchases
            .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

        if (existingPurchase != null)
        {
            return Result<VerifyPurchaseResultDto>.Success(new VerifyPurchaseResultDto(
                Success: true,
                IsValid: existingPurchase.IsVerified,
                Message: "Purchase already verified",
                UnlockedContent: existingPurchase.ContentUnlocked
            ));
        }

        // TODO: In production, implement real receipt verification:
        // - For Google Play: Use Google Play Developer API to verify purchase token
        // - For App Store: Use App Store Server API to verify receipt
        // For now, we validate that transaction ID is present (development mode)
        var isValid = !string.IsNullOrEmpty(request.TransactionId);

        var unlockedContent = new List<string>();
        var isSubscription = IsSubscriptionProduct(request.ProductId);
        DateTime? expiresAt = null;

        // Calculate subscription expiry if applicable
        if (isSubscription && isValid)
        {
            expiresAt = CalculateSubscriptionExpiry(request.ProductId);
        }

        var purchase = new Purchase
        {
            UserId = userId,
            ProductId = request.ProductId,
            TransactionId = request.TransactionId,
            Platform = request.Platform,
            ReceiptData = request.ReceiptData,
            IsVerified = isValid,
            IsSubscription = isSubscription,
            ExpiresAt = expiresAt,
            AutoRenewing = isSubscription, // Assume auto-renewing by default
            PurchaseTimestamp = DateTime.UtcNow,
            VerifiedAt = isValid ? DateTime.UtcNow : null
        };

        if (isValid)
        {
            unlockedContent = await UnlockContentForPurchase(userId, request.ProductId, cancellationToken);
            purchase.ContentUnlocked = unlockedContent;
        }

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<VerifyPurchaseResultDto>.Success(new VerifyPurchaseResultDto(
            Success: true,
            IsValid: isValid,
            Message: isValid ? "Purchase verified successfully" : "Invalid purchase",
            UnlockedContent: unlockedContent
        ));
    }

    private async Task<List<string>> UnlockContentForPurchase(
        Guid userId,
        string productId,
        CancellationToken cancellationToken)
    {
        var unlockedContent = new List<string>();

        // Get or create premium content record
        var premiumContent = await _context.UserPremiumContents
            .FirstOrDefaultAsync(pc => pc.UserId == userId, cancellationToken);

        if (premiumContent == null)
        {
            premiumContent = new UserPremiumContent { UserId = userId };
            _context.UserPremiumContents.Add(premiumContent);
        }

        // Handle different product types
        if (ProductIds.Themes.Contains(productId))
        {
            // Individual theme purchase
            if (!premiumContent.OwnedThemes.Contains(productId))
            {
                premiumContent.OwnedThemes.Add(productId);
                unlockedContent.Add(productId);
            }
        }
        else if (productId == ProductIds.ThemesBundle)
        {
            // Themes bundle - unlock all themes
            foreach (var theme in ProductIds.Themes)
            {
                if (!premiumContent.OwnedThemes.Contains(theme))
                {
                    premiumContent.OwnedThemes.Add(theme);
                    unlockedContent.Add(theme);
                }
            }
        }
        else if (ProductIds.CoinPacks.TryGetValue(productId, out var coinAmount))
        {
            // Coin purchase - add coins to user
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user != null)
            {
                user.Coins += coinAmount;
                user.UpdatedAt = DateTime.UtcNow;
                unlockedContent.Add($"coins:{coinAmount}");
            }
        }
        else if (ProductIds.PowerupPacks.Contains(productId))
        {
            // Power-up pack purchase
            if (!premiumContent.OwnedPowerups.Contains(productId))
            {
                premiumContent.OwnedPowerups.Add(productId);
                unlockedContent.Add(productId);
            }
        }
        else if (ProductIds.SnakeSkins.Contains(productId))
        {
            // Individual snake skin purchase
            if (!premiumContent.OwnedCosmetics.Contains(productId))
            {
                premiumContent.OwnedCosmetics.Add(productId);
                unlockedContent.Add(productId);
            }
        }
        else if (ProductIds.TrailEffects.Contains(productId))
        {
            // Individual trail effect purchase
            if (!premiumContent.OwnedCosmetics.Contains(productId))
            {
                premiumContent.OwnedCosmetics.Add(productId);
                unlockedContent.Add(productId);
            }
        }
        else if (ProductIds.CosmeticBundles.TryGetValue(productId, out var bundleContents))
        {
            // Cosmetic bundle - unlock all included skins and trails
            foreach (var skin in bundleContents.Skins)
            {
                if (!premiumContent.OwnedCosmetics.Contains(skin))
                {
                    premiumContent.OwnedCosmetics.Add(skin);
                    unlockedContent.Add(skin);
                }
            }
            foreach (var trail in bundleContents.Trails)
            {
                if (!premiumContent.OwnedCosmetics.Contains(trail))
                {
                    premiumContent.OwnedCosmetics.Add(trail);
                    unlockedContent.Add(trail);
                }
            }
        }
        else if (productId == ProductIds.ProMonthly || productId == ProductIds.ProYearly)
        {
            // Pro subscription
            premiumContent.PremiumTier = "premium";
            premiumContent.SubscriptionActive = true;
            premiumContent.SubscriptionExpiresAt = CalculateSubscriptionExpiry(productId);
            unlockedContent.Add("premium_subscription");

            // Pro subscription includes all themes
            foreach (var theme in ProductIds.Themes)
            {
                if (!premiumContent.OwnedThemes.Contains(theme))
                {
                    premiumContent.OwnedThemes.Add(theme);
                    unlockedContent.Add(theme);
                }
            }
        }
        else if (productId == ProductIds.BattlePass)
        {
            // Battle pass
            premiumContent.BattlePassActive = true;
            premiumContent.BattlePassExpiresAt = DateTime.UtcNow.AddDays(60); // 60-day season
            premiumContent.BattlePassTier = 0; // Start at tier 0
            unlockedContent.Add("battle_pass");
        }
        else if (ProductIds.TournamentEntries.Contains(productId))
        {
            // Tournament entry (consumable)
            premiumContent.TournamentEntries ??= new Dictionary<string, int>();

            if (premiumContent.TournamentEntries.ContainsKey(productId))
            {
                premiumContent.TournamentEntries[productId]++;
            }
            else
            {
                premiumContent.TournamentEntries[productId] = 1;
            }
            unlockedContent.Add($"tournament_entry:{productId}");
        }

        premiumContent.UpdatedAt = DateTime.UtcNow;

        return unlockedContent;
    }

    private static bool IsSubscriptionProduct(string productId)
    {
        return productId == ProductIds.ProMonthly ||
               productId == ProductIds.ProYearly ||
               productId == ProductIds.BattlePass;
    }

    private static DateTime? CalculateSubscriptionExpiry(string productId)
    {
        return productId switch
        {
            ProductIds.ProMonthly => DateTime.UtcNow.AddMonths(1),
            ProductIds.ProYearly => DateTime.UtcNow.AddYears(1),
            ProductIds.BattlePass => DateTime.UtcNow.AddDays(60), // 60-day season
            _ => null
        };
    }
}
