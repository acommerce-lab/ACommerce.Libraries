namespace ACommerce.Bookings.DTOs;

/// <summary>
/// بيانات تحديث الحجز
/// </summary>
public class UpdateBookingDto
{
    /// <summary>
    /// تاريخ بدء الإيجار
    /// </summary>
    public DateTime? CheckInDate { get; set; }

    /// <summary>
    /// تاريخ انتهاء الإيجار
    /// </summary>
    public DateTime? CheckOutDate { get; set; }

    /// <summary>
    /// ملاحظات المستأجر
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// ملاحظات المالك
    /// </summary>
    public string? HostNotes { get; set; }

    /// <summary>
    /// عدد الضيوف
    /// </summary>
    public int? GuestsCount { get; set; }
}
