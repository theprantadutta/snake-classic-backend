using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Infrastructure.Services.BackgroundJobs;

public interface IDailyChallengeJobService
{
    Task GenerateDailyChallenges();
    Task SendMorningReminder();
    Task SendEveningReminder();
}

public class DailyChallengeJobService : IDailyChallengeJobService
{
    private readonly IAppDbContext _context;
    private readonly IFirebaseMessagingService _firebaseMessaging;
    private readonly ILogger<DailyChallengeJobService> _logger;

    // Challenge templates pool
    private static readonly List<ChallengeTemplate> ChallengeTemplates = new()
    {
        // Score-based challenges
        new(ChallengeType.Score, ChallengeDifficulty.Easy, "Beginner Score", "Score at least 200 points in a single game", 200, 10, 25),
        new(ChallengeType.Score, ChallengeDifficulty.Medium, "Skilled Player", "Score at least 500 points in a single game", 500, 25, 50),
        new(ChallengeType.Score, ChallengeDifficulty.Hard, "Score Master", "Score at least 1000 points in a single game", 1000, 50, 100),

        // Food-based challenges
        new(ChallengeType.FoodEaten, ChallengeDifficulty.Easy, "Hungry Snake", "Eat 15 foods today", 15, 10, 25),
        new(ChallengeType.FoodEaten, ChallengeDifficulty.Medium, "Feast Mode", "Eat 30 foods today", 30, 25, 50),
        new(ChallengeType.FoodEaten, ChallengeDifficulty.Hard, "Insatiable", "Eat 50 foods today", 50, 50, 100),

        // Survival challenges
        new(ChallengeType.Survival, ChallengeDifficulty.Easy, "Survivor", "Survive for 60 seconds in a single game", 60, 10, 25),
        new(ChallengeType.Survival, ChallengeDifficulty.Medium, "Endurance", "Survive for 120 seconds in a single game", 120, 25, 50),
        new(ChallengeType.Survival, ChallengeDifficulty.Hard, "Immortal", "Survive for 180 seconds in a single game", 180, 50, 100),

        // Games played challenges
        new(ChallengeType.GamesPlayed, ChallengeDifficulty.Easy, "Casual Player", "Play 2 games today", 2, 10, 25),
        new(ChallengeType.GamesPlayed, ChallengeDifficulty.Medium, "Dedicated", "Play 5 games today", 5, 25, 50),
        new(ChallengeType.GamesPlayed, ChallengeDifficulty.Hard, "Snake Addict", "Play 10 games today", 10, 50, 100),

        // Game mode specific
        new(ChallengeType.GameMode, ChallengeDifficulty.Easy, "Classic Lover", "Play 1 game in Classic mode", 1, 10, 25, "classic"),
        new(ChallengeType.GameMode, ChallengeDifficulty.Medium, "Zen Master", "Play 2 games in Zen mode", 2, 25, 50, "zen"),
        new(ChallengeType.GameMode, ChallengeDifficulty.Hard, "Speed Demon", "Play 3 games in Speed mode", 3, 50, 100, "speed"),
    };

    public DailyChallengeJobService(
        IAppDbContext context,
        IFirebaseMessagingService firebaseMessaging,
        ILogger<DailyChallengeJobService> logger)
    {
        _context = context;
        _firebaseMessaging = firebaseMessaging;
        _logger = logger;
    }

    public async Task GenerateDailyChallenges()
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Check if challenges already exist for today
            var existingCount = await _context.DailyChallenges
                .CountAsync(c => c.ChallengeDate == today);

            if (existingCount >= 3)
            {
                _logger.LogInformation("Daily challenges already exist for {Date}", today);
                return;
            }

            // Delete any partial challenges for today
            var partialChallenges = await _context.DailyChallenges
                .Where(c => c.ChallengeDate == today)
                .ToListAsync();

            if (partialChallenges.Any())
            {
                _context.DailyChallenges.RemoveRange(partialChallenges);
            }

            // Select one challenge per difficulty
            var random = new Random();
            var selectedChallenges = new List<DailyChallenge>();

            foreach (var difficulty in new[] { ChallengeDifficulty.Easy, ChallengeDifficulty.Medium, ChallengeDifficulty.Hard })
            {
                var templates = ChallengeTemplates.Where(t => t.Difficulty == difficulty).ToList();
                var selected = templates[random.Next(templates.Count)];

                selectedChallenges.Add(new DailyChallenge
                {
                    Id = Guid.NewGuid(),
                    ChallengeDate = today,
                    Type = selected.Type,
                    Difficulty = difficulty,
                    Title = selected.Title,
                    Description = selected.Description,
                    TargetValue = selected.TargetValue,
                    CoinReward = selected.CoinReward,
                    XpReward = selected.XpReward,
                    RequiredGameMode = selected.RequiredGameMode,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.DailyChallenges.AddRange(selectedChallenges);
            await _context.SaveChangesAsync(default);

            _logger.LogInformation(
                "Generated {Count} daily challenges for {Date}: {Challenges}",
                selectedChallenges.Count, today,
                string.Join(", ", selectedChallenges.Select(c => c.Title)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily challenges");
        }
    }

    public async Task SendMorningReminder()
    {
        try
        {
            _logger.LogInformation("Sending morning daily challenge reminder");

            var payload = new NotificationPayload
            {
                Title = "🎯 New Daily Challenges!",
                Body = "Complete today's 3 challenges to earn coins and bonus rewards!",
                Route = "daily_challenge",
                Priority = "high",
                Data = new Dictionary<string, string>
                {
                    ["action"] = "daily_challenge",
                    ["route"] = "daily_challenge",
                    ["time"] = "morning"
                }
            };

            await _firebaseMessaging.SendToTopicAsync(payload, "daily_challenge");

            _logger.LogInformation("Morning daily challenge reminder sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send morning daily challenge reminder");
        }
    }

    public async Task SendEveningReminder()
    {
        try
        {
            _logger.LogInformation("Sending evening daily challenge reminder");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Get today's challenges
            var challengeIds = await _context.DailyChallenges
                .Where(c => c.ChallengeDate == today)
                .Select(c => c.Id)
                .ToListAsync();

            if (!challengeIds.Any())
            {
                _logger.LogWarning("No challenges found for today, skipping evening reminder");
                return;
            }

            // Get users who haven't completed all challenges
            var usersWithIncomplete = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    CompletedCount = _context.UserDailyChallenges
                        .Count(uc => uc.UserId == u.Id &&
                                    challengeIds.Contains(uc.ChallengeId) &&
                                    uc.IsCompleted)
                })
                .Where(x => x.CompletedCount < challengeIds.Count)
                .CountAsync();

            _logger.LogInformation(
                "{Count} users haven't completed all daily challenges",
                usersWithIncomplete);

            // Send topic-based notification (users subscribed to daily_challenge topic)
            var payload = new NotificationPayload
            {
                Title = "⏰ Challenges Expiring Soon!",
                Body = "Don't miss today's rewards - complete your remaining challenges!",
                Route = "daily_challenge",
                Priority = "high",
                Data = new Dictionary<string, string>
                {
                    ["action"] = "daily_challenge",
                    ["route"] = "daily_challenge",
                    ["time"] = "evening",
                    ["urgent"] = "true"
                }
            };

            await _firebaseMessaging.SendToTopicAsync(payload, "daily_challenge");

            _logger.LogInformation("Evening daily challenge reminder sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send evening daily challenge reminder");
        }
    }

    private record ChallengeTemplate(
        ChallengeType Type,
        ChallengeDifficulty Difficulty,
        string Title,
        string Description,
        int TargetValue,
        int CoinReward,
        int XpReward,
        string? RequiredGameMode = null
    );
}
