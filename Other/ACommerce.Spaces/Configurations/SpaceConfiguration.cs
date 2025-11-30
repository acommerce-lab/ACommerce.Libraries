using ACommerce.Spaces.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Spaces.Configurations;

public class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.ToTable("Spaces");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.ShortDescription)
            .HasMaxLength(500);

        builder.Property(x => x.LongDescription)
            .HasMaxLength(5000);

        builder.Property(x => x.RoomNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.District)
            .HasMaxLength(100);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.FeaturedImage)
            .HasMaxLength(500);

        builder.Property(x => x.VirtualTourUrl)
            .HasMaxLength(500);

        builder.Property(x => x.CancellationPolicy)
            .HasMaxLength(2000);

        builder.Property(x => x.HouseRules)
            .HasMaxLength(2000);

        builder.Property(x => x.AccessInstructions)
            .HasMaxLength(1000);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(50);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(200);

        builder.Property(x => x.AverageRating)
            .HasPrecision(3, 2);

        builder.Property(x => x.AreaSquareMeters)
            .HasPrecision(10, 2);

        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.IsFeatured);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SpaceBookingConfiguration : IEntityTypeConfiguration<SpaceBooking>
{
    public void Configure(EntityTypeBuilder<SpaceBooking> builder)
    {
        builder.ToTable("SpaceBookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.BookingNumber)
            .IsUnique();

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200);

        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(200);

        builder.Property(x => x.BasePrice)
            .HasPrecision(18, 4);

        builder.Property(x => x.ServiceFee)
            .HasPrecision(18, 4);

        builder.Property(x => x.Tax)
            .HasPrecision(18, 4);

        builder.Property(x => x.Discount)
            .HasPrecision(18, 4);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.RefundAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(x => x.DiscountCode)
            .HasMaxLength(50);

        builder.Property(x => x.PaymentTransactionId)
            .HasMaxLength(200);

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(x => x.Purpose)
            .HasMaxLength(500);

        builder.Property(x => x.CustomerNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.OwnerNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.InternalNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.SpecialRequests)
            .HasMaxLength(1000);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        builder.Property(x => x.CancelledBy)
            .HasMaxLength(100);

        builder.Property(x => x.QrCode)
            .HasMaxLength(500);

        builder.Property(x => x.BookingSource)
            .HasMaxLength(100);

        builder.Property(x => x.DeviceId)
            .HasMaxLength(200);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.HasOne(x => x.Space)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SpaceId);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.StartDateTime);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SpaceReviewConfiguration : IEntityTypeConfiguration<SpaceReview>
{
    public void Configure(EntityTypeBuilder<SpaceReview> builder)
    {
        builder.ToTable("SpaceReviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReviewerName)
            .HasMaxLength(200);

        builder.Property(x => x.ReviewerAvatar)
            .HasMaxLength(500);

        builder.Property(x => x.Title)
            .HasMaxLength(200);

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.Property(x => x.Pros)
            .HasMaxLength(500);

        builder.Property(x => x.Cons)
            .HasMaxLength(500);

        builder.Property(x => x.OwnerResponse)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Space)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.SpaceId);
        builder.HasIndex(x => x.ReviewerId);
        builder.HasIndex(x => x.Rating);
        builder.HasIndex(x => x.IsPublished);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SpaceAmenityConfiguration : IEntityTypeConfiguration<SpaceAmenity>
{
    public void Configure(EntityTypeBuilder<SpaceAmenity> builder)
    {
        builder.ToTable("SpaceAmenities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.Category)
            .HasMaxLength(50);

        builder.Property(x => x.Icon)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.PricingUnit)
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.ExtraPrice)
            .HasPrecision(18, 4);

        builder.HasOne(x => x.Space)
            .WithMany(x => x.Amenities)
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SpaceId);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SpacePriceConfiguration : IEntityTypeConfiguration<SpacePrice>
{
    public void Configure(EntityTypeBuilder<SpacePrice> builder)
    {
        builder.ToTable("SpacePrices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasPrecision(18, 4);

        builder.Property(x => x.OriginalPrice)
            .HasPrecision(18, 4);

        builder.Property(x => x.WeekendPrice)
            .HasPrecision(18, 4);

        builder.Property(x => x.HolidayPrice)
            .HasPrecision(18, 4);

        builder.Property(x => x.DiscountPercentage)
            .HasPrecision(5, 2);

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Space)
            .WithMany(x => x.Prices)
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SpaceId);
        builder.HasIndex(x => x.PricingType);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SpaceAvailabilityConfiguration : IEntityTypeConfiguration<SpaceAvailability>
{
    public void Configure(EntityTypeBuilder<SpaceAvailability> builder)
    {
        builder.ToTable("SpaceAvailabilities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.SpecialPrice)
            .HasPrecision(18, 4);

        builder.HasOne(x => x.Space)
            .WithMany(x => x.Availabilities)
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SpaceId);
        builder.HasIndex(x => x.DayOfWeek);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SpaceFavoriteConfiguration : IEntityTypeConfiguration<SpaceFavorite>
{
    public void Configure(EntityTypeBuilder<SpaceFavorite> builder)
    {
        builder.ToTable("SpaceFavorites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Space)
            .WithMany()
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.SpaceId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
