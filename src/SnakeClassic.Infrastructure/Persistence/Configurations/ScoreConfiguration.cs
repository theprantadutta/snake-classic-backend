using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.GameMode).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Difficulty).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.GameData).HasColumnType("jsonb");
        builder.Property(s => s.IdempotencyKey).HasMaxLength(100);

        // Indexes for leaderboard queries
        builder.HasIndex(s => s.ScoreValue).IsDescending();
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => new { s.GameMode, s.Difficulty });

        // Composite index for user's scores
        builder.HasIndex(s => new { s.UserId, s.CreatedAt }).IsDescending(false, true);

        // Unique index for idempotency (offline sync)
        builder.HasIndex(s => s.IdempotencyKey).IsUnique().HasFilter("idempotency_key IS NOT NULL");

        // Relationship
        builder.HasOne(s => s.Replay)
            .WithOne(r => r.Score)
            .HasForeignKey<GameReplay>(r => r.ScoreId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
