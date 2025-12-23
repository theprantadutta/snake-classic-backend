using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class MatchmakingQueue : BaseEntity
{
    public Guid UserId { get; set; }
    public MultiplayerGameMode Mode { get; set; }
    public int DesiredPlayers { get; set; } = 2;  // 2, 4, 6, 8
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    public bool IsMatched { get; set; }
    public Guid? MatchedGameId { get; set; }
    public string? ConnectionId { get; set; }  // SignalR connection for notifications

    // Navigation property
    public User User { get; set; } = null!;
}
