using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.BattlePass.DTOs;

namespace SnakeClassic.Application.Features.BattlePass.Commands.AddXp;

public record AddBattlePassXpCommand(int XpAmount) : IRequest<Result<AddXpResultDto>>;
