using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class NotificationHistoryConfiguration : IEntityTypeConfiguration<NotificationHistory>
{
    public void Configure(EntityTypeBuilder<NotificationHistory> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.NotificationId).HasMaxLength(255);
        builder.Property(n => n.Title).HasMaxLength(255).IsRequired();
        builder.Property(n => n.Body).HasColumnType("text").IsRequired();
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(n => n.Priority).HasConversion<string>().HasMaxLength(50);
        builder.Property(n => n.RecipientType).HasMaxLength(50);
        builder.Property(n => n.ExtraData).HasColumnType("jsonb");

        builder.HasIndex(n => n.SentAt).IsDescending();
        builder.HasIndex(n => n.Type);
    }
}
