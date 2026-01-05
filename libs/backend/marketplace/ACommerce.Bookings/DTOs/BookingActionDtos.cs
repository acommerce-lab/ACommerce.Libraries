namespace ACommerce.Bookings.DTOs;

/// <summary>
/// تأكيد الحجز من المالك
/// </summary>
public class ConfirmBookingDto
{
    /// <summary>
    /// ملاحظات المالك
    /// </summary>
    public string? HostNotes { get; set; }
}

/// <summary>
/// رفض الحجز من المالك
/// </summary>
public class RejectBookingDto
{
    /// <summary>
    /// سبب الرفض
    /// </summary>
    public required string Reason { get; set; }
}

/// <summary>
/// إلغاء الحجز
/// </summary>
public class CancelBookingDto
{
    /// <summary>
    /// سبب الإلغاء
    /// </summary>
    public required string Reason { get; set; }
}

/// <summary>
/// التحقق من دفع العربون
/// </summary>
public class VerifyDepositPaymentDto
{
    /// <summary>
    /// معرف عملية الدفع
    /// </summary>
    public required string PaymentId { get; set; }

    /// <summary>
    /// معرف المعاملة من بوابة الدفع
    /// </summary>
    public string? TransactionId { get; set; }
}

/// <summary>
/// تحرير الضمان
/// </summary>
public class ReleaseEscrowDto
{
    /// <summary>
    /// المبلغ المراد تحريره (اختياري - يستخدم كامل المبلغ افتراضياً)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// استرداد الضمان للمستأجر
/// </summary>
public class RefundEscrowDto
{
    /// <summary>
    /// المبلغ المراد استرداده
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// سبب الاسترداد
    /// </summary>
    public required string Reason { get; set; }
}
