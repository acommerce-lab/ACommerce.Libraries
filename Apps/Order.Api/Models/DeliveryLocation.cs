namespace Order.Api.Models;

/// <summary>
/// موقع التوصيل للسيارة - يستخدم عندما يختار العميل توصيل للسيارة
/// </summary>
public class DeliveryLocation
{
    /// <summary>
    /// خط العرض
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// خط الطول
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// وصف إضافي للموقع (اختياري)
    /// مثال: "أمام البوابة الرئيسية" أو "الموقف رقم 5"
    /// </summary>
    public string? LocationDescription { get; set; }

    /// <summary>
    /// هل الموقع مباشر (Live Location)
    /// إذا كان true، يتم تحديث الموقع تلقائياً
    /// </summary>
    public bool IsLiveLocation { get; set; }

    /// <summary>
    /// آخر تحديث للموقع
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// معلومات السيارة (اختياري - للتوصيل للسيارة)
/// </summary>
public class CarInfo
{
    /// <summary>
    /// نوع السيارة
    /// </summary>
    public string? CarModel { get; set; }

    /// <summary>
    /// لون السيارة
    /// </summary>
    public string? CarColor { get; set; }

    /// <summary>
    /// رقم اللوحة (اختياري)
    /// </summary>
    public string? PlateNumber { get; set; }
}
