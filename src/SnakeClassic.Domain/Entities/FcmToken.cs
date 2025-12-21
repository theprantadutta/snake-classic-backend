using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class FcmToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public string Platform { get; set; } = "flutter";
    public List<string> SubscribedTopics { get; set; } = new();
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
