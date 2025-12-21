using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Api.Controllers.V1;

public class TestController : BaseApiController
{
    private readonly IFirebaseMessagingService _messagingService;

    public TestController(IFirebaseMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }

    [HttpPost("send-test-notification")]
    [Authorize]
    public async Task<ActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
    {
        try
        {
            var payload = new NotificationPayload
            {
                Title = request.Title ?? "Test Notification",
                Body = request.Body ?? "This is a test notification from Snake Classic API",
                Priority = "high"
            };

            var messageId = await _messagingService.SendToTokenAsync(payload, request.Token);
            return Ok(new { success = true, messageId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}

public record TestNotificationRequest(string Token, string? Title, string? Body);
