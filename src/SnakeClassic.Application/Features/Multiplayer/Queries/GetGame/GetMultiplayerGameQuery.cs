using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Multiplayer.DTOs;

namespace SnakeClassic.Application.Features.Multiplayer.Queries.GetGame;

public record GetMultiplayerGameQuery(Guid GameId) : IRequest<Result<MultiplayerGameDto>>;
