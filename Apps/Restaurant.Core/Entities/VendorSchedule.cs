using ACommerce.SharedKernel.Abstractions.Entities;

namespace Restaurant.Core.Entities;

/// <summary>
/// جدول الدوام اليومي للمطعم
/// </summary>
public class VendorSchedule : IBaseEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف ملف المطعم
    /// </summary>
    public Guid RestaurantProfileId { get; set; }

    /// <summary>
    /// ملف المطعم
    /// </summary>
    public RestaurantProfile? RestaurantProfile { get; set; }

    /// <summary>
    /// يوم الأسبوع (السبت = 6، الأحد = 0، ... الجمعة = 5)
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// هل المطعم يعمل في هذا اليوم؟
    /// </summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>
    /// وقت الفتح
    /// </summary>
    public TimeSpan OpenTime { get; set; } = new TimeSpan(8, 0, 0); // 08:00

    /// <summary>
    /// وقت الإغلاق
    /// </summary>
    public TimeSpan CloseTime { get; set; } = new TimeSpan(23, 0, 0); // 23:00

    /// <summary>
    /// بداية فترة الاستراحة (اختياري)
    /// </summary>
    public TimeSpan? BreakStartTime { get; set; }

    /// <summary>
    /// نهاية فترة الاستراحة (اختياري)
    /// </summary>
    public TimeSpan? BreakEndTime { get; set; }

    /// <summary>
    /// الحصول على اسم اليوم بالعربية
    /// </summary>
    public string GetArabicDayName()
    {
        return DayOfWeek switch
        {
            DayOfWeek.Saturday => "السبت",
            DayOfWeek.Sunday => "الأحد",
            DayOfWeek.Monday => "الاثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الأربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            _ => "غير معروف"
        };
    }

    /// <summary>
    /// تنسيق ساعات العمل للعرض
    /// </summary>
    public string GetFormattedHours()
    {
        if (!IsOpen)
        {
            return "مغلق";
        }

        var hours = $"{OpenTime:hh\\:mm} - {CloseTime:hh\\:mm}";

        if (BreakStartTime.HasValue && BreakEndTime.HasValue)
        {
            hours += $" (استراحة: {BreakStartTime.Value:hh\\:mm} - {BreakEndTime.Value:hh\\:mm})";
        }

        return hours;
    }
}
