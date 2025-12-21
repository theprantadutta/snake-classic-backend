using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TournamentId).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(255).IsRequired();
        builder.Property(t => t.Description).HasColumnType("text");
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.PrizeDistribution).HasColumnType("jsonb");
        builder.Property(t => t.Rules).HasColumnType("jsonb");

        builder.HasIndex(t => t.TournamentId).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.StartDate);
        builder.HasIndex(t => t.EndDate);
        builder.HasIndex(t => new { t.Status, t.StartDate });

        // Relationship
        builder.HasMany(t => t.Entries)
            .WithOne(e => e.Tournament)
            .HasForeignKey(e => e.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
