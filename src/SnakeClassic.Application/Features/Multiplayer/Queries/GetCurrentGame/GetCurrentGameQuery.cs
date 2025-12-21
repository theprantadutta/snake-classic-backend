using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Multiplayer.DTOs;

namespace SnakeClassic.Application.Features.Multiplayer.Queries.GetCurrentGame;

public record GetCurrentGameQuery : IRequest<Result<MultiplayerGameDto?>>;
