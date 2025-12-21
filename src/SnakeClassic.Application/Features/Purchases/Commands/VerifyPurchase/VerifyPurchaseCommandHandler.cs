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

        // Check for duplicate transaction
        var existingPurchase = await _context.Purchases
            .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

        if (existingPurchase != null)
        {
            return Result<VerifyPurchaseResultDto>.Success(new VerifyPurchaseResultDto(
                Success: true,
                IsValid: existingPurchase.IsVerified,
                Message: "Purchase already verified",
                UnlockedContent: null
            ));
        }

        // In a real app, verify with Google Play / App Store
        // For now, we'll trust the client
        var isValid = !string.IsNullOrEmpty(request.TransactionId);

        var purchase = new Purchase
        {
            UserId = _currentUser.UserId.Value,
            ProductId = request.ProductId,
            TransactionId = request.TransactionId,
            Platform = request.Platform,
            IsVerified = isValid,
            IsSubscription = request.ProductId.Contains("subscription", StringComparison.OrdinalIgnoreCase)
        };

        _context.Purchases.Add(purchase);

        var unlockedContent = new List<string>();

        if (isValid)
        {
            var premiumContent = await _context.UserPremiumContents
                .FirstOrDefaultAsync(pc => pc.UserId == _currentUser.UserId.Value, cancellationToken);

            if (premiumContent == null)
            {
                premiumContent = new UserPremiumContent { UserId = _currentUser.UserId.Value };
                _context.UserPremiumContents.Add(premiumContent);
            }

            // Unlock content based on product ID
            if (request.ProductId.Contains("theme_"))
            {
                premiumContent.OwnedThemes.Add(request.ProductId);
                unlockedContent.Add(request.ProductId);
            }
            else if (request.ProductId.Contains("powerup_"))
            {
                premiumContent.OwnedPowerups.Add(request.ProductId);
                unlockedContent.Add(request.ProductId);
            }
            else if (request.ProductId.Contains("premium"))
            {
                premiumContent.PremiumTier = "premium";
                unlockedContent.Add("premium_tier");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<VerifyPurchaseResultDto>.Success(new VerifyPurchaseResultDto(
            Success: true,
            IsValid: isValid,
            Message: isValid ? "Purchase verified successfully" : "Invalid purchase",
            UnlockedContent: unlockedContent
        ));
    }
}
