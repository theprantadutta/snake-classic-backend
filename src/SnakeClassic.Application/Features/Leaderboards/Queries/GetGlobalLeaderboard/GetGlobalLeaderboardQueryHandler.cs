using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Leaderboards.DTOs;

namespace SnakeClassic.Application.Features.Leaderboards.Queries.GetGlobalLeaderboard;

public class GetGlobalLeaderboardQueryHandler : IRequestHandler<GetGlobalLeaderboardQuery, Result<LeaderboardResponseDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetGlobalLeaderboardQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<LeaderboardResponseDto>> Handle(GetGlobalLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var totalPlayers = await _context.Users.CountAsync(cancellationToken);

        var entries = await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.HighScore)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Select((u, index) => new LeaderboardEntryDto(
                request.Offset + index + 1,
                u.Id,
                u.Username,
                u.DisplayName,
                u.PhotoUrl,
                u.HighScore,
                u.Level,
                null
            ))
            .ToListAsync(cancellationToken);

        // Fix rank assignment (EF Core doesn't support index in Select properly)
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i] = entries[i] with { Rank = request.Offset + i + 1 };
        }

        int? currentUserRank = null;
        if (_currentUser.IsAuthenticated && _currentUser.UserId.HasValue)
        {
            var currentUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);

            if (currentUser != null)
            {
                var playersAbove = await _context.Users
                    .CountAsync(u => u.HighScore > currentUser.HighScore, cancellationToken);
                currentUserRank = playersAbove + 1;
            }
        }

        return Result<LeaderboardResponseDto>.Success(new LeaderboardResponseDto(
            entries,
            currentUserRank,
            totalPlayers
        ));
    }
}
