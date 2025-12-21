using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class NotificationHistory : BaseEntity
{
    public string? NotificationId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public NotificationType Type { get; set; } = NotificationType.Social;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string RecipientType { get; set; } = "token";
    public int RecipientsCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public Dictionary<string, object>? ExtraData { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
