using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class DailyChallengeConfiguration : IEntityTypeConfiguration<DailyChallenge>
{
    public void Configure(EntityTypeBuilder<DailyChallenge> builder)
    {
        builder.HasKey(dc => dc.Id);

        // Index on date for efficient querying of today's challenges
        builder.HasIndex(dc => dc.ChallengeDate);

        // Composite index for getting challenges by date and difficulty
        builder.HasIndex(dc => new { dc.ChallengeDate, dc.Difficulty });

        builder.Property(dc => dc.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(dc => dc.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(dc => dc.RequiredGameMode)
            .HasMaxLength(50);

        // Enum conversions
        builder.Property(dc => dc.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(dc => dc.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
