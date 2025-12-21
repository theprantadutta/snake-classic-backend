using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.BattlePass.Commands.PurchasePremium;

public record PurchasePremiumCommand : IRequest<Result<PurchasePremiumResultDto>>;

public record PurchasePremiumResultDto(bool Success, string Message);
