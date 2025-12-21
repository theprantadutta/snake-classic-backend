using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Purchases.Commands.VerifyPurchase;
using SnakeClassic.Application.Features.Purchases.Queries.GetPremiumContent;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class PurchasesController : BaseApiController
{
    [HttpPost("verify")]
    public async Task<ActionResult> VerifyPurchase([FromBody] VerifyPurchaseRequest request)
    {
        var command = new VerifyPurchaseCommand(
            request.PurchaseData.ProductId,
            request.PurchaseData.TransactionId,
            request.Platform,
            request.PurchaseData.ReceiptData
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("restore")]
    public async Task<ActionResult> RestorePurchases([FromBody] RestorePurchasesRequest request)
    {
        // For restore, we verify each transaction
        // In a real implementation, you would iterate through previous purchases
        return Ok(new { message = "Purchases restored", count = 0 });
    }

    [HttpGet("premium-content")]
    public async Task<ActionResult> GetPremiumContent()
    {
        var result = await Mediator.Send(new GetPremiumContentQuery());
        return HandleResult(result);
    }

    [HttpPost("webhook/google-play")]
    [AllowAnonymous]
    public ActionResult GooglePlayWebhook([FromBody] object payload)
    {
        // Handle Google Play Real-time Developer Notifications
        // In production, verify and process the notification
        return Ok();
    }

    [HttpPost("webhook/app-store")]
    [AllowAnonymous]
    public ActionResult AppStoreWebhook([FromBody] object payload)
    {
        // Handle App Store Server Notifications
        // In production, verify and process the notification
        return Ok();
    }
}

public record PurchaseData(
    string ProductId,
    string TransactionId,
    string? ReceiptData
);

public record VerifyPurchaseRequest(
    PurchaseData PurchaseData,
    string Platform
);

public record RestorePurchasesRequest(string Platform, string? ReceiptData);
