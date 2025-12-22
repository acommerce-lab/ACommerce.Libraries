using ACommerce.Bookings.Enums;

namespace ACommerce.Bookings.DTOs;

/// <summary>
/// بيانات إنشاء حجز جديد
/// </summary>
public class CreateBookingDto
{
    /// <summary>
    /// معرف العقار
    /// </summary>
    public Guid SpaceId { get; set; }

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

    /// <summary>
    /// السعر الكلي
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// نسبة العربون (اختياري - يستخدم الافتراضي 10%)
    /// </summary>
    public decimal? DepositPercentage { get; set; }

    /// <summary>
    /// ملاحظات المستأجر
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// عدد الضيوف
    /// </summary>
    public int GuestsCount { get; set; } = 1;

    /// <summary>
    /// معرف عملية الدفع (إذا تم الدفع مسبقاً)
    /// </summary>
    public string? PaymentId { get; set; }

    /// <summary>
    /// رابط العودة بعد الدفع
    /// </summary>
    public string? ReturnUrl { get; set; }
}
