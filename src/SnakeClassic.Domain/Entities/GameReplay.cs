using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class GameReplay : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ScoreId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public Dictionary<string, object> ReplayData { get; set; } = new();
    public int FinalScore { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Score? Score { get; set; }
}
