using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Multiplayer.DTOs;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Multiplayer.Commands.CreateGame;

public class CreateMultiplayerGameCommandHandler
    : IRequestHandler<CreateMultiplayerGameCommand, Result<CreateGameResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateMultiplayerGameCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateGameResultDto>> Handle(
        CreateMultiplayerGameCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<CreateGameResultDto>.Unauthorized();
        }

        // Check if user is already in a game
        var existingGame = await _context.MultiplayerPlayers
            .Include(mp => mp.Game)
            .FirstOrDefaultAsync(mp =>
                mp.UserId == _currentUser.UserId.Value &&
                (mp.Game.Status == MultiplayerGameStatus.Waiting ||
                 mp.Game.Status == MultiplayerGameStatus.Countdown ||
                 mp.Game.Status == MultiplayerGameStatus.Playing), cancellationToken);

        if (existingGame != null)
        {
            return Result<CreateGameResultDto>.Failure("Already in an active game");
        }

        // Generate unique room code
        var roomCode = GenerateRoomCode();
        while (await _context.MultiplayerGames.AnyAsync(g => g.RoomCode == roomCode, cancellationToken))
        {
            roomCode = GenerateRoomCode();
        }

        var game = new MultiplayerGame
        {
            GameId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            Mode = request.Mode,
            Status = MultiplayerGameStatus.Waiting,
            RoomCode = roomCode,
            MaxPlayers = Math.Clamp(request.MaxPlayers, 2, 8),
            HostId = _currentUser.UserId.Value
        };

        _context.MultiplayerGames.Add(game);

        // Add host as first player
        var player = new MultiplayerPlayer
        {
            Game = game,
            UserId = _currentUser.UserId.Value,
            PlayerIndex = 0,
            IsReady = true,
            IsAlive = true
        };

        _context.MultiplayerPlayers.Add(player);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CreateGameResultDto>.Created(new CreateGameResultDto(game.Id, game.RoomCode));
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
