using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.BattlePass.DTOs;

namespace SnakeClassic.Application.Features.BattlePass.Commands.ClaimReward;

public record ClaimBattlePassRewardCommand(int Level, bool IsPremium) : IRequest<Result<ClaimRewardResultDto>>;
