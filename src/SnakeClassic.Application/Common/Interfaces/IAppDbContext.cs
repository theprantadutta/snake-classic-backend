using Microsoft.EntityFrameworkCore;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserPreferences> UserPreferences { get; }
    DbSet<FcmToken> FcmTokens { get; }
    DbSet<UserPremiumContent> UserPremiumContents { get; }
    DbSet<Score> Scores { get; }
    DbSet<GameReplay> GameReplays { get; }
    DbSet<Achievement> Achievements { get; }
    DbSet<UserAchievement> UserAchievements { get; }
    DbSet<Friendship> Friendships { get; }
    DbSet<Tournament> Tournaments { get; }
    DbSet<TournamentEntry> TournamentEntries { get; }
    DbSet<MultiplayerGame> MultiplayerGames { get; }
    DbSet<MultiplayerPlayer> MultiplayerPlayers { get; }
    DbSet<BattlePassSeason> BattlePassSeasons { get; }
    DbSet<UserBattlePassProgress> UserBattlePassProgresses { get; }
    DbSet<Purchase> Purchases { get; }
    DbSet<NotificationHistory> NotificationHistories { get; }
    DbSet<ScheduledJob> ScheduledJobs { get; }
    DbSet<DailyLoginBonus> DailyLoginBonuses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
