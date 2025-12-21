using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Multiplayer.Commands.LeaveGame;

public record LeaveMultiplayerGameCommand(Guid GameId) : IRequest<Result<bool>>;
