using ACommerce.SharedKernel.Abstractions.Entities;

namespace Restaurant.Core.Entities;

/// <summary>
/// نطاق التوصيل - يحدد المسافة ورسوم التوصيل
/// مثال:
/// - النطاق 1: 0-3 كم = مجاني
/// - النطاق 2: 3-6 كم = 5 ريال
/// - النطاق 3: 6-10 كم = 10 ريال
/// </summary>
public class DeliveryZone : IBaseEntity
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
    /// اسم النطاق للعرض (مثل: "النطاق الأول"، "المنطقة القريبة")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// الحد الأدنى للمسافة بالكيلومتر (من أين يبدأ هذا النطاق)
    /// </summary>
    public decimal MinRadiusKm { get; set; }

    /// <summary>
    /// الحد الأقصى للمسافة بالكيلومتر (إلى أين ينتهي هذا النطاق)
    /// </summary>
    public decimal MaxRadiusKm { get; set; }

    /// <summary>
    /// رسوم التوصيل لهذا النطاق
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// الوقت المتوقع للتوصيل بالدقائق (الحد الأدنى)
    /// </summary>
    public int EstimatedMinutesMin { get; set; }

    /// <summary>
    /// الوقت المتوقع للتوصيل بالدقائق (الحد الأقصى)
    /// </summary>
    public int EstimatedMinutesMax { get; set; }

    /// <summary>
    /// هل النطاق نشط؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// التحقق مما إذا كانت المسافة ضمن هذا النطاق
    /// </summary>
    public bool ContainsDistance(decimal distanceKm)
    {
        return distanceKm >= MinRadiusKm && distanceKm < MaxRadiusKm;
    }

    /// <summary>
    /// الحصول على نص رسوم التوصيل للعرض
    /// </summary>
    public string GetDeliveryFeeDisplay()
    {
        return DeliveryFee == 0 ? "مجاني" : $"{DeliveryFee} ريال";
    }

    /// <summary>
    /// الحصول على نص الوقت المتوقع للعرض
    /// </summary>
    public string GetEstimatedTimeDisplay()
    {
        if (EstimatedMinutesMin == EstimatedMinutesMax)
        {
            return $"{EstimatedMinutesMin} دقيقة";
        }
        return $"{EstimatedMinutesMin}-{EstimatedMinutesMax} دقيقة";
    }

    /// <summary>
    /// الحصول على وصف النطاق الكامل
    /// </summary>
    public string GetFullDescription()
    {
        var distance = MinRadiusKm == 0
            ? $"حتى {MaxRadiusKm} كم"
            : $"من {MinRadiusKm} إلى {MaxRadiusKm} كم";

        return $"{distance} - {GetDeliveryFeeDisplay()} ({GetEstimatedTimeDisplay()})";
    }
}
