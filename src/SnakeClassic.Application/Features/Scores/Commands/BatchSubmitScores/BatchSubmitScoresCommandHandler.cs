using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Scores.DTOs;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Scores.Commands.BatchSubmitScores;

public class BatchSubmitScoresCommandHandler : IRequestHandler<BatchSubmitScoresCommand, Result<BatchScoreResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public BatchSubmitScoresCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<BatchScoreResultDto>> Handle(BatchSubmitScoresCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BatchScoreResultDto>.Unauthorized();
        }

        var processed = 0;
        var skippedKeys = new List<string>();

        // Get existing idempotency keys
        var existingKeys = await _context.Scores
            .Where(s => s.UserId == _currentUser.UserId.Value && s.IdempotencyKey != null)
            .Select(s => s.IdempotencyKey!)
            .ToListAsync(cancellationToken);

        var existingKeySet = new HashSet<string>(existingKeys);
        var highestScore = 0;

        foreach (var scoreDto in request.Scores)
        {
            // Check idempotency
            if (!string.IsNullOrEmpty(scoreDto.IdempotencyKey) && existingKeySet.Contains(scoreDto.IdempotencyKey))
            {
                skippedKeys.Add(scoreDto.IdempotencyKey);
                continue;
            }

            if (!Enum.TryParse<GameMode>(scoreDto.GameMode, true, out var gameMode))
            {
                gameMode = GameMode.Classic;
            }
            if (!Enum.TryParse<Difficulty>(scoreDto.Difficulty, true, out var difficulty))
            {
                difficulty = Difficulty.Normal;
            }

            var score = new Score
            {
                UserId = _currentUser.UserId.Value,
                ScoreValue = scoreDto.Score,
                GameDurationSeconds = scoreDto.GameDurationSeconds,
                FoodsEaten = scoreDto.FoodsEaten,
                GameMode = gameMode,
                Difficulty = difficulty,
                IdempotencyKey = scoreDto.IdempotencyKey,
                GameData = scoreDto.GameData
            };

            _context.Scores.Add(score);

            if (scoreDto.Score > highestScore)
            {
                highestScore = scoreDto.Score;
            }

            processed++;
        }

        // Update user's high score if needed
        if (highestScore > 0)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);
            if (user != null && highestScore > user.HighScore)
            {
                user.HighScore = highestScore;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<BatchScoreResultDto>.Success(new BatchScoreResultDto(
            processed,
            skippedKeys.Count,
            skippedKeys
        ));
    }
}
