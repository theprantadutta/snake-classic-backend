using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class Purchase : BaseEntity
{
    public Guid UserId { get; set; }
    public string ProductId { get; set; } = null!;
    public string TransactionId { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public string? ReceiptData { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationError { get; set; }
    public bool IsSubscription { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool AutoRenewing { get; set; }
    public List<string>? ContentUnlocked { get; set; }
    public DateTime PurchaseTimestamp { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
