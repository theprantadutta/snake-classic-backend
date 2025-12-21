using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class GameReplayConfiguration : IEntityTypeConfiguration<GameReplay>
{
    public void Configure(EntityTypeBuilder<GameReplay> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).HasMaxLength(255);
        builder.Property(r => r.Description).HasColumnType("text");
        builder.Property(r => r.ReplayData).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.IsPublic);
        builder.HasIndex(r => r.CreatedAt).IsDescending();
    }
}
