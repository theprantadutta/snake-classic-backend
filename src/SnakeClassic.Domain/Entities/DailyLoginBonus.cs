using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class DailyLoginBonus : BaseEntity
{
    public Guid UserId { get; set; }
    public int CurrentStreak { get; set; }
    public DateOnly? LastClaimDate { get; set; }
    public int TotalClaims { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
