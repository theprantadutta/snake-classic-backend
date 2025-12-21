namespace SnakeClassic.Application.Features.Purchases.DTOs;

public record PurchaseDto(
    Guid Id,
    string ProductId,
    string TransactionId,
    string Platform,
    bool IsVerified,
    bool IsSubscription,
    DateTime? ExpiresAt,
    DateTime PurchasedAt
);

public record VerifyPurchaseResultDto(
    bool Success,
    bool IsValid,
    string? Message,
    List<string>? UnlockedContent
);

public record PremiumContentDto(
    string PremiumTier,
    List<string> OwnedThemes,
    List<string> OwnedPowerups,
    List<string> OwnedCosmetics,
    bool IsSubscriptionActive
);
