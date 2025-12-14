namespace ACommerce.Locations.Abstractions.DTOs;

// ════════════════════════════════════════════════════════════
// Geo Search DTOs
// ════════════════════════════════════════════════════════════

/// <summary>
/// طلب البحث بالموقع الجغرافي
/// </summary>
public class GeoSearchRequest
{
    /// <summary>
    /// خط العرض
    /// </summary>
    public required double Latitude { get; set; }

    /// <summary>
    /// خط الطول
    /// </summary>
    public required double Longitude { get; set; }

    /// <summary>
    /// نصف القطر بالكيلومتر
    /// </summary>
    public double RadiusKm { get; set; } = 10;

    /// <summary>
    /// الحد الأقصى للنتائج
    /// </summary>
    public int Limit { get; set; } = 50;
}

/// <summary>
/// نتيجة البحث بالموقع
/// </summary>
public class GeoSearchResult<T>
{
    /// <summary>
    /// العنصر
    /// </summary>
    public required T Item { get; set; }

    /// <summary>
    /// المسافة بالكيلومتر
    /// </summary>
    public double DistanceKm { get; set; }
}

/// <summary>
/// نقطة جغرافية بسيطة
/// </summary>
public class GeoPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// حساب المسافة إلى نقطة أخرى (بالكيلومتر)
    /// </summary>
    public double DistanceTo(GeoPoint other)
    {
        const double R = 6371; // نصف قطر الأرض بالكيلومتر

        var lat1 = ToRadians(Latitude);
        var lat2 = ToRadians(other.Latitude);
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}

/// <summary>
/// موقع جغرافي مع التسلسل الهرمي
/// </summary>
public class LocationHierarchyDto
{
    public CountryResponseDto? Country { get; set; }
    public RegionResponseDto? Region { get; set; }
    public CityResponseDto? City { get; set; }
    public NeighborhoodResponseDto? Neighborhood { get; set; }

    /// <summary>
    /// العنوان المختصر
    /// </summary>
    public string GetShortAddress()
    {
        var parts = new List<string>();

        if (Neighborhood != null) parts.Add(Neighborhood.Name);
        if (City != null) parts.Add(City.Name);

        return string.Join(", ", parts);
    }

    /// <summary>
    /// العنوان الكامل
    /// </summary>
    public string GetFullAddress()
    {
        var parts = new List<string>();

        if (Neighborhood != null) parts.Add(Neighborhood.Name);
        if (City != null) parts.Add(City.Name);
        if (Region != null) parts.Add(Region.Name);
        if (Country != null) parts.Add(Country.Name);

        return string.Join(", ", parts);
    }
}
