using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class TournamentEntry : BaseEntity
{
    public Guid TournamentId { get; set; }
    public Guid UserId { get; set; }
    public int BestScore { get; set; }
    public int GamesPlayed { get; set; }
    public int? Rank { get; set; }
    public bool PrizeClaimed { get; set; }
    public Dictionary<string, object>? PrizeAmount { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastPlayedAt { get; set; }

    // Navigation properties
    public Tournament Tournament { get; set; } = null!;
    public User User { get; set; } = null!;
}
