using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class MultiplayerPlayerConfiguration : IEntityTypeConfiguration<MultiplayerPlayer>
{
    public void Configure(EntityTypeBuilder<MultiplayerPlayer> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Direction).HasMaxLength(10);
        builder.Property(p => p.SnakeColor).HasMaxLength(20);
        builder.Property(p => p.SnakePositions).HasColumnType("jsonb");
        builder.Property(p => p.ConnectionId).HasMaxLength(100);

        // Ignore computed property
        builder.Ignore(p => p.CanReconnect);

        // Composite unique index
        builder.HasIndex(p => new { p.GameId, p.UserId }).IsUnique();

        builder.HasIndex(p => p.GameId);
        builder.HasIndex(p => p.UserId);

        // Relationships
        builder.HasOne(p => p.User)
            .WithMany(u => u.MultiplayerGames)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
