using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class UserPreferences : BaseEntity
{
    public Guid UserId { get; set; }
    public string Theme { get; set; } = "classic";
    public bool SoundEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
    public bool VibrationEnabled { get; set; } = true;
    public bool NotificationsEnabled { get; set; } = true;
    public Dictionary<string, object>? SettingsJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
