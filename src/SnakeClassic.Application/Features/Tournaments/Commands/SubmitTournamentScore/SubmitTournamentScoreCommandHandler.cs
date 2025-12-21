using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Tournaments.Commands.SubmitTournamentScore;

public class SubmitTournamentScoreCommandHandler
    : IRequestHandler<SubmitTournamentScoreCommand, Result<SubmitTournamentScoreResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SubmitTournamentScoreCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SubmitTournamentScoreResultDto>> Handle(
        SubmitTournamentScoreCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<SubmitTournamentScoreResultDto>.Unauthorized();
        }

        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament == null)
        {
            return Result<SubmitTournamentScoreResultDto>.NotFound("Tournament not found");
        }

        if (tournament.Status != TournamentStatus.Active)
        {
            return Result<SubmitTournamentScoreResultDto>.Failure("Tournament is not active");
        }

        var entry = await _context.TournamentEntries
            .FirstOrDefaultAsync(te =>
                te.TournamentId == request.TournamentId &&
                te.UserId == _currentUser.UserId.Value, cancellationToken);

        if (entry == null)
        {
            return Result<SubmitTournamentScoreResultDto>.Failure("Not registered for this tournament");
        }

        entry.GamesPlayed++;
        if (request.Score > entry.BestScore)
        {
            entry.BestScore = request.Score;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Calculate new rank
        var playersAbove = await _context.TournamentEntries
            .CountAsync(te =>
                te.TournamentId == request.TournamentId &&
                te.BestScore > entry.BestScore, cancellationToken);

        return Result<SubmitTournamentScoreResultDto>.Success(new SubmitTournamentScoreResultDto(
            Success: true,
            NewBestScore: entry.BestScore,
            GamesPlayed: entry.GamesPlayed,
            NewRank: playersAbove + 1
        ));
    }
}
