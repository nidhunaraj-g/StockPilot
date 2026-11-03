
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StockPilot.Domain.Entities;
using StockPilot.Infrastructure.Identity;

namespace StockPilot.Infrastructure.Persistence
{
    public class StockPilotDbContext(
        DbContextOptions<StockPilotDbContext> options)
        : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(StockPilotDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.UpdatedAtUtc = null;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = now;

                    entry.Property(entity => entity.CreatedAtUtc)
                        .IsModified = false;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}