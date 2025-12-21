using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class DailyLoginBonusConfiguration : IEntityTypeConfiguration<DailyLoginBonus>
{
    public void Configure(EntityTypeBuilder<DailyLoginBonus> builder)
    {
        builder.HasKey(d => d.Id);

        // One DailyLoginBonus per user
        builder.HasIndex(d => d.UserId).IsUnique();

        builder.Property(d => d.CurrentStreak)
            .HasDefaultValue(0);

        builder.Property(d => d.TotalClaims)
            .HasDefaultValue(0);

        // Relationship
        builder.HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<DailyLoginBonus>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
