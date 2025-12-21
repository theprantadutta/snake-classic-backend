using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Tournaments.DTOs;

namespace SnakeClassic.Application.Features.Tournaments.Queries.GetTournamentLeaderboard;

public record GetTournamentLeaderboardQuery(Guid TournamentId, int Limit = 100) : IRequest<Result<TournamentLeaderboardDto>>;
