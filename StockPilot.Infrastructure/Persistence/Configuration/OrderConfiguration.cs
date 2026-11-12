using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders", table =>
            {
                table.HasCheckConstraint(
                    "CK_Orders_Subtotal",
                    "[Subtotal] >= 0");

                table.HasCheckConstraint(
                    "CK_Orders_TaxAmount",
                    "[TaxAmount] >= 0");

                table.HasCheckConstraint(
                    "CK_Orders_TotalAmount",
                    "[TotalAmount] >= 0");
            });

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Property(o => o.CustomerName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(o => o.CustomerEmail)
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(o => o.CustomerPhone)
                .HasMaxLength(20);

            builder.Property(o => o.DeliveryAddress)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(o => o.DeliveryCity)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(o => o.DeliveryState)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(o => o.DeliveryCountry)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(o => o.DeliveryPostalCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(o => o.OrderDateUtc)
                .IsRequired();

            builder.Property(o => o.Subtotal)
                .HasPrecision(18, 2);

            builder.Property(o => o.TaxAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.OrderDateUtc);
            builder.HasIndex(o => o.Status);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
