using Microsoft.EntityFrameworkCore;
using Restaurant.Core.Entities;

namespace Restaurant.Customer.Api;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
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
        base.OnModelCreating(modelBuilder);

        // RestaurantProfile
        modelBuilder.Entity<RestaurantProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VendorId).IsUnique();

            entity.HasMany(e => e.WeeklySchedule)
                  .WithOne(e => e.RestaurantProfile)
                  .HasForeignKey(e => e.RestaurantProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.DeliveryZones)
                  .WithOne(e => e.RestaurantProfile)
                  .HasForeignKey(e => e.RestaurantProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Employees)
                  .WithOne(e => e.RestaurantProfile)
                  .HasForeignKey(e => e.RestaurantProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // VendorSchedule
        modelBuilder.Entity<VendorSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RestaurantProfileId, e.DayOfWeek }).IsUnique();
        });

        // DeliveryZone
        modelBuilder.Entity<DeliveryZone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.MinRadiusKm).HasPrecision(18, 2);
            entity.Property(e => e.MaxRadiusKm).HasPrecision(18, 2);
        });

        // VendorEmployee
        modelBuilder.Entity<VendorEmployee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.RestaurantProfileId, e.Role });
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
        });

        // RestaurantOrder
        modelBuilder.Entity<RestaurantOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RestaurantProfileId);

            entity.Property(e => e.DistanceKm).HasPrecision(18, 2);
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasOne(e => e.RestaurantProfile)
                  .WithMany()
                  .HasForeignKey(e => e.RestaurantProfileId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeliveryZone)
                  .WithMany()
                  .HasForeignKey(e => e.DeliveryZoneId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedDriver)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedDriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.StatusHistory)
                  .WithOne(e => e.RestaurantOrder)
                  .HasForeignKey(e => e.RestaurantOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Items)
                  .WithOne(e => e.RestaurantOrder)
                  .HasForeignKey(e => e.RestaurantOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // RestaurantOrderItem
        modelBuilder.Entity<RestaurantOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.ExtrasPrice).HasPrecision(18, 2);
        });

        // OrderStatusHistory
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RestaurantOrderId);
            entity.HasIndex(e => e.ChangedAt);
        });

        // OrderDriverAssignment
        modelBuilder.Entity<OrderDriverAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RestaurantOrderId);
            entity.HasIndex(e => e.DriverEmployeeId);

            entity.HasOne(e => e.RestaurantOrder)
                  .WithMany()
                  .HasForeignKey(e => e.RestaurantOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Driver)
                  .WithMany()
                  .HasForeignKey(e => e.DriverEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Demo Data
        SeedDemoData(modelBuilder);
    }

    private static void SeedDemoData(ModelBuilder modelBuilder)
    {
        var restaurantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var vendorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Demo Restaurant
        modelBuilder.Entity<RestaurantProfile>().HasData(new RestaurantProfile
        {
            Id = restaurantId,
            VendorId = vendorId,
            CuisineType = "وجبات سريعة",
            Description = "أفضل البرجر في المدينة",
            AveragePreparationTime = 20,
            MinimumOrderAmount = 25,
            SupportsDelivery = true,
            SupportsPickup = false,
            CurrentRadarStatus = Core.Enums.RadarStatus.Open,
            Latitude = 24.7136,
            Longitude = 46.6753,
            FullAddress = "الرياض، حي النخيل",
            City = "الرياض",
            CreatedAt = DateTime.UtcNow
        });

        // Weekly Schedule
        var days = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday,
                          DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        for (int i = 0; i < days.Length; i++)
        {
            modelBuilder.Entity<VendorSchedule>().HasData(new VendorSchedule
            {
                Id = Guid.NewGuid(),
                RestaurantProfileId = restaurantId,
                DayOfWeek = days[i],
                IsOpen = days[i] != DayOfWeek.Friday, // مغلق يوم الجمعة
                OpenTime = new TimeSpan(10, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0)
            });
        }

        // Delivery Zones
        modelBuilder.Entity<DeliveryZone>().HasData(
            new DeliveryZone
            {
                Id = Guid.NewGuid(),
                RestaurantProfileId = restaurantId,
                Name = "النطاق الأول",
                MinRadiusKm = 0,
                MaxRadiusKm = 3,
                DeliveryFee = 0,
                EstimatedMinutesMin = 15,
                EstimatedMinutesMax = 25,
                IsActive = true,
                SortOrder = 1
            },
            new DeliveryZone
            {
                Id = Guid.NewGuid(),
                RestaurantProfileId = restaurantId,
                Name = "النطاق الثاني",
                MinRadiusKm = 3,
                MaxRadiusKm = 6,
                DeliveryFee = 5,
                EstimatedMinutesMin = 25,
                EstimatedMinutesMax = 35,
                IsActive = true,
                SortOrder = 2
            },
            new DeliveryZone
            {
                Id = Guid.NewGuid(),
                RestaurantProfileId = restaurantId,
                Name = "النطاق الثالث",
                MinRadiusKm = 6,
                MaxRadiusKm = 10,
                DeliveryFee = 10,
                EstimatedMinutesMin = 35,
                EstimatedMinutesMax = 50,
                IsActive = true,
                SortOrder = 3
            }
        );
    }
}
