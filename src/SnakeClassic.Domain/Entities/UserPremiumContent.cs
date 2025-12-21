using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class UserPremiumContent : BaseEntity
{
    public Guid UserId { get; set; }
    public string PremiumTier { get; set; } = "free";
    public bool SubscriptionActive { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public bool BattlePassActive { get; set; }
    public DateTime? BattlePassExpiresAt { get; set; }
    public int BattlePassTier { get; set; }
    public List<string> OwnedThemes { get; set; } = new();
    public List<string> OwnedPowerups { get; set; } = new();
    public List<string> OwnedCosmetics { get; set; } = new();
    public Dictionary<string, int>? TournamentEntries { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
