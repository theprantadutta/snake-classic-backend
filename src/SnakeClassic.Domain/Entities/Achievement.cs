using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class Achievement : BaseEntity
{
    public string AchievementId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public AchievementCategory Category { get; set; } = AchievementCategory.General;
    public AchievementTier Tier { get; set; } = AchievementTier.Bronze;
    public RequirementType RequirementType { get; set; } = RequirementType.Count;
    public int RequirementValue { get; set; } = 1;
    public int XpReward { get; set; }
    public int CoinReward { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSecret { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
