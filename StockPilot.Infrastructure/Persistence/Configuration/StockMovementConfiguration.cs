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
    public class StockMovementConfiguration
    : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.MovementType)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.Reason)
                .HasMaxLength(500);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ProductId, x.CreatedAtUtc });
        }
    }
}
