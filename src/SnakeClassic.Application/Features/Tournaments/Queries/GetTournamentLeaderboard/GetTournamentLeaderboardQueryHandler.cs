using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Tournaments.DTOs;

namespace SnakeClassic.Application.Features.Tournaments.Queries.GetTournamentLeaderboard;

public class GetTournamentLeaderboardQueryHandler
    : IRequestHandler<GetTournamentLeaderboardQuery, Result<TournamentLeaderboardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTournamentLeaderboardQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<TournamentLeaderboardDto>> Handle(
        GetTournamentLeaderboardQuery request,
        CancellationToken cancellationToken)
    {
        var tournament = await _context.Tournaments
            .AsNoTracking()
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament == null)
        {
            return Result<TournamentLeaderboardDto>.NotFound("Tournament not found");
        }

        var entries = await _context.TournamentEntries
            .AsNoTracking()
            .Include(te => te.User)
            .Where(te => te.TournamentId == request.TournamentId)
            .OrderByDescending(te => te.BestScore)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var leaderboardEntries = entries.Select((e, index) => new TournamentEntryDto(
            index + 1,
            e.UserId,
            e.User.Username,
            e.User.DisplayName,
            e.User.PhotoUrl,
            e.BestScore,
            e.GamesPlayed,
            e.PrizeClaimed
        )).ToList();

        int? currentUserRank = null;
        var isJoined = false;
        if (_currentUser.IsAuthenticated && _currentUser.UserId.HasValue)
        {
            var userEntry = await _context.TournamentEntries
                .FirstOrDefaultAsync(te =>
                    te.TournamentId == request.TournamentId &&
                    te.UserId == _currentUser.UserId.Value, cancellationToken);

            if (userEntry != null)
            {
                isJoined = true;
                var playersAbove = await _context.TournamentEntries
                    .CountAsync(te =>
                        te.TournamentId == request.TournamentId &&
                        te.BestScore > userEntry.BestScore, cancellationToken);
                currentUserRank = playersAbove + 1;
            }
        }

        return Result<TournamentLeaderboardDto>.Success(new TournamentLeaderboardDto(
            new TournamentDto(
                tournament.Id,
                tournament.TournamentId,
                tournament.Name,
                tournament.Description,
                tournament.Status,
                tournament.StartDate,
                tournament.EndDate,
                tournament.EntryFee,
                tournament.PrizePool,
                tournament.MaxParticipants,
                tournament.Entries.Count,
                isJoined
            ),
            leaderboardEntries,
            currentUserRank
        ));
    }
}
