using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirebaseUid).HasMaxLength(255);
        builder.HasIndex(u => u.FirebaseUid).IsUnique().HasFilter("firebase_uid IS NOT NULL");

        builder.Property(u => u.Email).HasMaxLength(255);
        builder.HasIndex(u => u.Email);

        builder.Property(u => u.Username).HasMaxLength(20);
        builder.HasIndex(u => u.Username).IsUnique().HasFilter("username IS NOT NULL");

        builder.Property(u => u.DisplayName).HasMaxLength(255);
        builder.Property(u => u.PhotoUrl).HasColumnType("text");
        builder.Property(u => u.StatusMessage).HasMaxLength(255);

        // Enum conversions stored as strings
        builder.Property(u => u.AuthProvider).HasConversion<string>().HasMaxLength(50);
        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(50);

        // Indexes for query performance
        builder.HasIndex(u => u.HighScore).IsDescending();
        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.Level);
        builder.HasIndex(u => u.LastSeen);

        // Relationships
        builder.HasOne(u => u.Preferences)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.PremiumContent)
            .WithOne(p => p.User)
            .HasForeignKey<UserPremiumContent>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Scores)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Achievements)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.FcmTokens)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.GameReplays)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Purchases)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
