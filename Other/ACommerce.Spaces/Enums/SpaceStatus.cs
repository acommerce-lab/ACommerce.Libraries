namespace ACommerce.Spaces.Enums;

/// <summary>
/// حالة المساحة
/// </summary>
public enum SpaceStatus
{
    /// <summary>
    /// مسودة - غير منشورة
    /// </summary>
    Draft = 0,

    /// <summary>
    /// نشطة - متاحة للحجز
    /// </summary>
    Active = 1,

    /// <summary>
    /// معلقة - تحت المراجعة
    /// </summary>
    Pending = 2,

    /// <summary>
    /// غير متاحة مؤقتاً
    /// </summary>
    Unavailable = 3,

    /// <summary>
    /// تحت الصيانة
    /// </summary>
    UnderMaintenance = 4,

    /// <summary>
    /// مؤرشفة
    /// </summary>
    Archived = 5
}
