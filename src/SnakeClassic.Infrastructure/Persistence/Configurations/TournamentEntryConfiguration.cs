using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class TournamentEntryConfiguration : IEntityTypeConfiguration<TournamentEntry>
{
    public void Configure(EntityTypeBuilder<TournamentEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PrizeAmount).HasColumnType("jsonb");

        // Composite unique index - one entry per user per tournament
        builder.HasIndex(e => new { e.TournamentId, e.UserId }).IsUnique();

        // Index for tournament leaderboard queries
        builder.HasIndex(e => new { e.TournamentId, e.BestScore }).IsDescending(false, true);

        builder.HasIndex(e => e.UserId);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.TournamentEntries)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
