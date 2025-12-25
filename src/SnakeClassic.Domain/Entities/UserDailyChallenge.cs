using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class UserDailyChallenge : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ChallengeId { get; set; }
    public int CurrentProgress { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool ClaimedReward { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public DailyChallenge Challenge { get; set; } = null!;
}
