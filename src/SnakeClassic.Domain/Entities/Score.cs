using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class Score : BaseEntity
{
    public Guid UserId { get; set; }
    public int ScoreValue { get; set; }
    public int GameDurationSeconds { get; set; }
    public int FoodsEaten { get; set; }
    public GameMode GameMode { get; set; } = GameMode.Classic;
    public Difficulty Difficulty { get; set; } = Difficulty.Normal;
    public Dictionary<string, object>? GameData { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime? PlayedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
    public GameReplay? Replay { get; set; }
}
