using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class MultiplayerPlayer : BaseEntity
{
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public int PlayerIndex { get; set; }
    public int Score { get; set; }
    public bool IsAlive { get; set; } = true;
    public bool IsReady { get; set; }
    public List<Dictionary<string, object>>? SnakePositions { get; set; }
    public string Direction { get; set; } = "right";
    public string? SnakeColor { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdateAt { get; set; } = DateTime.UtcNow;

    // Reconnection support
    public string? ConnectionId { get; set; }
    public DateTime? DisconnectedAt { get; set; }

    // Elimination tracking for multi-player games
    public int? EliminationRank { get; set; }
    public DateTime? EliminatedAt { get; set; }

    // Navigation properties
    public MultiplayerGame Game { get; set; } = null!;
    public User User { get; set; } = null!;

    // Computed property for reconnection eligibility (60 second window)
    public bool CanReconnect => DisconnectedAt != null &&
        (DateTime.UtcNow - DisconnectedAt.Value).TotalSeconds < 60;
}
