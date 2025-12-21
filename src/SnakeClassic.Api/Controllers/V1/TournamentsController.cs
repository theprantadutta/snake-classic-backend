using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Tournaments.Commands.JoinTournament;
using SnakeClassic.Application.Features.Tournaments.Commands.SubmitTournamentScore;
using SnakeClassic.Application.Features.Tournaments.Queries.GetTournamentLeaderboard;
using SnakeClassic.Application.Features.Tournaments.Queries.GetTournaments;

namespace SnakeClassic.Api.Controllers.V1;

public class TournamentsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetTournaments([FromQuery] bool activeOnly = false)
    {
        var result = await Mediator.Send(new GetTournamentsQuery(activeOnly));
        return HandleResult(result);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult> GetActiveTournaments()
    {
        var result = await Mediator.Send(new GetTournamentsQuery(true));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetTournament(Guid id)
    {
        var result = await Mediator.Send(new GetTournamentLeaderboardQuery(id));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/join")]
    [Authorize]
    public async Task<ActionResult> JoinTournament(Guid id)
    {
        var result = await Mediator.Send(new JoinTournamentCommand(id));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/score")]
    [Authorize]
    public async Task<ActionResult> SubmitTournamentScore(Guid id, [FromBody] SubmitTournamentScoreRequest request)
    {
        var result = await Mediator.Send(new SubmitTournamentScoreCommand(id, request.Score));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/leaderboard")]
    [AllowAnonymous]
    public async Task<ActionResult> GetTournamentLeaderboard(Guid id, [FromQuery] int limit = 100)
    {
        var result = await Mediator.Send(new GetTournamentLeaderboardQuery(id, limit));
        return HandleResult(result);
    }
}

public record SubmitTournamentScoreRequest(int Score);
