
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configuration
{
    public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseItem> builder)
        {
            builder.ToTable("PurchaseItems");

            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.Quantity)
                .IsRequired();

            builder.Property(pi => pi.ReceivedQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.UnitCost)
                .HasPrecision(18, 2);

            builder.Property(pi => pi.Subtotal)
                .HasPrecision(18, 2);

            builder.HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
