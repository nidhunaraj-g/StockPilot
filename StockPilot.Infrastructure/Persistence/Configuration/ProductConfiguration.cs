using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Persistence.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.SKU)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(p => p.SKU)
                .IsUnique();

            builder.Property(p => p.Barcode)
                .HasMaxLength(100);

            builder.HasIndex(p => p.Barcode)
                .IsUnique()
                .HasFilter("[Barcode] IS NOT NULL");

            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.CostPrice)
                .HasPrecision(18, 2);

            builder.Property(p => p.SellingPrice)
                .HasPrecision(18, 2);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Products_CostPrice",
                "[CostPrice] >= 0"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Products_SellingPrice",
                "[SellingPrice] >= 0"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Products_MinimumStockLevel",
                "[MinimumStockLevel] >= 0"));

            builder.Property(p => p.RowVersion)
                .IsRowVersion();

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}