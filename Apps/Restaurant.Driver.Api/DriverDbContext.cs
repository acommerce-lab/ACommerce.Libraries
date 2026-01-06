using Microsoft.EntityFrameworkCore;
using Restaurant.Core.Entities;

namespace Restaurant.Driver.Api;

public class DriverDbContext : DbContext
{
    public DriverDbContext(DbContextOptions<DriverDbContext> options)
        : base(options)
    {
    }

    public DbSet<VendorEmployee> VendorEmployees => Set<VendorEmployee>();
    public DbSet<RestaurantOrder> RestaurantOrders => Set<RestaurantOrder>();
    public DbSet<RestaurantOrderItem> RestaurantOrderItems => Set<RestaurantOrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OrderDriverAssignment> OrderDriverAssignments => Set<OrderDriverAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VendorEmployee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
        });

        modelBuilder.Entity<RestaurantOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Property(e => e.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<RestaurantOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<OrderDriverAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
