using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [HttpPost("create")]
    public async Task<ActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        if (!Enum.TryParse<MultiplayerGameMode>(request.Mode, true, out var mode))
        {
            mode = MultiplayerGameMode.FreeForAll;
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
}

public record CreateGameRequest(string? Mode, int? MaxPlayers);
public record JoinGameRequest(string RoomCode);
