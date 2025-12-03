using ACommerce.Subscriptions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Subscriptions.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasKey(e => e.Id);

        // Basic Info
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.NameEn)
            .HasMaxLength(100);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.DescriptionEn)
            .HasMaxLength(500);

        builder.Property(e => e.Icon)
            .HasMaxLength(50);

        builder.Property(e => e.Color)
            .HasMaxLength(20);

        // Pricing
        builder.Property(e => e.MonthlyPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.QuarterlyPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.SemiAnnualPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.AnnualPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("SAR");

        // Commission
        builder.Property(e => e.CommissionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.CommissionPercentage)
            .HasPrecision(5, 2);

        builder.Property(e => e.CommissionFixedAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.MinCommission)
            .HasPrecision(18, 2);

        builder.Property(e => e.MaxCommission)
            .HasPrecision(18, 2);

        // Features
        builder.Property(e => e.AnalyticsLevel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.SupportLevel)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Metadata
        builder.Property(e => e.ExtraFeaturesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.MetadataJson)
            .HasColumnType("nvarchar(max)");

        // Ignore NotMapped
        builder.Ignore(e => e.ExtraFeatures);
        builder.Ignore(e => e.Metadata);

        // Indexes
        builder.HasIndex(e => e.Slug)
            .IsUnique();

        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.IsDefault);
        builder.HasIndex(e => e.SortOrder);
        builder.HasIndex(e => e.IsDeleted);

        // Relationships
        builder.HasMany(e => e.Subscriptions)
            .WithOne(s => s.Plan)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
