using ACommerce.Bookings.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Bookings.Entities;

/// <summary>
/// كيان الحجز - حجز عقار للإيجار
/// </summary>
public class Booking : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // العلاقات
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// معرف العقار (Listing/Space)
    /// </summary>
    public Guid SpaceId { get; set; }

    /// <summary>
    /// معرف المستأجر (Customer)
    /// </summary>
    public required string CustomerId { get; set; }

    /// <summary>
    /// معرف المالك (Vendor/Host)
    /// </summary>
    public Guid HostId { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // معلومات العقار (نسخة للتاريخ)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// اسم العقار وقت الحجز
    /// </summary>
    public string? SpaceName { get; set; }

    /// <summary>
    /// صورة العقار وقت الحجز
    /// </summary>
    public string? SpaceImage { get; set; }

    /// <summary>
    /// موقع العقار
    /// </summary>
    public string? SpaceLocation { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // التواريخ والفترة
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// تاريخ بدء الإيجار
    /// </summary>
    public DateTime CheckInDate { get; set; }

    /// <summary>
    /// تاريخ انتهاء الإيجار
    /// </summary>
    public DateTime CheckOutDate { get; set; }

    /// <summary>
    /// نوع الإيجار
    /// </summary>
    public RentType RentType { get; set; } = RentType.Monthly;

    // ═══════════════════════════════════════════════════════════════════
    // التسعير والدفع
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// السعر الكلي للإيجار
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// نسبة العربون المئوية
    /// </summary>
    public decimal DepositPercentage { get; set; } = 10;

    /// <summary>
    /// مبلغ العربون
    /// </summary>
    public decimal DepositAmount { get; set; }

    /// <summary>
    /// المبلغ المتبقي
    /// </summary>
    public decimal RemainingAmount => TotalPrice - DepositAmount;

    /// <summary>
    /// العملة
    /// </summary>
    public string Currency { get; set; } = "SAR";

    // ═══════════════════════════════════════════════════════════════════
    // معلومات الدفع
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// معرف عملية دفع العربون
    /// </summary>
    public string? DepositPaymentId { get; set; }

    /// <summary>
    /// تاريخ دفع العربون
    /// </summary>
    public DateTime? DepositPaidAt { get; set; }

    /// <summary>
    /// معرف عملية الدفع النهائي (إن وجد)
    /// </summary>
    public string? FinalPaymentId { get; set; }

    /// <summary>
    /// تاريخ الدفع النهائي
    /// </summary>
    public DateTime? FinalPaymentAt { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // الضمان (Escrow)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// حالة الضمان
    /// </summary>
    public EscrowStatus EscrowStatus { get; set; } = EscrowStatus.None;

    /// <summary>
    /// تاريخ تحرير الضمان
    /// </summary>
    public DateTime? EscrowReleasedAt { get; set; }

    /// <summary>
    /// المبلغ المحرر من الضمان
    /// </summary>
    public decimal? EscrowReleasedAmount { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // الحالة
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// حالة الحجز
    /// </summary>
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

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
    /// من قام بالإلغاء (customer, host, system)
    /// </summary>
    public string? CancelledBy { get; set; }

    /// <summary>
    /// تاريخ الرفض
    /// </summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>
    /// سبب الرفض
    /// </summary>
    public string? RejectionReason { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // ملاحظات ومعلومات إضافية
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// ملاحظات المستأجر
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// ملاحظات المالك
    /// </summary>
    public string? HostNotes { get; set; }

    /// <summary>
    /// ملاحظات داخلية (للإدارة)
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// عدد الضيوف
    /// </summary>
    public int GuestsCount { get; set; } = 1;

    /// <summary>
    /// معرف التقييم (بعد الاكتمال)
    /// </summary>
    public Guid? ReviewId { get; set; }
}
