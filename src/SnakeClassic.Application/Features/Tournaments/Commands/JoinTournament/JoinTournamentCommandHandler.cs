using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Tournaments.Commands.JoinTournament;

public class JoinTournamentCommandHandler
    : IRequestHandler<JoinTournamentCommand, Result<JoinTournamentResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public JoinTournamentCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<JoinTournamentResultDto>> Handle(
        JoinTournamentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<JoinTournamentResultDto>.Unauthorized();
        }

        var tournament = await _context.Tournaments
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament == null)
        {
            return Result<JoinTournamentResultDto>.NotFound("Tournament not found");
        }

        if (tournament.Status == TournamentStatus.Completed)
        {
            return Result<JoinTournamentResultDto>.Failure("Tournament has ended");
        }

        if (tournament.Entries.Count >= tournament.MaxParticipants)
        {
            return Result<JoinTournamentResultDto>.Failure("Tournament is full");
        }

        var existingEntry = tournament.Entries.FirstOrDefault(e => e.UserId == _currentUser.UserId.Value);
        if (existingEntry != null)
        {
            return Result<JoinTournamentResultDto>.Failure("Already joined this tournament");
        }

        // Check and deduct entry fee
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);
        if (user == null)
        {
            return Result<JoinTournamentResultDto>.NotFound("User not found");
        }

        if (user.Coins < tournament.EntryFee)
        {
            return Result<JoinTournamentResultDto>.Failure("Insufficient coins");
        }

        user.Coins -= tournament.EntryFee;

        var entry = new TournamentEntry
        {
            TournamentId = tournament.Id,
            UserId = _currentUser.UserId.Value
        };

        _context.TournamentEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<JoinTournamentResultDto>.Success(
            new JoinTournamentResultDto(true, "Successfully joined tournament"));
    }
}
