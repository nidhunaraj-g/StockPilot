
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configuration
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.ToTable("Purchases");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PurchaseNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.PurchaseNumber)
                .IsUnique();

            builder.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Draft");

            builder.Property(p => p.Subtotal)
                .HasPrecision(18, 2);

            builder.Property(p => p.TaxAmount)
                .HasPrecision(18, 2);

            builder.Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(p => p.Notes)
                .HasMaxLength(500);

            builder.Property(p => p.PurchaseDateUtc)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAtUtc)
                .IsRequired();

            builder.Property(p => p.UpdatedAtUtc)
                .IsRequired(false);

            builder.HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Items)
                .WithOne(i => i.Purchase)
                .HasForeignKey(i => i.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
