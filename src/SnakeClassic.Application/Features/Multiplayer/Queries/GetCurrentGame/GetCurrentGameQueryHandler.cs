using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Multiplayer.DTOs;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Multiplayer.Queries.GetCurrentGame;

public class GetCurrentGameQueryHandler
    : IRequestHandler<GetCurrentGameQuery, Result<MultiplayerGameDto?>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentGameQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<MultiplayerGameDto?>> Handle(
        GetCurrentGameQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<MultiplayerGameDto?>.Unauthorized();
        }

        var playerInGame = await _context.MultiplayerPlayers
            .AsNoTracking()
            .Include(mp => mp.Game)
                .ThenInclude(g => g.Players)
                    .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(mp =>
                mp.UserId == _currentUser.UserId.Value &&
                (mp.Game.Status == MultiplayerGameStatus.Waiting ||
                 mp.Game.Status == MultiplayerGameStatus.Countdown ||
                 mp.Game.Status == MultiplayerGameStatus.Playing), cancellationToken);

        if (playerInGame == null)
        {
            return Result<MultiplayerGameDto?>.Success(null);
        }

        var game = playerInGame.Game;
        var players = game.Players
            .OrderBy(p => p.PlayerIndex)
            .Select(p => new MultiplayerPlayerDto(
                p.UserId,
                p.User?.Username,
                p.User?.DisplayName,
                p.User?.PhotoUrl,
                p.PlayerIndex,
                p.Score,
                p.IsReady,
                p.IsAlive
            )).ToList();

        return Result<MultiplayerGameDto?>.Success(new MultiplayerGameDto(
            game.Id,
            game.GameId,
            game.Mode,
            game.Status,
            game.RoomCode,
            game.MaxPlayers,
            game.Players.Count,
            game.HostId,
            players
        ));
    }
}
