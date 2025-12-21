using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Infrastructure.Persistence.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductId).HasMaxLength(255).IsRequired();
        builder.Property(p => p.TransactionId).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Platform).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ReceiptData).HasColumnType("text");
        builder.Property(p => p.VerificationError).HasColumnType("text");
        builder.Property(p => p.ContentUnlocked).HasColumnType("jsonb");

        builder.HasIndex(p => p.TransactionId).IsUnique();
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.ProductId);
        builder.HasIndex(p => p.IsVerified);
        builder.HasIndex(p => p.CreatedAt).IsDescending();
    }
}
