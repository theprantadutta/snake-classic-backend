using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class BattlePassSeasonConfiguration : IEntityTypeConfiguration<BattlePassSeason>
{
    public void Configure(EntityTypeBuilder<BattlePassSeason> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SeasonId).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(255).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");
        builder.Property(s => s.Theme).HasMaxLength(100);
        builder.Property(s => s.ThemeColor).HasMaxLength(20);
        builder.Property(s => s.Price).HasPrecision(10, 2);
        builder.Property(s => s.LevelsConfig).HasColumnType("jsonb");
        builder.Property(s => s.ExtraData).HasColumnType("jsonb");

        builder.HasIndex(s => s.SeasonId).IsUnique();
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => new { s.StartDate, s.EndDate });

        // Relationship
        builder.HasMany(s => s.UserProgress)
            .WithOne(p => p.Season)
            .HasForeignKey(p => p.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
