using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(50);

        // Composite unique index to prevent duplicate friendships
        builder.HasIndex(f => new { f.UserId, f.FriendId }).IsUnique();

        // Indexes for querying
        builder.HasIndex(f => new { f.UserId, f.Status });
        builder.HasIndex(f => new { f.FriendId, f.Status });

        // Relationships
        builder.HasOne(f => f.User)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Friend)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
