using ACommerce.Subscriptions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Subscriptions.Configurations;

public class SubscriptionInvoiceConfiguration : IEntityTypeConfiguration<SubscriptionInvoice>
{
    public void Configure(EntityTypeBuilder<SubscriptionInvoice> builder)
    {
        builder.ToTable("SubscriptionInvoices");

        builder.HasKey(e => e.Id);

        // Invoice Info
        builder.Property(e => e.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.BillingCycle)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Amounts
        builder.Property(e => e.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountCode)
            .HasMaxLength(50);

        builder.Property(e => e.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(e => e.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Total)
            .HasPrecision(18, 2);

        builder.Property(e => e.AmountPaid)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("SAR");

        // Payment
        builder.Property(e => e.PaymentId)
            .HasMaxLength(100);

        builder.Property(e => e.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(e => e.LastPaymentError)
            .HasMaxLength(500);

        // Billing Details
        builder.Property(e => e.CustomerName)
            .HasMaxLength(200);

        builder.Property(e => e.CustomerEmail)
            .HasMaxLength(255);

        builder.Property(e => e.BillingAddress)
            .HasMaxLength(500);

        builder.Property(e => e.TaxNumber)
            .HasMaxLength(50);

        // Line Items
        builder.Property(e => e.LineItemDescription)
            .HasMaxLength(500);

        builder.Property(e => e.PlanName)
            .HasMaxLength(100);

        // Refund
        builder.Property(e => e.RefundedAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.RefundReason)
            .HasMaxLength(500);

        // Metadata
        builder.Property(e => e.PdfUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Metadata - use database-agnostic text type
        builder.Property(e => e.MetadataJson);

        // Ignore NotMapped
        builder.Ignore(e => e.AmountDue);
        builder.Ignore(e => e.IsPaid);
        builder.Ignore(e => e.IsOverdue);

        // Indexes
        builder.HasIndex(e => e.InvoiceNumber)
            .IsUnique();

        builder.HasIndex(e => e.SubscriptionId);
        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.DueDate);
        builder.HasIndex(e => e.PaidAt);
        builder.HasIndex(e => e.IsDeleted);

        // Composite indexes
        builder.HasIndex(e => new { e.VendorId, e.Status });
        builder.HasIndex(e => new { e.Status, e.DueDate });
    }
}
