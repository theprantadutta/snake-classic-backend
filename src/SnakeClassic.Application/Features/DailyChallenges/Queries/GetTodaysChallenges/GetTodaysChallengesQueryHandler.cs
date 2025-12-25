using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.DailyChallenges.DTOs;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.DailyChallenges.Queries.GetTodaysChallenges;

public class GetTodaysChallengesQueryHandler : IRequestHandler<GetTodaysChallengesQuery, DailyChallengesResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTodaysChallengesQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<DailyChallengesResponse> Handle(GetTodaysChallengesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get today's challenges
        var challenges = await _context.DailyChallenges
            .Where(c => c.ChallengeDate == today)
            .OrderBy(c => c.Difficulty)
            .ToListAsync(cancellationToken);

        // Get user's progress for these challenges
        var challengeIds = challenges.Select(c => c.Id).ToList();
        var userProgress = userId.HasValue
            ? await _context.UserDailyChallenges
                .Where(uc => uc.UserId == userId.Value && challengeIds.Contains(uc.ChallengeId))
                .ToDictionaryAsync(uc => uc.ChallengeId, cancellationToken)
            : new Dictionary<Guid, UserDailyChallenge>();

        // Build response
        var challengeDtos = challenges.Select(c =>
        {
            var progress = userProgress.GetValueOrDefault(c.Id);
            return new DailyChallengeDto(
                Id: c.Id,
                Title: c.Title,
                Description: c.Description,
                Type: c.Type,
                Difficulty: c.Difficulty,
                TargetValue: c.TargetValue,
                CurrentProgress: progress?.CurrentProgress ?? 0,
                IsCompleted: progress?.IsCompleted ?? false,
                CoinReward: c.CoinReward,
                XpReward: c.XpReward,
                RequiredGameMode: c.RequiredGameMode,
                ClaimedReward: progress?.ClaimedReward ?? false
            );
        }).ToList();

        var completedCount = challengeDtos.Count(c => c.IsCompleted);
        var allCompleted = completedCount == challengeDtos.Count && challengeDtos.Count > 0;
        var bonusCoins = allCompleted ? 25 : 0; // Bonus for completing all challenges

        return new DailyChallengesResponse(
            Challenges: challengeDtos,
            CompletedCount: completedCount,
            TotalCount: challengeDtos.Count,
            AllCompleted: allCompleted,
            BonusCoins: bonusCoins
        );
    }
}
