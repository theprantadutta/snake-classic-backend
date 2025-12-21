using SnakeClassic.Domain.Common;

namespace SnakeClassic.Domain.Entities;

public class ScheduledJob : BaseEntity
{
    public string JobId { get; set; } = null!;
    public string JobName { get; set; } = null!;
    public string TriggerType { get; set; } = "date";
    public Dictionary<string, object>? TriggerConfig { get; set; }
    public string JobType { get; set; } = "notification";
    public Dictionary<string, object>? Payload { get; set; }
    public DateTime? NextRunTime { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
