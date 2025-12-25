using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class UserDailyChallengeConfiguration : IEntityTypeConfiguration<UserDailyChallenge>
{
    public void Configure(EntityTypeBuilder<UserDailyChallenge> builder)
    {
        builder.HasKey(udc => udc.Id);

        // Composite unique index to prevent duplicates
        builder.HasIndex(udc => new { udc.UserId, udc.ChallengeId }).IsUnique();

        // Index for querying user's challenges
        builder.HasIndex(udc => udc.UserId);

        // Index for querying completed/unclaimed challenges
        builder.HasIndex(udc => new { udc.UserId, udc.IsCompleted, udc.ClaimedReward });

        // Relationships
        builder.HasOne(udc => udc.User)
            .WithMany(u => u.DailyChallenges)
            .HasForeignKey(udc => udc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(udc => udc.Challenge)
            .WithMany(dc => dc.UserChallenges)
            .HasForeignKey(udc => udc.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
