using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Scores.DTOs;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Scores.Commands.SubmitScore;

public class SubmitScoreCommandHandler : IRequestHandler<SubmitScoreCommand, Result<ScoreDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SubmitScoreCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ScoreDto>> Handle(SubmitScoreCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ScoreDto>.Unauthorized();
        }

        // Check idempotency key for duplicate prevention
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingScore = await _context.Scores
                .FirstOrDefaultAsync(s => s.IdempotencyKey == request.IdempotencyKey, cancellationToken);

            if (existingScore != null)
            {
                return Result<ScoreDto>.Success(new ScoreDto(
                    existingScore.Id,
                    existingScore.ScoreValue,
                    existingScore.GameDurationSeconds,
                    existingScore.FoodsEaten,
                    existingScore.GameMode,
                    existingScore.Difficulty,
                    existingScore.CreatedAt
                ));
            }
        }

        // Parse enums
        if (!Enum.TryParse<GameMode>(request.GameMode, true, out var gameMode))
        {
            gameMode = GameMode.Classic;
        }
        if (!Enum.TryParse<Difficulty>(request.Difficulty, true, out var difficulty))
        {
            difficulty = Difficulty.Normal;
        }

        // Create score
        var score = new Score
        {
            UserId = _currentUser.UserId.Value,
            ScoreValue = request.Score,
            GameDurationSeconds = request.GameDurationSeconds,
            FoodsEaten = request.FoodsEaten,
            GameMode = gameMode,
            Difficulty = difficulty,
            IdempotencyKey = request.IdempotencyKey,
            GameData = request.GameData,
            PlayedAt = request.PlayedAt
        };

        _context.Scores.Add(score);

        // Update user's high score if this is higher
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);
        if (user != null && request.Score > user.HighScore)
        {
            user.HighScore = request.Score;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ScoreDto>.Created(new ScoreDto(
            score.Id,
            score.ScoreValue,
            score.GameDurationSeconds,
            score.FoodsEaten,
            score.GameMode,
            score.Difficulty,
            score.CreatedAt
        ));
    }
}
