using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class MultiplayerGame : BaseEntity
{
    public string GameId { get; set; } = null!;
    public GameMode Mode { get; set; } = GameMode.Classic;
    public MultiplayerGameStatus Status { get; set; } = MultiplayerGameStatus.Waiting;
    public string? RoomCode { get; set; }
    public int MaxPlayers { get; set; } = 4;
    public List<Dictionary<string, object>>? FoodPositions { get; set; }
    public List<Dictionary<string, object>>? PowerUps { get; set; }
    public Dictionary<string, object>? GameSettings { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    // Navigation property
    public ICollection<MultiplayerPlayer> Players { get; set; } = new List<MultiplayerPlayer>();
}
