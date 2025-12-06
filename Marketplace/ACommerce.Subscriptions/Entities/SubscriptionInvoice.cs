using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.Entities;

/// <summary>
/// فاتورة الاشتراك
/// </summary>
public class SubscriptionInvoice : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    #region Relations - العلاقات

    /// <summary>معرف الاشتراك</summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>الاشتراك</summary>
    public Subscription? Subscription { get; set; }

    /// <summary>معرف المزود</summary>
    public Guid VendorId { get; set; }

    #endregion

    #region Invoice Info - معلومات الفاتورة

    /// <summary>رقم الفاتورة</summary>
    public required string InvoiceNumber { get; set; }

    /// <summary>حالة الفاتورة</summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    /// <summary>دورة الفوترة</summary>
    public BillingCycle BillingCycle { get; set; }

    /// <summary>بداية فترة الفوترة</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>نهاية فترة الفوترة</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>تاريخ الاستحقاق</summary>
    public DateTime DueDate { get; set; }

    #endregion

    #region Amounts - المبالغ

    /// <summary>المبلغ الأساسي (قبل الخصم والضريبة)</summary>
    public decimal Subtotal { get; set; }

    /// <summary>مبلغ الخصم</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>كود الخصم المستخدم</summary>
    public string? DiscountCode { get; set; }

    /// <summary>نسبة الضريبة</summary>
    public decimal TaxRate { get; set; }

    /// <summary>مبلغ الضريبة</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>المبلغ الإجمالي</summary>
    public decimal Total { get; set; }

    /// <summary>المبلغ المدفوع</summary>
    public decimal AmountPaid { get; set; }

    /// <summary>المبلغ المتبقي</summary>
    [NotMapped]
    public decimal AmountDue => Total - AmountPaid;

    /// <summary>العملة</summary>
    public string Currency { get; set; } = "SAR";

    #endregion

    #region Payment - الدفع

    /// <summary>معرف عملية الدفع</summary>
    public string? PaymentId { get; set; }

    /// <summary>طريقة الدفع</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>تاريخ الدفع</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>عدد محاولات الدفع الفاشلة</summary>
    public int FailedPaymentAttempts { get; set; }

    /// <summary>رسالة آخر خطأ في الدفع</summary>
    public string? LastPaymentError { get; set; }

    #endregion

    #region Billing Details - تفاصيل الفوترة

    /// <summary>اسم العميل</summary>
    public string? CustomerName { get; set; }

    /// <summary>البريد الإلكتروني</summary>
    public string? CustomerEmail { get; set; }

    /// <summary>عنوان الفوترة</summary>
    public string? BillingAddress { get; set; }

    /// <summary>الرقم الضريبي</summary>
    public string? TaxNumber { get; set; }

    #endregion

    #region Line Items - بنود الفاتورة

    /// <summary>وصف البند</summary>
    public string? LineItemDescription { get; set; }

    /// <summary>اسم الباقة</summary>
    public string? PlanName { get; set; }

    #endregion

    #region Refund - الاسترداد

    /// <summary>هل تم استرداد المبلغ</summary>
    public bool IsRefunded { get; set; }

    /// <summary>المبلغ المسترد</summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>تاريخ الاسترداد</summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>سبب الاسترداد</summary>
    public string? RefundReason { get; set; }

    #endregion

    #region Metadata - بيانات إضافية

    /// <summary>رابط تحميل PDF</summary>
    public string? PdfUrl { get; set; }

    /// <summary>ملاحظات</summary>
    public string? Notes { get; set; }

    /// <summary>بيانات إضافية JSON</summary>
    public string? MetadataJson { get; set; }

    #endregion

    #region Helper Methods - دوال مساعدة

    /// <summary>إنشاء رقم فاتورة جديد</summary>
    public static string GenerateInvoiceNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"INV-{timestamp}-{random}";
    }

    /// <summary>هل الفاتورة مدفوعة بالكامل</summary>
    [NotMapped]
    public bool IsPaid => Status == InvoiceStatus.Paid && AmountDue <= 0;

    /// <summary>هل الفاتورة متأخرة</summary>
    [NotMapped]
    public bool IsOverdue => Status == InvoiceStatus.Pending && DateTime.UtcNow > DueDate;

    #endregion
}
