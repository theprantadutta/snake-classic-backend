using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Purchases.DTOs;

namespace SnakeClassic.Application.Features.Purchases.Queries.GetPremiumContent;

public record GetPremiumContentQuery : IRequest<Result<PremiumContentDto>>;
