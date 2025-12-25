using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class DailyChallenge : BaseEntity
{
    public DateOnly ChallengeDate { get; set; }
    public ChallengeType Type { get; set; }
    public ChallengeDifficulty Difficulty { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int TargetValue { get; set; }
    public int CoinReward { get; set; }
    public int XpReward { get; set; }
    public string? RequiredGameMode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<UserDailyChallenge> UserChallenges { get; set; } = new List<UserDailyChallenge>();
}
