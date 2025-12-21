using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Multiplayer.DTOs;

namespace SnakeClassic.Application.Features.Multiplayer.Commands.JoinGame;

public record JoinMultiplayerGameCommand(string RoomCode) : IRequest<Result<JoinGameResultDto>>;
