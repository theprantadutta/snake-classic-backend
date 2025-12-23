using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<FcmToken> FcmTokens => Set<FcmToken>();
    public DbSet<UserPremiumContent> UserPremiumContents => Set<UserPremiumContent>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<GameReplay> GameReplays => Set<GameReplay>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentEntry> TournamentEntries => Set<TournamentEntry>();
    public DbSet<MultiplayerGame> MultiplayerGames => Set<MultiplayerGame>();
    public DbSet<MultiplayerPlayer> MultiplayerPlayers => Set<MultiplayerPlayer>();
    public DbSet<BattlePassSeason> BattlePassSeasons => Set<BattlePassSeason>();
    public DbSet<UserBattlePassProgress> UserBattlePassProgresses => Set<UserBattlePassProgress>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<NotificationHistory> NotificationHistories => Set<NotificationHistory>();
    public DbSet<ScheduledJob> ScheduledJobs => Set<ScheduledJob>();
    public DbSet<DailyLoginBonus> DailyLoginBonuses => Set<DailyLoginBonus>();
    public DbSet<MatchmakingQueue> MatchmakingQueues => Set<MatchmakingQueue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply snake_case naming convention for PostgreSQL compatibility
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Table name
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            // Column names
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (columnName != null)
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }

            // Foreign key names
            foreach (var key in entity.GetForeignKeys())
            {
                var constraintName = key.GetConstraintName();
                if (constraintName != null)
                {
                    key.SetConstraintName(ToSnakeCase(constraintName));
                }
            }

            // Index names
            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (indexName != null)
                {
                    index.SetDatabaseName(ToSnakeCase(indexName));
                }
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (i > 0 && char.IsUpper(c))
            {
                result.Append('_');
            }
            result.Append(char.ToLower(c));
        }
        return result.ToString();
    }
}
