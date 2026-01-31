using Microsoft.AspNetCore.Mvc;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Vendors.Entities;
using System.Text.Json;

namespace Order.Api.Controllers;

/// <summary>
/// عرض العروض المتاحة مع دعم البحث بالموقع
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly IBaseAsyncRepository<ProductListing> _listingRepository;
    private readonly IBaseAsyncRepository<Vendor> _vendorRepository;
    private readonly ILogger<OffersController> _logger;

    public OffersController(
        IBaseAsyncRepository<ProductListing> listingRepository,
        IBaseAsyncRepository<Vendor> vendorRepository,
        ILogger<OffersController> logger)
    {
        _listingRepository = listingRepository;
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على العروض المتاحة
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOffers(
        [FromQuery] double? latitude = null,
        [FromQuery] double? longitude = null,
        [FromQuery] double radiusKm = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? vendorId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var now = DateTime.UtcNow;

        // جلب العروض النشطة
        var listings = await _listingRepository.GetAllWithPredicateAsync(l =>
            l.IsActive &&
            !l.IsDeleted &&
            l.Status == ACommerce.Catalog.Listings.Enums.ListingStatus.Active &&
            (l.StartsAt == null || l.StartsAt <= now) &&
            (l.EndsAt == null || l.EndsAt >= now));

        var query = listings.AsQueryable();

        // فلترة حسب الفئة
        if (categoryId.HasValue)
            query = query.Where(l => l.CategoryId == categoryId.Value);

        // فلترة حسب المتجر
        if (vendorId.HasValue)
            query = query.Where(l => l.VendorId == vendorId.Value);

        // البحث بالاسم
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l =>
                (l.Title != null && l.Title.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (l.Description != null && l.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));

        // جلب المتاجر للفلترة بالموقع
        var allVendors = (await _vendorRepository.ListAllAsync()).ToDictionary(v => v.Id);

        var results = new List<object>();

        foreach (var listing in query)
        {
            if (!allVendors.TryGetValue(listing.VendorId, out var vendor))
                continue;

            // استخراج إحداثيات المتجر من Metadata أو من Listing
            double? vendorLat = listing.Latitude;
            double? vendorLng = listing.Longitude;

            if (!vendorLat.HasValue && vendor.Metadata.TryGetValue("latitude", out var latStr) && double.TryParse(latStr, out var lat))
                vendorLat = lat;
            if (!vendorLng.HasValue && vendor.Metadata.TryGetValue("longitude", out var lngStr) && double.TryParse(lngStr, out var lng))
                vendorLng = lng;

            // فلترة بالموقع
            double? distance = null;
            if (latitude.HasValue && longitude.HasValue && vendorLat.HasValue && vendorLng.HasValue)
            {
                distance = CalculateDistance(latitude.Value, longitude.Value, vendorLat.Value, vendorLng.Value);
                if (distance > radiusKm)
                    continue;
            }

            // التحقق من مواعيد التوافر
            var isOpen = IsVendorOpen(vendor);
            var workingHours = ParseWorkingHours(vendor);

            results.Add(new
            {
                listing.Id,
                listing.Title,
                listing.Description,
                listing.Price,
                OriginalPrice = listing.CompareAtPrice,
                DiscountPercent = listing.CompareAtPrice.HasValue && listing.CompareAtPrice > 0 ?
                    Math.Round((1 - listing.Price / listing.CompareAtPrice.Value) * 100) : 0,
                ImageUrl = listing.FeaturedImage,
                Images = !string.IsNullOrEmpty(listing.ImagesJson) ?
                    JsonSerializer.Deserialize<List<string>>(listing.ImagesJson) : null,
                listing.CategoryId,
                AvailableQuantity = listing.QuantityAvailable,
                Vendor = new
                {
                    vendor.Id,
                    Name = vendor.StoreName,
                    vendor.Description,
                    LogoUrl = vendor.Logo,
                    Latitude = vendorLat,
                    Longitude = vendorLng,
                    Distance = distance.HasValue ? Math.Round(distance.Value, 1) : (double?)null,
                    IsOpen = isOpen,
                    WorkingHours = workingHours
                }
            });
        }

        // ترتيب حسب المسافة إذا كان الموقع متاحاً
        if (latitude.HasValue && longitude.HasValue)
        {
            results = results.OrderBy(r => ((dynamic)r).Vendor.Distance ?? double.MaxValue).ToList();
        }

        // تقسيم الصفحات
        var total = results.Count;
        var paged = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            Items = paged,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    /// <summary>
    /// الحصول على تفاصيل عرض
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOffer(Guid id)
    {
        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null || listing.IsDeleted || !listing.IsActive)
            return NotFound(new { Message = "العرض غير موجود" });

        var vendor = await _vendorRepository.GetByIdAsync(listing.VendorId);

        // استخراج إحداثيات المتجر
        double? vendorLat = listing.Latitude;
        double? vendorLng = listing.Longitude;

        if (vendor != null)
        {
            if (!vendorLat.HasValue && vendor.Metadata.TryGetValue("latitude", out var latStr) && double.TryParse(latStr, out var lat))
                vendorLat = lat;
            if (!vendorLng.HasValue && vendor.Metadata.TryGetValue("longitude", out var lngStr) && double.TryParse(lngStr, out var lng))
                vendorLng = lng;
        }

        var isOpen = vendor != null && IsVendorOpen(vendor);
        var workingHours = vendor != null ? ParseWorkingHours(vendor) : null;

        return Ok(new
        {
            listing.Id,
            listing.Title,
            listing.Description,
            listing.Price,
            OriginalPrice = listing.CompareAtPrice,
            DiscountPercent = listing.CompareAtPrice.HasValue && listing.CompareAtPrice > 0 ?
                Math.Round((1 - listing.Price / listing.CompareAtPrice.Value) * 100) : 0,
            ImageUrl = listing.FeaturedImage,
            Images = !string.IsNullOrEmpty(listing.ImagesJson) ?
                JsonSerializer.Deserialize<List<string>>(listing.ImagesJson) : null,
            listing.CategoryId,
            AvailableQuantity = listing.QuantityAvailable,
            StartDate = listing.StartsAt,
            EndDate = listing.EndsAt,
            Vendor = vendor != null ? new
            {
                vendor.Id,
                Name = vendor.StoreName,
                vendor.Description,
                LogoUrl = vendor.Logo,
                CoverImageUrl = vendor.BannerImage,
                Latitude = vendorLat,
                Longitude = vendorLng,
                ContactPhone = vendor.Metadata.GetValueOrDefault("phone", ""),
                IsOpen = isOpen,
                WorkingHours = workingHours
            } : null
        });
    }

    /// <summary>
    /// حساب المسافة بين نقطتين (Haversine formula)
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // نصف قطر الأرض بالكيلومتر

        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRad(double deg) => deg * (Math.PI / 180);

    /// <summary>
    /// التحقق من أن المتجر مفتوح حالياً
    /// </summary>
    private static bool IsVendorOpen(Vendor vendor)
    {
        if (!vendor.Metadata.TryGetValue("working_hours", out var workingHoursJson) || string.IsNullOrEmpty(workingHoursJson))
            return true; // افتراضياً مفتوح إذا لم تحدد المواعيد

        try
        {
            var workingHours = JsonSerializer.Deserialize<Dictionary<string, WorkingDay>>(workingHoursJson);
            if (workingHours == null)
                return true;

            var now = DateTime.Now; // التوقيت المحلي
            var dayName = now.DayOfWeek.ToString();

            if (!workingHours.TryGetValue(dayName, out var day))
                return false;

            if (string.IsNullOrEmpty(day.Open) || string.IsNullOrEmpty(day.Close))
                return false;

            var openTime = TimeSpan.Parse(day.Open);
            var closeTime = TimeSpan.Parse(day.Close);
            var currentTime = now.TimeOfDay;

            // التعامل مع المتاجر التي تغلق بعد منتصف الليل
            if (closeTime < openTime)
            {
                return currentTime >= openTime || currentTime <= closeTime;
            }

            return currentTime >= openTime && currentTime <= closeTime;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// تحويل مواعيد العمل لصيغة مقروءة
    /// </summary>
    private static Dictionary<string, object>? ParseWorkingHours(Vendor vendor)
    {
        if (!vendor.Metadata.TryGetValue("working_hours", out var json) || string.IsNullOrEmpty(json))
            return null;

        try
        {
            var hours = JsonSerializer.Deserialize<Dictionary<string, WorkingDay>>(json);
            return hours?.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)new { kvp.Value.Open, kvp.Value.Close });
        }
        catch
        {
            return null;
        }
    }

    private class WorkingDay
    {
        public string? Open { get; set; }
        public string? Close { get; set; }
    }
}
