using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.DailyBonus.DTOs;

namespace SnakeClassic.Application.Features.DailyBonus.Commands.ClaimDailyBonus;

public record ClaimDailyBonusCommand : IRequest<Result<ClaimDailyBonusResultDto>>;
