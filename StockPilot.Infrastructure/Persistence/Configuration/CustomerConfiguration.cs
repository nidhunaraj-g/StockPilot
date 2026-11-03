using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;
using StockPilot.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Persistence.Configuration
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.ApplicationUserId)
                .IsRequired();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Customer>(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(c => c.Email)
                .IsUnique();

            builder.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Address)
                .HasMaxLength(250);

            builder.Property(c => c.City)
                .HasMaxLength(100);

            builder.Property(c => c.State)
                .HasMaxLength(100);

            builder.Property(c => c.Country)
                .HasMaxLength(100);

            builder.Property(c => c.PostalCode)
                .HasMaxLength(20);

            // Account deactivation configuration
            builder.Property(c => c.DeactivatedAtUtc)
                .IsRequired(false);

            builder.Property(c => c.IsAdminDeactivated)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAtUtc)
                .IsRequired();

            builder.Property(c => c.UpdatedAtUtc)
                .IsRequired(false);
        }
    }
}