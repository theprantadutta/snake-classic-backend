using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Purchases.DTOs;

namespace SnakeClassic.Application.Features.Purchases.Commands.VerifyPurchase;

public record VerifyPurchaseCommand(
    string ProductId,
    string TransactionId,
    string Platform,
    string? ReceiptData
) : IRequest<Result<VerifyPurchaseResultDto>>;
