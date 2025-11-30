using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.Entities;

/// <summary>
/// حجز المساحة
/// </summary>
public class SpaceBooking : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// رقم الحجز
    /// </summary>
    public required string BookingNumber { get; set; }

    /// <summary>
    /// معرف المساحة
    /// </summary>
    public Guid SpaceId { get; set; }
    public Space? Space { get; set; }

    /// <summary>
    /// معرف العميل
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// اسم العميل
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// هاتف العميل
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// بريد العميل
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// تاريخ ووقت البداية
    /// </summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// تاريخ ووقت النهاية
    /// </summary>
    public DateTime EndDateTime { get; set; }

    /// <summary>
    /// عدد الضيوف
    /// </summary>
    public int GuestsCount { get; set; } = 1;

    /// <summary>
    /// حالة الحجز
    /// </summary>
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    /// <summary>
    /// نوع التسعير المستخدم
    /// </summary>
    public PricingType PricingType { get; set; }

    /// <summary>
    /// السعر الأساسي
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// رسوم الخدمة
    /// </summary>
    public decimal ServiceFee { get; set; }

    /// <summary>
    /// الضريبة
    /// </summary>
    public decimal Tax { get; set; }

    /// <summary>
    /// الخصم
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// كود الخصم
    /// </summary>
    public string? DiscountCode { get; set; }

    /// <summary>
    /// المبلغ الإجمالي
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// كود العملة
    /// </summary>
    public string CurrencyCode { get; set; } = "SAR";

    /// <summary>
    /// حالة الدفع
    /// </summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// تاريخ الدفع
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// معرف عملية الدفع
    /// </summary>
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// سبب الحجز/الغرض
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// ملاحظات العميل
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// ملاحظات المالك
    /// </summary>
    public string? OwnerNotes { get; set; }

    /// <summary>
    /// ملاحظات داخلية
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// طلبات خاصة
    /// </summary>
    public string? SpecialRequests { get; set; }

    /// <summary>
    /// تاريخ التأكيد
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// تاريخ الإلغاء
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// سبب الإلغاء
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// من قام بالإلغاء
    /// </summary>
    public string? CancelledBy { get; set; }

    /// <summary>
    /// مبلغ الاسترداد
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// تاريخ الاسترداد
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// تاريخ تسجيل الدخول الفعلي
    /// </summary>
    public DateTime? CheckedInAt { get; set; }

    /// <summary>
    /// تاريخ تسجيل الخروج الفعلي
    /// </summary>
    public DateTime? CheckedOutAt { get; set; }

    /// <summary>
    /// تم التقييم
    /// </summary>
    public bool IsReviewed { get; set; }

    /// <summary>
    /// كود QR للدخول
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// تذكير تم إرساله
    /// </summary>
    public bool ReminderSent { get; set; }

    /// <summary>
    /// المنصة المستخدمة للحجز
    /// </summary>
    public string? BookingSource { get; set; }

    /// <summary>
    /// معرف الجهاز
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// عنوان IP
    /// </summary>
    public string? IpAddress { get; set; }
}
