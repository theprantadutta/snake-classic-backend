using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class UserBattlePassProgressConfiguration : IEntityTypeConfiguration<UserBattlePassProgress>
{
    public void Configure(EntityTypeBuilder<UserBattlePassProgress> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClaimedRewards).HasColumnType("jsonb");

        // Composite unique index - one progress per user per season
        builder.HasIndex(p => new { p.UserId, p.SeasonId }).IsUnique();

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.HasPremium);

        // Relationships
        builder.HasOne(p => p.User)
            .WithMany(u => u.BattlePassProgress)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
