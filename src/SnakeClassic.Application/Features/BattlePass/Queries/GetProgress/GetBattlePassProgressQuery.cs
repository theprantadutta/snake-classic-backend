using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.BattlePass.DTOs;

namespace SnakeClassic.Application.Features.BattlePass.Queries.GetProgress;

public record GetBattlePassProgressQuery : IRequest<Result<UserBattlePassProgressDto?>>;
