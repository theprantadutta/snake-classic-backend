using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Achievements.Commands.SeedAchievements;

public class SeedAchievementsCommandHandler : IRequestHandler<SeedAchievementsCommand, Result<SeedAchievementsResultDto>>
{
    private readonly IAppDbContext _context;

    public SeedAchievementsCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SeedAchievementsResultDto>> Handle(SeedAchievementsCommand request, CancellationToken cancellationToken)
    {
        var defaultAchievements = GetDefaultAchievements();
        var existingAchievements = await _context.Achievements
            .ToDictionaryAsync(a => a.AchievementId, cancellationToken);

        int created = 0;
        int updated = 0;

        foreach (var achievement in defaultAchievements)
        {
            if (existingAchievements.TryGetValue(achievement.AchievementId, out var existing))
            {
                // Update existing achievement
                existing.Name = achievement.Name;
                existing.Description = achievement.Description;
                existing.Icon = achievement.Icon;
                existing.Category = achievement.Category;
                existing.Tier = achievement.Tier;
                existing.RequirementType = achievement.RequirementType;
                existing.RequirementValue = achievement.RequirementValue;
                existing.XpReward = achievement.XpReward;
                existing.CoinReward = achievement.CoinReward;
                updated++;
            }
            else
            {
                // Create new achievement
                _context.Achievements.Add(achievement);
                created++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SeedAchievementsResultDto>.Success(
            new SeedAchievementsResultDto(created, updated, defaultAchievements.Count));
    }

    private static List<Achievement> GetDefaultAchievements()
    {
        return new List<Achievement>
        {
            // Score Achievements
            new Achievement
            {
                AchievementId = "first_bite",
                Name = "First Bite",
                Description = "Score your first point",
                Icon = "star",
                Category = AchievementCategory.Score,
                Tier = AchievementTier.Bronze,
                RequirementType = RequirementType.Score,
                RequirementValue = 1,
                XpReward = 10,
                CoinReward = 5
            },
            new Achievement
            {
                AchievementId = "getting_started",
                Name = "Getting Started",
                Description = "Score 100 points",
                Icon = "emoji_events",
                Category = AchievementCategory.Score,
                Tier = AchievementTier.Bronze,
                RequirementType = RequirementType.Score,
                RequirementValue = 100,
                XpReward = 25,
                CoinReward = 10
            },
            new Achievement
            {
                AchievementId = "high_scorer",
                Name = "High Scorer",
                Description = "Score 500 points in a single game",
                Icon = "trending_up",
                Category = AchievementCategory.Score,
                Tier = AchievementTier.Silver,
                RequirementType = RequirementType.Score,
                RequirementValue = 500,
                XpReward = 50,
                CoinReward = 25
            },
            new Achievement
            {
                AchievementId = "master_scorer",
                Name = "Master Scorer",
                Description = "Score 1000 points in a single game",
                Icon = "military_tech",
                Category = AchievementCategory.Score,
                Tier = AchievementTier.Gold,
                RequirementType = RequirementType.Score,
                RequirementValue = 1000,
                XpReward = 100,
                CoinReward = 50
            },
            new Achievement
            {
                AchievementId = "legendary_scorer",
                Name = "Legendary Scorer",
                Description = "Score 2000 points in a single game",
                Icon = "diamond",
                Category = AchievementCategory.Score,
                Tier = AchievementTier.Platinum,
                RequirementType = RequirementType.Score,
                RequirementValue = 2000,
                XpReward = 200,
                CoinReward = 100
            },

            // Games Played Achievements
            new Achievement
            {
                AchievementId = "first_game",
                Name = "First Game",
                Description = "Play your first game",
                Icon = "play_arrow",
                Category = AchievementCategory.Games,
                Tier = AchievementTier.Bronze,
                RequirementType = RequirementType.Count,
                RequirementValue = 1,
                XpReward = 10,
                CoinReward = 5
            },
            new Achievement
            {
                AchievementId = "regular_player",
                Name = "Regular Player",
                Description = "Play 10 games",
                Icon = "videogame_asset",
                Category = AchievementCategory.Games,
                Tier = AchievementTier.Bronze,
                RequirementType = RequirementType.Count,
                RequirementValue = 10,
                XpReward = 25,
                CoinReward = 10
            },
            new Achievement
            {
                AchievementId = "dedicated_player",
                Name = "Dedicated Player",
                Description = "Play 50 games",
                Icon = "sports_esports",
                Category = AchievementCategory.Games,
                Tier = AchievementTier.Silver,
                RequirementType = RequirementType.Count,
                RequirementValue = 50,
                XpReward = 50,
                CoinReward = 25
            },
            new Achievement
            {
                AchievementId = "snake_enthusiast",
                Name = "Snake Enthusiast",
                Description = "Play 100 games",
                Icon = "gamepad",
                Category = AchievementCategory.Games,
                Tier = AchievementTier.Gold,
                RequirementType = RequirementType.Count,
                RequirementValue = 100,
                XpReward = 100,
                CoinReward = 50
            },
            new Achievement
            {
                AchievementId = "snake_addict",
                Name = "Snake Addict",
                Description = "Play 500 games",
                Icon = "sports_esports",
                Category = AchievementCategory.Games,
                Tier = AchievementTier.Platinum,
                RequirementType = RequirementType.Count,
                RequirementValue = 500,
                XpReward = 250,
                CoinReward = 125
            },

            // Survival Achievements
            new Achievement
            {
                AchievementId = "survivor",
                Name = "Survivor",
                Description = "Survive for 60 seconds",
                Icon = "timer",
                Category = AchievementCategory.Survival,
                Tier = AchievementTier.Bronze,
                RequirementType = RequirementType.Time,
                RequirementValue = 60,
                XpReward = 15,
                CoinReward = 8
            },
            new Achievement
            {
                AchievementId = "endurance",
                Name = "Endurance",
                Description = "Survive for 2 minutes",
                Icon = "schedule",
                Category = AchievementCategory.Survival,
                Tier = AchievementTier.Silver,
                RequirementType = RequirementType.Time,
                RequirementValue = 120,
                XpReward = 30,
                CoinReward = 15
            },
            new Achievement
            {
                AchievementId = "marathon",
                Name = "Marathon",
                Description = "Survive for 5 minutes",
                Icon = "hourglass_full",
                Category = AchievementCategory.Survival,
                Tier = AchievementTier.Gold,
                RequirementType = RequirementType.Time,
                RequirementValue = 300,
                XpReward = 75,
                CoinReward = 40
            },

            // Special Achievements
            new Achievement
            {
                AchievementId = "no_walls",
                Name = "Wall Avoider",
                Description = "Play 5 games without hitting walls",
                Icon = "shield",
                Category = AchievementCategory.Special,
                Tier = AchievementTier.Silver,
                RequirementType = RequirementType.Count,
                RequirementValue = 5,
                XpReward = 60,
                CoinReward = 30
            },
            new Achievement
            {
                AchievementId = "speedster",
                Name = "Speedster",
                Description = "Reach level 10 (max speed)",
                Icon = "speed",
                Category = AchievementCategory.Special,
                Tier = AchievementTier.Gold,
                RequirementType = RequirementType.Count,
                RequirementValue = 10,
                XpReward = 80,
                CoinReward = 40
            },
            new Achievement
            {
                AchievementId = "perfectionist",
                Name = "Perfectionist",
                Description = "Complete a game without hitting yourself",
                Icon = "verified",
                Category = AchievementCategory.Special,
                Tier = AchievementTier.Gold,
                RequirementType = RequirementType.Count,
                RequirementValue = 1,
                XpReward = 90,
                CoinReward = 45
            },
            new Achievement
            {
                AchievementId = "all_food_types",
                Name = "Gourmet",
                Description = "Eat all 3 types of food in a single game",
                Icon = "restaurant",
                Category = AchievementCategory.Special,
                Tier = AchievementTier.Silver,
                RequirementType = RequirementType.Count,
                RequirementValue = 1,
                XpReward = 40,
                CoinReward = 20
            }
        };
    }
}
