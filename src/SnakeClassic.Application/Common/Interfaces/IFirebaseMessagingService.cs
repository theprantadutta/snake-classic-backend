namespace SnakeClassic.Application.Common.Interfaces;

public interface IFirebaseMessagingService
{
    Task<string> SendToTokenAsync(NotificationPayload payload, string token);
    Task<string> SendToTopicAsync(NotificationPayload payload, string topic);
    Task<BatchSendResult> SendMulticastAsync(NotificationPayload payload, IEnumerable<string> tokens);
    Task SubscribeToTopicAsync(string token, string topic);
    Task UnsubscribeFromTopicAsync(string token, string topic);
}

public class NotificationPayload
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string Priority { get; set; } = "normal";
    public Dictionary<string, string>? Data { get; set; }
    public string? Route { get; set; }
    public Dictionary<string, string>? RouteParams { get; set; }
}

public class BatchSendResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
