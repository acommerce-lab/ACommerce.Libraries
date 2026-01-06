using Microsoft.EntityFrameworkCore;
using Restaurant.Core.Entities;

namespace Restaurant.Vendor.Api;

public class VendorDbContext : DbContext
{
    public VendorDbContext(DbContextOptions<VendorDbContext> options)
        : base(options)
    {
    }

    public DbSet<RestaurantProfile> RestaurantProfiles => Set<RestaurantProfile>();
    public DbSet<VendorSchedule> VendorSchedules => Set<VendorSchedule>();
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<VendorEmployee> VendorEmployees => Set<VendorEmployee>();
    public DbSet<RestaurantOrder> RestaurantOrders => Set<RestaurantOrder>();
    public DbSet<RestaurantOrderItem> RestaurantOrderItems => Set<RestaurantOrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OrderDriverAssignment> OrderDriverAssignments => Set<OrderDriverAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Same configuration as Customer API
        // In production, this would be shared or use a common configuration

        modelBuilder.Entity<RestaurantProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VendorId).IsUnique();
        });

        modelBuilder.Entity<VendorSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<DeliveryZone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.MinRadiusKm).HasPrecision(18, 2);
            entity.Property(e => e.MaxRadiusKm).HasPrecision(18, 2);
        });

        modelBuilder.Entity<VendorEmployee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
        });

        modelBuilder.Entity<RestaurantOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Property(e => e.DistanceKm).HasPrecision(18, 2);
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<RestaurantOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.ExtrasPrice).HasPrecision(18, 2);
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
