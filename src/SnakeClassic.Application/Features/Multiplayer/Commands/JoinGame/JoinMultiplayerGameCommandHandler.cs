using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Multiplayer.DTOs;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Multiplayer.Commands.JoinGame;

public class JoinMultiplayerGameCommandHandler
    : IRequestHandler<JoinMultiplayerGameCommand, Result<JoinGameResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public JoinMultiplayerGameCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<JoinGameResultDto>> Handle(
        JoinMultiplayerGameCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<JoinGameResultDto>.Unauthorized();
        }

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(g => g.RoomCode == request.RoomCode.ToUpper(), cancellationToken);

        if (game == null)
        {
            return Result<JoinGameResultDto>.NotFound("Game not found");
        }

        if (game.Status != MultiplayerGameStatus.Waiting)
        {
            return Result<JoinGameResultDto>.Failure("Game has already started");
        }

        if (game.Players.Count >= game.MaxPlayers)
        {
            return Result<JoinGameResultDto>.Failure("Game is full");
        }

        if (game.Players.Any(p => p.UserId == _currentUser.UserId.Value))
        {
            return Result<JoinGameResultDto>.Failure("Already in this game");
        }

        var playerIndex = game.Players.Count;
        var player = new MultiplayerPlayer
        {
            GameId = game.Id,
            UserId = _currentUser.UserId.Value,
            PlayerIndex = playerIndex,
            IsReady = false,
            IsAlive = true
        };

        _context.MultiplayerPlayers.Add(player);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with user data
        await _context.MultiplayerPlayers
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == player.Id, cancellationToken);

        var players = game.Players.Select(p => new MultiplayerPlayerDto(
            p.UserId,
            p.User?.Username,
            p.User?.DisplayName,
            p.User?.PhotoUrl,
            p.PlayerIndex,
            p.Score,
            p.IsReady,
            p.IsAlive
        )).ToList();

        return Result<JoinGameResultDto>.Success(new JoinGameResultDto(
            game.Id,
            playerIndex,
            players
        ));
    }
}
