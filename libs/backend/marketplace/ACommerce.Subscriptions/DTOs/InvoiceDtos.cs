using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.DTOs;

/// <summary>
/// DTO لعرض الفاتورة
/// </summary>
public record SubscriptionInvoiceDto
{
    public Guid Id { get; init; }
    public Guid SubscriptionId { get; init; }
    public Guid VendorId { get; init; }

    // Invoice Info
    public required string InvoiceNumber { get; init; }
    public InvoiceStatus Status { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime CreatedAt { get; init; }

    // Amounts
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public string? DiscountCode { get; init; }
    public decimal TaxRate { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal AmountDue { get; init; }
    public string Currency { get; init; } = "SAR";

    // Payment
    public string? PaymentMethod { get; init; }
    public DateTime? PaidAt { get; init; }

    // Details
    public string? PlanName { get; init; }
    public string? LineItemDescription { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerEmail { get; init; }

    // Computed
    public bool IsPaid { get; init; }
    public bool IsOverdue { get; init; }

    // Download
    public string? PdfUrl { get; init; }
}

/// <summary>
/// DTO لعرض ملخص الفاتورة
/// </summary>
public record InvoiceSummaryDto
{
    public Guid Id { get; init; }
    public required string InvoiceNumber { get; init; }
    public InvoiceStatus Status { get; init; }
    public DateTime DueDate { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "SAR";
    public bool IsPaid { get; init; }
    public bool IsOverdue { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO لقائمة الفواتير مع تصفية
/// </summary>
public record InvoiceListQueryDto
{
    public Guid? VendorId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}

/// <summary>
/// DTO لإنشاء فاتورة
/// </summary>
public record CreateInvoiceDto
{
    public Guid SubscriptionId { get; init; }
    public Guid VendorId { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal Subtotal { get; init; }
    public string? DiscountCode { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxRate { get; init; }
    public string Currency { get; init; } = "SAR";
    public string? CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public string? BillingAddress { get; init; }
    public string? TaxNumber { get; init; }
    public string? LineItemDescription { get; init; }
    public string? PlanName { get; init; }
    public int DueDays { get; init; } = 7;
}

/// <summary>
/// DTO لتسجيل دفعة
/// </summary>
public record RecordPaymentDto
{
    public Guid InvoiceId { get; init; }
    public string? PaymentId { get; init; }
    public string? PaymentMethod { get; init; }
    public decimal Amount { get; init; }
    public DateTime? PaidAt { get; init; }
}

/// <summary>
/// DTO لطلب استرداد
/// </summary>
public record RefundRequestDto
{
    public Guid InvoiceId { get; init; }
    public decimal? Amount { get; init; }
    public required string Reason { get; init; }
}

/// <summary>
/// DTO لتقرير الفواتير
/// </summary>
public record InvoiceReportDto
{
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public int TotalInvoices { get; init; }
    public int PaidInvoices { get; init; }
    public int PendingInvoices { get; init; }
    public int OverdueInvoices { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalPending { get; init; }
    public decimal TotalOverdue { get; init; }
    public decimal AverageInvoiceAmount { get; init; }
    public string Currency { get; init; } = "SAR";
    public Dictionary<string, decimal> RevenueByMonth { get; init; } = new();
    public Dictionary<string, int> InvoicesByStatus { get; init; } = new();
}
