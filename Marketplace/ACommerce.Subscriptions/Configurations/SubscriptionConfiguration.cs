using ACommerce.Subscriptions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Subscriptions.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(e => e.Id);

        // Status
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.BillingCycle)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(500);

        // Pricing Snapshot
        builder.Property(e => e.Price)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("SAR");

        builder.Property(e => e.CommissionPercentage)
            .HasPrecision(5, 2);

        builder.Property(e => e.CommissionFixedAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.CommissionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Usage
        builder.Property(e => e.CurrentStorageUsedMB)
            .HasPrecision(18, 2);

        // Settings
        builder.Property(e => e.PaymentMethodId)
            .HasMaxLength(100);

        builder.Property(e => e.BillingEmail)
            .HasMaxLength(255);

        // Promotion
        builder.Property(e => e.CouponCode)
            .HasMaxLength(50);

        builder.Property(e => e.DiscountPercentage)
            .HasPrecision(5, 2);

        // Metadata
        builder.Property(e => e.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.InternalNotes)
            .HasMaxLength(1000);

        // Ignore NotMapped
        builder.Ignore(e => e.Metadata);
        builder.Ignore(e => e.IsInTrial);
        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.DaysRemaining);
        builder.Ignore(e => e.ListingsUsagePercentage);
        builder.Ignore(e => e.HasReachedListingsLimit);
        builder.Ignore(e => e.HasReachedStorageLimit);

        // Indexes
        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.PlanId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CurrentPeriodEnd);
        builder.HasIndex(e => e.NextPaymentDate);
        builder.HasIndex(e => e.IsDeleted);

        // Composite indexes
        builder.HasIndex(e => new { e.VendorId, e.Status });
        builder.HasIndex(e => new { e.Status, e.CurrentPeriodEnd });

        // Relationships
        builder.HasOne(e => e.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(e => e.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Invoices)
            .WithOne(i => i.Subscription)
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Events)
            .WithOne(e => e.Subscription)
            .HasForeignKey(e => e.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
