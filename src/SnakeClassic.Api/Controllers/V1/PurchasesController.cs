using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Purchases.Commands.RestorePurchases;
using SnakeClassic.Application.Features.Purchases.Commands.VerifyPurchase;
using SnakeClassic.Application.Features.Purchases.Queries.GetPremiumContent;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class PurchasesController : BaseApiController
{
    /// <summary>
    /// Verify a purchase from Google Play or App Store
    /// </summary>
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

    /// <summary>
    /// Restore previously purchased items for the authenticated user
    /// </summary>
    [HttpPost("restore")]
    public async Task<ActionResult> RestorePurchases([FromBody] RestorePurchasesRequest request)
    {
        var command = new RestorePurchasesCommand(request.Platform, request.ReceiptData);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the authenticated user's premium content (owned items, subscription status)
    /// </summary>
    [HttpGet("premium-content")]
    public async Task<ActionResult> GetPremiumContent()
    {
        var result = await Mediator.Send(new GetPremiumContentQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Google Play Real-time Developer Notifications webhook
    /// TODO: Implement signature verification and notification processing
    /// </summary>
    [HttpPost("webhook/google-play")]
    [AllowAnonymous]
    public ActionResult GooglePlayWebhook([FromBody] object payload)
    {
        // TODO: Implement Google Play RTDN handling
        // 1. Verify the JWT signature using Google's public keys
        // 2. Parse the notification type (SUBSCRIPTION_PURCHASED, SUBSCRIPTION_RENEWED, etc.)
        // 3. Update user subscription status accordingly
        // Reference: https://developer.android.com/google/play/billing/rtdn-reference
        return Ok(new { status = "received", implemented = false });
    }

    /// <summary>
    /// Apple App Store Server Notifications webhook
    /// TODO: Implement signature verification and notification processing
    /// </summary>
    [HttpPost("webhook/app-store")]
    [AllowAnonymous]
    public ActionResult AppStoreWebhook([FromBody] object payload)
    {
        // TODO: Implement App Store Server Notifications handling
        // 1. Verify the JWS signature using Apple's certificates
        // 2. Parse the notification type (DID_RENEW, DID_FAIL_TO_RENEW, etc.)
        // 3. Update user subscription status accordingly
        // Reference: https://developer.apple.com/documentation/appstoreservernotifications
        return Ok(new { status = "received", implemented = false });
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
