using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class UserPremiumContentConfiguration : IEntityTypeConfiguration<UserPremiumContent>
{
    public void Configure(EntityTypeBuilder<UserPremiumContent> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PremiumTier).HasMaxLength(50);
        builder.Property(p => p.OwnedThemes).HasColumnType("jsonb");
        builder.Property(p => p.OwnedPowerups).HasColumnType("jsonb");
        builder.Property(p => p.OwnedCosmetics).HasColumnType("jsonb");
        builder.Property(p => p.TournamentEntries).HasColumnType("jsonb");

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.SubscriptionActive);
    }
}
