using ACommerce.Subscriptions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Subscriptions.Configurations;

public class SubscriptionEventConfiguration : IEntityTypeConfiguration<SubscriptionEvent>
{
    public void Configure(EntityTypeBuilder<SubscriptionEvent> builder)
    {
        builder.ToTable("SubscriptionEvents");

        builder.HasKey(e => e.Id);

        // Event Info
        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // State Change
        builder.Property(e => e.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Financial
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .HasMaxLength(10);

        builder.Property(e => e.PaymentId)
            .HasMaxLength(100);

        // Source
        builder.Property(e => e.PerformedBy)
            .HasMaxLength(100);

        builder.Property(e => e.PerformedByType)
            .HasMaxLength(20);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        // Metadata
        builder.Property(e => e.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.SubscriptionId);
        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.EventDate);
        builder.HasIndex(e => e.IsDeleted);

        // Composite indexes
        builder.HasIndex(e => new { e.SubscriptionId, e.EventDate });
        builder.HasIndex(e => new { e.VendorId, e.EventType });
    }
}
