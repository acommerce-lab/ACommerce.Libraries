using ACommerce.Bookings.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Bookings.Entities;

/// <summary>
/// سجل تغييرات حالة الحجز
/// </summary>
public class BookingStatusHistory : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف الحجز
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// الحالة السابقة
    /// </summary>
    public BookingStatus? PreviousStatus { get; set; }

    /// <summary>
    /// الحالة الجديدة
    /// </summary>
    public BookingStatus NewStatus { get; set; }

    /// <summary>
    /// من قام بالتغيير (user id أو system)
    /// </summary>
    public string? ChangedBy { get; set; }

    /// <summary>
    /// ملاحظات التغيير
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// بيانات إضافية (JSON)
    /// </summary>
    public string? Metadata { get; set; }
}
