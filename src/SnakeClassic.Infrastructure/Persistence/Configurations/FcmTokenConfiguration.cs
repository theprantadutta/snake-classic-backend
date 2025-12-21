using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class FcmTokenConfiguration : IEntityTypeConfiguration<FcmToken>
{
    public void Configure(EntityTypeBuilder<FcmToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Platform).HasMaxLength(50);
        builder.Property(t => t.SubscribedTopics).HasColumnType("jsonb");

        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
