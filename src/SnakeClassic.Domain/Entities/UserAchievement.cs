using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class UserAchievement : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public int CurrentProgress { get; set; }
    public bool IsUnlocked { get; set; }
    public bool RewardClaimed { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
