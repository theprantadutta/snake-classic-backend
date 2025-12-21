using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Multiplayer.Commands.LeaveGame;

public class LeaveMultiplayerGameCommandHandler : IRequestHandler<LeaveMultiplayerGameCommand, Result<bool>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LeaveMultiplayerGameCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(LeaveMultiplayerGameCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<bool>.Unauthorized();
        }

        var player = await _context.MultiplayerPlayers
            .Include(p => p.Game)
            .FirstOrDefaultAsync(p =>
                p.GameId == request.GameId &&
                p.UserId == _currentUser.UserId.Value, cancellationToken);

        if (player == null)
        {
            return Result<bool>.NotFound("Not in this game");
        }

        var game = player.Game;

        // If game is playing, mark player as not alive instead of removing
        if (game.Status == MultiplayerGameStatus.Playing)
        {
            player.IsAlive = false;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }

        // Remove player from waiting game
        _context.MultiplayerPlayers.Remove(player);

        // If host leaves waiting game, assign new host or delete game
        if (game.HostId == _currentUser.UserId.Value)
        {
            var remainingPlayers = await _context.MultiplayerPlayers
                .Where(p => p.GameId == game.Id && p.Id != player.Id)
                .OrderBy(p => p.PlayerIndex)
                .ToListAsync(cancellationToken);

            if (remainingPlayers.Any())
            {
                game.HostId = remainingPlayers.First().UserId;
            }
            else
            {
                game.Status = MultiplayerGameStatus.Finished;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
