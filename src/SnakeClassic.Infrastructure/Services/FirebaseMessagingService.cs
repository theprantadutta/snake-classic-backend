using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Infrastructure.Services;

public class FirebaseMessagingService : IFirebaseMessagingService
{
    private readonly ILogger<FirebaseMessagingService> _logger;

    public FirebaseMessagingService(ILogger<FirebaseMessagingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SendToTokenAsync(NotificationPayload payload, string token)
    {
        var message = CreateMessage(payload);
        message.Token = token;

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent message to token. MessageId: {MessageId}", response);
            return response;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Failed to send message to token: {Token}", token);
            throw;
        }
    }

    public async Task<string> SendToTopicAsync(NotificationPayload payload, string topic)
    {
        var message = CreateMessage(payload);
        message.Topic = topic;

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent message to topic {Topic}. MessageId: {MessageId}", topic, response);
            return response;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Failed to send message to topic: {Topic}", topic);
            throw;
        }
    }

    public async Task<BatchSendResult> SendMulticastAsync(NotificationPayload payload, IEnumerable<string> tokens)
    {
        var tokenList = tokens.ToList();
        if (!tokenList.Any())
        {
            return new BatchSendResult { SuccessCount = 0, FailureCount = 0 };
        }

        var message = new MulticastMessage
        {
            Tokens = tokenList,
            Notification = new Notification
            {
                Title = payload.Title,
                Body = payload.Body,
                ImageUrl = payload.ImageUrl
            },
            Data = BuildDataPayload(payload),
            Android = new AndroidConfig
            {
                Priority = payload.Priority == "high" ? Priority.High : Priority.Normal,
                Notification = new AndroidNotification
                {
                    ChannelId = "snake_classic_notifications",
                    Color = "#4CAF50"
                }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

            var errors = new List<string>();
            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess && response.Responses[i].Exception != null)
                {
                    errors.Add($"Token {i}: {response.Responses[i].Exception.Message}");
                }
            }

            _logger.LogInformation("Multicast send completed. Success: {Success}, Failure: {Failure}",
                response.SuccessCount, response.FailureCount);

            return new BatchSendResult
            {
                SuccessCount = response.SuccessCount,
                FailureCount = response.FailureCount,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send multicast message");
            throw;
        }
    }

    public async Task SubscribeToTopicAsync(string token, string topic)
    {
        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(new[] { token }, topic);
            _logger.LogInformation("Subscribed token to topic {Topic}. Success: {Success}", topic, response.SuccessCount);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Failed to subscribe token to topic: {Topic}", topic);
            throw;
        }
    }

    public async Task UnsubscribeFromTopicAsync(string token, string topic)
    {
        try
        {
            var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(new[] { token }, topic);
            _logger.LogInformation("Unsubscribed token from topic {Topic}. Success: {Success}", topic, response.SuccessCount);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe token from topic: {Topic}", topic);
            throw;
        }
    }

    private Message CreateMessage(NotificationPayload payload)
    {
        return new Message
        {
            Notification = new Notification
            {
                Title = payload.Title,
                Body = payload.Body,
                ImageUrl = payload.ImageUrl
            },
            Data = BuildDataPayload(payload),
            Android = new AndroidConfig
            {
                Priority = payload.Priority == "high" ? Priority.High : Priority.Normal,
                Notification = new AndroidNotification
                {
                    ChannelId = "snake_classic_notifications",
                    Color = "#4CAF50"
                }
            }
        };
    }

    private Dictionary<string, string> BuildDataPayload(NotificationPayload payload)
    {
        var data = new Dictionary<string, string>
        {
            ["sent_at"] = DateTime.UtcNow.ToString("O"),
            ["priority"] = payload.Priority
        };

        if (!string.IsNullOrEmpty(payload.Route))
        {
            data["route"] = payload.Route;
        }

        if (payload.RouteParams != null)
        {
            data["route_params"] = System.Text.Json.JsonSerializer.Serialize(payload.RouteParams);
        }

        if (payload.Data != null)
        {
            foreach (var (key, value) in payload.Data)
            {
                data[key] = value;
            }
        }

        return data;
    }
}
