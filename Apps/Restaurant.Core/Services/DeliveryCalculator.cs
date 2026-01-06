using Restaurant.Core.Entities;

namespace Restaurant.Core.Services;

/// <summary>
/// نتيجة حساب التوصيل
/// </summary>
public class DeliveryCalculationResult
{
    /// <summary>
    /// هل التوصيل متاح؟
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// المسافة بالكيلومتر
    /// </summary>
    public decimal DistanceKm { get; set; }

    /// <summary>
    /// رسوم التوصيل
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// الوقت المتوقع (الحد الأدنى) بالدقائق
    /// </summary>
    public int EstimatedMinutesMin { get; set; }

    /// <summary>
    /// الوقت المتوقع (الحد الأقصى) بالدقائق
    /// </summary>
    public int EstimatedMinutesMax { get; set; }

    /// <summary>
    /// نطاق التوصيل المستخدم
    /// </summary>
    public DeliveryZone? DeliveryZone { get; set; }

    /// <summary>
    /// رسالة الخطأ (إذا لم يكن التوصيل متاحاً)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// الحصول على نص الوقت المتوقع
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
    /// الحصول على نص رسوم التوصيل
    /// </summary>
    public string GetDeliveryFeeDisplay()
    {
        return DeliveryFee == 0 ? "مجاني" : $"{DeliveryFee} ريال";
    }

    /// <summary>
    /// نتيجة غير متاحة
    /// </summary>
    public static DeliveryCalculationResult NotAvailable(string message)
    {
        return new DeliveryCalculationResult
        {
            IsAvailable = false,
            ErrorMessage = message
        };
    }
}

/// <summary>
/// خدمة حساب تكلفة التوصيل والمسافة
/// </summary>
public class DeliveryCalculator
{
    /// <summary>
    /// حساب المسافة بين نقطتين باستخدام صيغة هافرساين
    /// </summary>
    public static decimal CalculateDistanceKm(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = earthRadiusKm * c;

        return (decimal)Math.Round(distance, 2);
    }

    /// <summary>
    /// تحويل الدرجات إلى راديان
    /// </summary>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// حساب تكلفة التوصيل بناءً على المسافة ونطاقات المطعم
    /// </summary>
    public DeliveryCalculationResult Calculate(
        RestaurantProfile restaurant,
        double customerLatitude,
        double customerLongitude)
    {
        // حساب المسافة
        var distanceKm = CalculateDistanceKm(
            restaurant.Latitude,
            restaurant.Longitude,
            customerLatitude,
            customerLongitude);

        // التحقق من وجود نطاقات
        var activeZones = restaurant.DeliveryZones
            .Where(z => z.IsActive)
            .OrderBy(z => z.SortOrder)
            .ThenBy(z => z.MaxRadiusKm)
            .ToList();

        if (!activeZones.Any())
        {
            return DeliveryCalculationResult.NotAvailable("المطعم لم يحدد نطاقات التوصيل");
        }

        // البحث عن النطاق المناسب
        var matchingZone = activeZones.FirstOrDefault(z => z.ContainsDistance(distanceKm));

        if (matchingZone == null)
        {
            var maxDistance = activeZones.Max(z => z.MaxRadiusKm);
            return DeliveryCalculationResult.NotAvailable(
                $"موقعك خارج نطاق التوصيل. الحد الأقصى للتوصيل هو {maxDistance} كم");
        }

        // إضافة وقت التحضير للوقت المتوقع
        var preparationTime = restaurant.AveragePreparationTime;

        return new DeliveryCalculationResult
        {
            IsAvailable = true,
            DistanceKm = distanceKm,
            DeliveryFee = matchingZone.DeliveryFee,
            EstimatedMinutesMin = preparationTime + matchingZone.EstimatedMinutesMin,
            EstimatedMinutesMax = preparationTime + matchingZone.EstimatedMinutesMax,
            DeliveryZone = matchingZone
        };
    }

    /// <summary>
    /// التحقق مما إذا كان العميل ضمن نطاق التوصيل
    /// </summary>
    public bool IsWithinDeliveryRange(
        RestaurantProfile restaurant,
        double customerLatitude,
        double customerLongitude)
    {
        var result = Calculate(restaurant, customerLatitude, customerLongitude);
        return result.IsAvailable;
    }

    /// <summary>
    /// الحصول على جميع نطاقات التوصيل مع معلومات المسافة للعميل
    /// </summary>
    public List<DeliveryZoneInfo> GetDeliveryZonesInfo(
        RestaurantProfile restaurant,
        double? customerLatitude = null,
        double? customerLongitude = null)
    {
        var zones = restaurant.DeliveryZones
            .Where(z => z.IsActive)
            .OrderBy(z => z.SortOrder)
            .ThenBy(z => z.MaxRadiusKm)
            .ToList();

        decimal? customerDistance = null;
        if (customerLatitude.HasValue && customerLongitude.HasValue)
        {
            customerDistance = CalculateDistanceKm(
                restaurant.Latitude,
                restaurant.Longitude,
                customerLatitude.Value,
                customerLongitude.Value);
        }

        return zones.Select(z => new DeliveryZoneInfo
        {
            Zone = z,
            IsCustomerInThisZone = customerDistance.HasValue && z.ContainsDistance(customerDistance.Value),
            CustomerDistance = customerDistance
        }).ToList();
    }
}

/// <summary>
/// معلومات نطاق التوصيل مع موقع العميل
/// </summary>
public class DeliveryZoneInfo
{
    public DeliveryZone Zone { get; set; } = null!;
    public bool IsCustomerInThisZone { get; set; }
    public decimal? CustomerDistance { get; set; }
}
