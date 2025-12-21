using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJob>
{
    public void Configure(EntityTypeBuilder<ScheduledJob> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.JobId).HasMaxLength(100).IsRequired();
        builder.Property(j => j.JobName).HasMaxLength(255).IsRequired();
        builder.Property(j => j.TriggerType).HasMaxLength(50);
        builder.Property(j => j.TriggerConfig).HasColumnType("jsonb");
        builder.Property(j => j.JobType).HasMaxLength(100);
        builder.Property(j => j.Payload).HasColumnType("jsonb");
        builder.Property(j => j.Status).HasMaxLength(50);

        builder.HasIndex(j => j.JobId).IsUnique();
        builder.HasIndex(j => j.NextRunTime);
        builder.HasIndex(j => j.Status);
    }
}
