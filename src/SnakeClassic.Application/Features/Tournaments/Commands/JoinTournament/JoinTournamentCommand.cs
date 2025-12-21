using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Tournaments.Commands.JoinTournament;

public record JoinTournamentCommand(Guid TournamentId) : IRequest<Result<JoinTournamentResultDto>>;

public record JoinTournamentResultDto(bool Success, string Message);
