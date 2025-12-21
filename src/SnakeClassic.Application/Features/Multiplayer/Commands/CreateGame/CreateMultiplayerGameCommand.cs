using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Multiplayer.DTOs;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Multiplayer.Commands.CreateGame;

public record CreateMultiplayerGameCommand(
    MultiplayerGameMode Mode,
    int MaxPlayers = 4
) : IRequest<Result<CreateGameResultDto>>;
