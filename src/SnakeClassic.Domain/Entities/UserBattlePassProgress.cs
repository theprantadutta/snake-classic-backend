using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class UserBattlePassProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SeasonId { get; set; }
    public bool HasPremium { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public int CurrentXp { get; set; }
    public int TotalXpEarned { get; set; }
    public List<string> ClaimedRewards { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public BattlePassSeason Season { get; set; } = null!;
}
