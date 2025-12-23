using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Api.Services;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Multiplayer.Commands.CreateGame;
using SnakeClassic.Application.Features.Multiplayer.Commands.JoinGame;
using SnakeClassic.Application.Features.Multiplayer.Commands.LeaveGame;
using SnakeClassic.Application.Features.Multiplayer.Queries.GetCurrentGame;
using SnakeClassic.Application.Features.Multiplayer.Queries.GetGame;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class MultiplayerController : BaseApiController
{
    private readonly IAppDbContext _context;
    private readonly IMatchmakingJobService _matchmakingJobService;

    public MultiplayerController(IAppDbContext context, IMatchmakingJobService matchmakingJobService)
    {
        _context = context;
        _matchmakingJobService = matchmakingJobService;
    }
    [HttpPost("create")]
    public async Task<ActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        if (!Enum.TryParse<MultiplayerGameMode>(request.Mode, true, out var mode))
        {
            mode = MultiplayerGameMode.Classic;
        }

        var result = await Mediator.Send(new CreateMultiplayerGameCommand(mode, request.MaxPlayers ?? 4));
        return HandleResult(result);
    }

    [HttpPost("join")]
    public async Task<ActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
        var result = await Mediator.Send(new JoinMultiplayerGameCommand(request.RoomCode));
        return HandleResult(result);
    }

    [HttpGet("game/{id:guid}")]
    public async Task<ActionResult> GetGame(Guid id)
    {
        var result = await Mediator.Send(new GetMultiplayerGameQuery(id));
        return HandleResult(result);
    }

    [HttpPost("game/{id:guid}/leave")]
    public async Task<ActionResult> LeaveGame(Guid id)
    {
        var result = await Mediator.Send(new LeaveMultiplayerGameCommand(id));
        return HandleResult(result);
    }

    [HttpGet("current")]
    public async Task<ActionResult> GetCurrentGame()
    {
        var result = await Mediator.Send(new GetCurrentGameQuery());
        return HandleResult(result);
    }

    [HttpGet("available")]
    public async Task<ActionResult> GetAvailableGames()
    {
        // Get public games that are waiting for players
        var games = await _context.MultiplayerGames
            .Include(g => g.Players)
            .Where(g => g.Status == MultiplayerGameStatus.Waiting)
            .Where(g => g.Players.Count < g.MaxPlayers)
            .OrderByDescending(g => g.CreatedAt)
            .Take(20)
            .Select(g => new
            {
                g.Id,
                g.GameId,
                g.RoomCode,
                Mode = g.Mode.ToString(),
                g.MaxPlayers,
                CurrentPlayers = g.Players.Count,
                g.HostId,
                g.GameSettings,
                g.CreatedAt
            })
            .ToListAsync();

        return Ok(games);
    }

    /// <summary>
    /// DEBUG: Get matchmaking queue status
    /// </summary>
    [HttpGet("debug/queue")]
    public async Task<ActionResult> GetQueueStatus()
    {
        var queue = await _context.MatchmakingQueues
            .Include(q => q.User)
            .Where(q => !q.IsMatched)
            .OrderBy(q => q.QueuedAt)
            .Select(q => new
            {
                q.Id,
                q.UserId,
                Username = q.User.Username ?? q.User.DisplayName,
                Mode = q.Mode.ToString(),
                q.DesiredPlayers,
                q.QueuedAt,
                q.IsMatched,
                q.ConnectionId
            })
            .ToListAsync();

        return Ok(new
        {
            TotalInQueue = queue.Count,
            Players = queue
        });
    }

    /// <summary>
    /// DEBUG: Manually trigger matchmaking
    /// </summary>
    [HttpPost("debug/process-matchmaking")]
    public async Task<ActionResult> TriggerMatchmaking()
    {
        await _matchmakingJobService.ProcessMatchmakingQueue();
        return Ok(new { Message = "Matchmaking processed manually" });
    }
}

public record CreateGameRequest(string? Mode, int? MaxPlayers);
public record JoinGameRequest(string RoomCode);
