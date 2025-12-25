using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.DailyChallenges.DTOs;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.DailyChallenges.Commands.UpdateProgress;

public class UpdateChallengeProgressCommandHandler : IRequestHandler<UpdateChallengeProgressCommand, UpdateChallengeProgressResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateChallengeProgressCommandHandler> _logger;

    public UpdateChallengeProgressCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateChallengeProgressCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UpdateChallengeProgressResponse> Handle(UpdateChallengeProgressCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return new UpdateChallengeProgressResponse(new List<DailyChallengeDto>(), new List<Guid>());
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newlyCompletedIds = new List<Guid>();

        // Get today's challenges matching the type
        var matchingChallenges = await _context.DailyChallenges
            .Where(c => c.ChallengeDate == today && c.Type == request.Type)
            .ToListAsync(cancellationToken);

        // Filter by game mode if applicable
        if (request.Type == ChallengeType.GameMode && !string.IsNullOrEmpty(request.GameMode))
        {
            matchingChallenges = matchingChallenges
                .Where(c => c.RequiredGameMode == null ||
                           c.RequiredGameMode.Equals(request.GameMode, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var updatedChallenges = new List<DailyChallengeDto>();

        foreach (var challenge in matchingChallenges)
        {
            // Get or create user progress
            var userChallenge = await _context.UserDailyChallenges
                .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.ChallengeId == challenge.Id, cancellationToken);

            if (userChallenge == null)
            {
                userChallenge = new UserDailyChallenge
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    ChallengeId = challenge.Id,
                    CurrentProgress = 0,
                    IsCompleted = false,
                    ClaimedReward = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserDailyChallenges.Add(userChallenge);
            }

            // Skip if already completed
            if (userChallenge.IsCompleted)
            {
                updatedChallenges.Add(new DailyChallengeDto(
                    Id: challenge.Id,
                    Title: challenge.Title,
                    Description: challenge.Description,
                    Type: challenge.Type,
                    Difficulty: challenge.Difficulty,
                    TargetValue: challenge.TargetValue,
                    CurrentProgress: userChallenge.CurrentProgress,
                    IsCompleted: true,
                    CoinReward: challenge.CoinReward,
                    XpReward: challenge.XpReward,
                    RequiredGameMode: challenge.RequiredGameMode,
                    ClaimedReward: userChallenge.ClaimedReward
                ));
                continue;
            }

            // Update progress based on challenge type
            var newProgress = request.Type switch
            {
                // For score/survival, take the max value (best attempt)
                ChallengeType.Score or ChallengeType.Survival =>
                    Math.Max(userChallenge.CurrentProgress, request.Value),

                // For food/games, accumulate
                ChallengeType.FoodEaten or ChallengeType.GamesPlayed or ChallengeType.GameMode =>
                    userChallenge.CurrentProgress + request.Value,

                _ => userChallenge.CurrentProgress + request.Value
            };

            userChallenge.CurrentProgress = newProgress;
            userChallenge.UpdatedAt = DateTime.UtcNow;

            // Check if completed
            if (newProgress >= challenge.TargetValue && !userChallenge.IsCompleted)
            {
                userChallenge.IsCompleted = true;
                userChallenge.CompletedAt = DateTime.UtcNow;
                newlyCompletedIds.Add(challenge.Id);

                _logger.LogInformation(
                    "User {UserId} completed challenge {ChallengeId} ({Title})",
                    userId.Value, challenge.Id, challenge.Title);
            }

            updatedChallenges.Add(new DailyChallengeDto(
                Id: challenge.Id,
                Title: challenge.Title,
                Description: challenge.Description,
                Type: challenge.Type,
                Difficulty: challenge.Difficulty,
                TargetValue: challenge.TargetValue,
                CurrentProgress: userChallenge.CurrentProgress,
                IsCompleted: userChallenge.IsCompleted,
                CoinReward: challenge.CoinReward,
                XpReward: challenge.XpReward,
                RequiredGameMode: challenge.RequiredGameMode,
                ClaimedReward: userChallenge.ClaimedReward
            ));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateChallengeProgressResponse(
            UpdatedChallenges: updatedChallenges,
            NewlyCompletedIds: newlyCompletedIds
        );
    }
}
