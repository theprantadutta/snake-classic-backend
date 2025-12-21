using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Multiplayer.DTOs;

namespace SnakeClassic.Application.Features.Multiplayer.Queries.GetGame;

public class GetMultiplayerGameQueryHandler
    : IRequestHandler<GetMultiplayerGameQuery, Result<MultiplayerGameDto>>
{
    private readonly IAppDbContext _context;

    public GetMultiplayerGameQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MultiplayerGameDto>> Handle(
        GetMultiplayerGameQuery request,
        CancellationToken cancellationToken)
    {
        var game = await _context.MultiplayerGames
            .AsNoTracking()
            .Include(g => g.Players)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(g => g.Id == request.GameId, cancellationToken);

        if (game == null)
        {
            return Result<MultiplayerGameDto>.NotFound("Game not found");
        }

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

        return Result<MultiplayerGameDto>.Success(new MultiplayerGameDto(
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
