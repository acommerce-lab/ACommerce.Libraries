using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Spaces.Entities;

/// <summary>
/// أوقات توفر المساحة
/// </summary>
public class SpaceAvailability : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المساحة
    /// </summary>
    public Guid SpaceId { get; set; }
    public Space? Space { get; set; }

    /// <summary>
    /// يوم الأسبوع (0 = الأحد، 6 = السبت)
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// وقت البداية
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// وقت النهاية
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// متاح في هذا اليوم
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// سعر خاص لهذا اليوم
    /// </summary>
    public decimal? SpecialPrice { get; set; }

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// استثناءات التوفر (إغلاقات مؤقتة)
/// </summary>
public class SpaceAvailabilityException : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المساحة
    /// </summary>
    public Guid SpaceId { get; set; }
    public Space? Space { get; set; }

    /// <summary>
    /// تاريخ البداية
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// تاريخ النهاية
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// السبب
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// نوع الاستثناء (إغلاق، صيانة، حجز خاص، إلخ)
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// متكرر سنوياً (للعطل)
    /// </summary>
    public bool IsRecurring { get; set; }
}
