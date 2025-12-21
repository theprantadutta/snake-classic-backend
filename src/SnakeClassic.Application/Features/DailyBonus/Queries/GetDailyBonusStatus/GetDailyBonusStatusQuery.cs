using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.DailyBonus.DTOs;

namespace SnakeClassic.Application.Features.DailyBonus.Queries.GetDailyBonusStatus;

public record GetDailyBonusStatusQuery : IRequest<Result<DailyBonusStatusDto>>;
