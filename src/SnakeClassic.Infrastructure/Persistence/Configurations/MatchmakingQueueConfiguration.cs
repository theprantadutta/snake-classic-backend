using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class MatchmakingQueueConfiguration : IEntityTypeConfiguration<MatchmakingQueue>
{
    public void Configure(EntityTypeBuilder<MatchmakingQueue> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Mode)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(q => q.ConnectionId)
            .HasMaxLength(100);

        // Index for efficient matchmaking queries
        builder.HasIndex(q => new { q.Mode, q.DesiredPlayers, q.IsMatched, q.QueuedAt });

        // Unique constraint - one queue entry per user
        builder.HasIndex(q => q.UserId)
            .IsUnique()
            .HasFilter("is_matched = false");

        // Relationship
        builder.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
