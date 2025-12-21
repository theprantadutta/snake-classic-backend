using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class Tournament : BaseEntity
{
    public string TournamentId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public TournamentType Type { get; set; } = TournamentType.Daily;
    public TournamentStatus Status { get; set; } = TournamentStatus.Upcoming;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int EntryFee { get; set; }
    public int MinLevel { get; set; } = 1;
    public int? MaxPlayers { get; set; }
    public Dictionary<string, object>? PrizePool { get; set; }
    public Dictionary<string, object>? Rules { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<TournamentEntry> Entries { get; set; } = new List<TournamentEntry>();
}
