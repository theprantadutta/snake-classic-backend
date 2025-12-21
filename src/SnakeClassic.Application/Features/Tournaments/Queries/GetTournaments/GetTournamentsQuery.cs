using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Tournaments.DTOs;

namespace SnakeClassic.Application.Features.Tournaments.Queries.GetTournaments;

public record GetTournamentsQuery(bool ActiveOnly = false) : IRequest<Result<List<TournamentDto>>>;
