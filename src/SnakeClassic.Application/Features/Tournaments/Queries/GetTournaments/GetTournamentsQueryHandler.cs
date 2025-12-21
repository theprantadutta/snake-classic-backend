using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Tournaments.DTOs;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Tournaments.Queries.GetTournaments;

public class GetTournamentsQueryHandler : IRequestHandler<GetTournamentsQuery, Result<TournamentsResponseDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTournamentsQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<TournamentsResponseDto>> Handle(GetTournamentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tournaments.AsNoTracking();

        if (request.ActiveOnly)
        {
            query = query.Where(t => t.Status == TournamentStatus.Active || t.Status == TournamentStatus.Upcoming);
        }

        var tournaments = await query
            .Include(t => t.Entries)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);

        var joinedTournamentIds = new HashSet<Guid>();
        if (_currentUser.IsAuthenticated && _currentUser.UserId.HasValue)
        {
            var joined = await _context.TournamentEntries
                .Where(te => te.UserId == _currentUser.UserId.Value)
                .Select(te => te.TournamentId)
                .ToListAsync(cancellationToken);
            joinedTournamentIds = joined.ToHashSet();
        }

        var result = tournaments.Select(t => new TournamentDto(
            t.Id,
            t.TournamentId,
            t.Name,
            t.Description,
            t.Status,
            t.StartDate,
            t.EndDate,
            t.EntryFee,
            t.PrizePool,
            t.MaxParticipants,
            t.Entries.Count,
            joinedTournamentIds.Contains(t.Id)
        )).ToList();

        return Result<TournamentsResponseDto>.Success(new TournamentsResponseDto(result));
    }
}
