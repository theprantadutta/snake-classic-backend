using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class BattlePassSeason : BaseEntity
{
    public string SeasonId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Theme { get; set; } = "default";
    public string ThemeColor { get; set; } = "#FFD700";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxLevel { get; set; } = 100;
    public decimal Price { get; set; } = 9.99m;
    public List<Dictionary<string, object>>? LevelsConfig { get; set; }
    public Dictionary<string, object>? ExtraData { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<UserBattlePassProgress> UserProgress { get; set; } = new List<UserBattlePassProgress>();
}
