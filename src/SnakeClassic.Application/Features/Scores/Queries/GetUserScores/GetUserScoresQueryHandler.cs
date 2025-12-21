using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Scores.DTOs;

namespace SnakeClassic.Application.Features.Scores.Queries.GetUserScores;

public class GetUserScoresQueryHandler : IRequestHandler<GetUserScoresQuery, Result<List<ScoreDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserScoresQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ScoreDto>>> Handle(GetUserScoresQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<List<ScoreDto>>.Unauthorized();
        }

        var scores = await _context.Scores
            .AsNoTracking()
            .Where(s => s.UserId == _currentUser.UserId.Value)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Select(s => new ScoreDto(
                s.Id,
                s.ScoreValue,
                s.GameDurationSeconds,
                s.FoodsEaten,
                s.GameMode,
                s.Difficulty,
                s.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<ScoreDto>>.Success(scores);
    }
}
