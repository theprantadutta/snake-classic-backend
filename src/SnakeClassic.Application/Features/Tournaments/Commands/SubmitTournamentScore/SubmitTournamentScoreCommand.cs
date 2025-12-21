using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Tournaments.Commands.SubmitTournamentScore;

public record SubmitTournamentScoreCommand(Guid TournamentId, int Score) : IRequest<Result<SubmitTournamentScoreResultDto>>;

public record SubmitTournamentScoreResultDto(
    bool Success,
    int NewBestScore,
    int GamesPlayed,
    int? NewRank
);
