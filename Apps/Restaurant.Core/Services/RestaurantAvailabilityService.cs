using Restaurant.Core.Entities;
using Restaurant.Core.Enums;

namespace Restaurant.Core.Services;

/// <summary>
/// معلومات حالة المطعم الكاملة
/// </summary>
public class RestaurantAvailabilityInfo
{
    /// <summary>
    /// حالة التوفر
    /// </summary>
    public RestaurantAvailabilityStatus Status { get; set; }

    /// <summary>
    /// رسالة الحالة للعرض
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// لون الحالة
    /// </summary>
    public string StatusColor { get; set; } = string.Empty;

    /// <summary>
    /// أيقونة الحالة
    /// </summary>
    public string StatusIcon { get; set; } = string.Empty;

    /// <summary>
    /// هل يمكن الطلب الآن؟
    /// </summary>
    public bool CanOrder { get; set; }

    /// <summary>
    /// سبب عدم إمكانية الطلب
    /// </summary>
    public string? CannotOrderReason { get; set; }

    /// <summary>
    /// وقت الفتح التالي (إذا كان مغلقاً)
    /// </summary>
    public DateTime? NextOpenTime { get; set; }

    /// <summary>
    /// وقت الإغلاق (إذا كان مفتوحاً)
    /// </summary>
    public TimeSpan? ClosingTime { get; set; }

    /// <summary>
    /// الوقت المتبقي حتى الإغلاق بالدقائق
    /// </summary>
    public int? MinutesUntilClose { get; set; }

    /// <summary>
    /// هل سيغلق قريباً؟ (أقل من 30 دقيقة)
    /// </summary>
    public bool IsClosingSoon => MinutesUntilClose.HasValue && MinutesUntilClose.Value <= 30;
}

/// <summary>
/// خدمة حساب حالة توفر المطعم
/// </summary>
public class RestaurantAvailabilityService
{
    /// <summary>
    /// الحصول على حالة المطعم الكاملة
    /// </summary>
    public RestaurantAvailabilityInfo GetAvailabilityInfo(RestaurantProfile restaurant)
    {
        var now = DateTime.Now;
        var currentTime = now.TimeOfDay;
        var todaySchedule = restaurant.WeeklySchedule
            .FirstOrDefault(s => s.DayOfWeek == now.DayOfWeek);

        // التحقق من جدول الدوام
        if (todaySchedule == null || !todaySchedule.IsOpen)
        {
            return CreateClosedInfo(restaurant, "المطعم مغلق اليوم", GetNextOpenTime(restaurant, now));
        }

        // التحقق من ساعات العمل
        if (currentTime < todaySchedule.OpenTime)
        {
            var openTime = now.Date.Add(todaySchedule.OpenTime);
            return CreateClosedInfo(restaurant,
                $"المطعم يفتح الساعة {todaySchedule.OpenTime:hh\\:mm}",
                openTime);
        }

        if (currentTime > todaySchedule.CloseTime)
        {
            return CreateClosedInfo(restaurant,
                "المطعم أغلق لهذا اليوم",
                GetNextOpenTime(restaurant, now));
        }

        // التحقق من فترة الاستراحة
        if (todaySchedule.BreakStartTime.HasValue && todaySchedule.BreakEndTime.HasValue)
        {
            if (currentTime >= todaySchedule.BreakStartTime.Value &&
                currentTime <= todaySchedule.BreakEndTime.Value)
            {
                var breakEndTime = now.Date.Add(todaySchedule.BreakEndTime.Value);
                return CreateClosedInfo(restaurant,
                    $"المطعم في استراحة حتى {todaySchedule.BreakEndTime.Value:hh\\:mm}",
                    breakEndTime);
            }
        }

        // الآن نتحقق من حالة الرادار
        var closingTime = todaySchedule.CloseTime;
        var minutesUntilClose = (int)(closingTime - currentTime).TotalMinutes;

        return restaurant.CurrentRadarStatus switch
        {
            RadarStatus.Open => new RestaurantAvailabilityInfo
            {
                Status = RestaurantAvailabilityStatus.Available,
                StatusMessage = "متاح الآن",
                StatusColor = "#22C55E",
                StatusIcon = "check-circle",
                CanOrder = true,
                ClosingTime = closingTime,
                MinutesUntilClose = minutesUntilClose
            },
            RadarStatus.Busy => new RestaurantAvailabilityInfo
            {
                Status = RestaurantAvailabilityStatus.Busy,
                StatusMessage = "مشغول حالياً",
                StatusColor = "#F59E0B",
                StatusIcon = "clock",
                CanOrder = false,
                CannotOrderReason = "المطعم مشغول بطلبات كثيرة، يرجى المحاولة لاحقاً",
                ClosingTime = closingTime,
                MinutesUntilClose = minutesUntilClose
            },
            RadarStatus.Closed => new RestaurantAvailabilityInfo
            {
                Status = RestaurantAvailabilityStatus.Closed,
                StatusMessage = "لا يستقبل طلبات",
                StatusColor = "#EF4444",
                StatusIcon = "x-circle",
                CanOrder = false,
                CannotOrderReason = "المطعم لا يستقبل طلبات جديدة حالياً",
                ClosingTime = closingTime,
                MinutesUntilClose = minutesUntilClose
            },
            _ => CreateClosedInfo(restaurant, "المطعم غير متاح", null)
        };
    }

    /// <summary>
    /// إنشاء معلومات حالة مغلق
    /// </summary>
    private static RestaurantAvailabilityInfo CreateClosedInfo(
        RestaurantProfile restaurant,
        string message,
        DateTime? nextOpenTime)
    {
        return new RestaurantAvailabilityInfo
        {
            Status = RestaurantAvailabilityStatus.Closed,
            StatusMessage = message,
            StatusColor = "#EF4444",
            StatusIcon = "x-circle",
            CanOrder = false,
            CannotOrderReason = message,
            NextOpenTime = nextOpenTime
        };
    }

    /// <summary>
    /// حساب وقت الفتح التالي
    /// </summary>
    private static DateTime? GetNextOpenTime(RestaurantProfile restaurant, DateTime from)
    {
        // البحث في الأيام السبعة القادمة
        for (int i = 0; i < 7; i++)
        {
            var checkDate = from.Date.AddDays(i);
            var schedule = restaurant.WeeklySchedule
                .FirstOrDefault(s => s.DayOfWeek == checkDate.DayOfWeek);

            if (schedule != null && schedule.IsOpen)
            {
                var openDateTime = checkDate.Add(schedule.OpenTime);

                // إذا كان نفس اليوم، تحقق من أن الوقت لم يمر
                if (i == 0 && openDateTime <= from)
                {
                    continue;
                }

                return openDateTime;
            }
        }

        return null;
    }

    /// <summary>
    /// التحقق السريع مما إذا كان المطعم يمكنه استقبال طلبات
    /// </summary>
    public bool CanAcceptOrders(RestaurantProfile restaurant)
    {
        var info = GetAvailabilityInfo(restaurant);
        return info.CanOrder;
    }

    /// <summary>
    /// الحصول على رسالة الحالة القصيرة
    /// </summary>
    public (string Message, string Color) GetStatusBadge(RestaurantProfile restaurant)
    {
        var info = GetAvailabilityInfo(restaurant);
        return (info.StatusMessage, info.StatusColor);
    }
}
