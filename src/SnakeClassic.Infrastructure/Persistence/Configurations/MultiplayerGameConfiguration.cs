using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class MultiplayerGameConfiguration : IEntityTypeConfiguration<MultiplayerGame>
{
    public void Configure(EntityTypeBuilder<MultiplayerGame> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.GameId).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Mode).HasConversion<string>().HasMaxLength(50);
        builder.Property(g => g.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(g => g.RoomCode).HasMaxLength(10);
        builder.Property(g => g.FoodPositions).HasColumnType("jsonb");
        builder.Property(g => g.PowerUps).HasColumnType("jsonb");
        builder.Property(g => g.GameSettings).HasColumnType("jsonb");

        builder.HasIndex(g => g.GameId).IsUnique();
        builder.HasIndex(g => g.RoomCode);
        builder.HasIndex(g => g.Status);
        builder.HasIndex(g => g.CreatedAt).IsDescending();

        // Relationship
        builder.HasMany(g => g.Players)
            .WithOne(p => p.Game)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
