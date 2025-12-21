using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AchievementId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(255).IsRequired();
        builder.Property(a => a.Description).HasColumnType("text");
        builder.Property(a => a.Icon).HasMaxLength(255);
        builder.Property(a => a.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.Tier).HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.RequirementType).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(a => a.AchievementId).IsUnique();
        builder.HasIndex(a => a.Category);
        builder.HasIndex(a => a.IsActive);
    }
}
