using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Purchases.DTOs;

namespace SnakeClassic.Application.Features.Purchases.Commands.RestorePurchases;

public record RestorePurchasesCommand(
    string Platform,
    string? ReceiptData
) : IRequest<Result<RestorePurchasesResultDto>>;

public record RestorePurchasesResultDto(
    bool Success,
    int RestoredCount,
    List<string> RestoredProducts,
    string? Message
);
