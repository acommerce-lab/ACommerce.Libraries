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
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<OffersController> _logger;

    public OffersController(
        IRepositoryFactory repositoryFactory,
        ILogger<OffersController> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على العروض المتاحة
    /// </summary>
    /// <param name="latitude">خط العرض (للبحث بالقرب)</param>
    /// <param name="longitude">خط الطول</param>
    /// <param name="radiusKm">نطاق البحث بالكيلومتر</param>
    /// <param name="categoryId">فلترة حسب الفئة</param>
    /// <param name="vendorId">فلترة حسب المتجر</param>
    /// <param name="search">بحث بالاسم</param>
    /// <param name="page">رقم الصفحة</param>
    /// <param name="pageSize">عدد العناصر</param>
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
        var listingRepo = _repositoryFactory.CreateRepository<ProductListing>();
        var vendorRepo = _repositoryFactory.CreateRepository<Vendor>();

        // جلب العروض النشطة
        var now = DateTime.UtcNow;
        var listings = await listingRepo.FindAsync(l =>
            l.IsActive &&
            !l.IsDeleted &&
            l.Status == ACommerce.Catalog.Listings.Enums.ListingStatus.Active &&
            (l.StartDate == null || l.StartDate <= now) &&
            (l.EndDate == null || l.EndDate >= now));

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
                (l.Title != null && l.Title.Contains(search)) ||
                (l.TitleEn != null && l.TitleEn.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (l.Description != null && l.Description.Contains(search)));

        // جلب المتاجر للفلترة بالموقع
        var allVendors = (await vendorRepo.GetAllAsync()).ToDictionary(v => v.Id);

        var results = new List<object>();

        foreach (var listing in query)
        {
            if (!allVendors.TryGetValue(listing.VendorId, out var vendor))
                continue;

            // فلترة بالموقع
            if (latitude.HasValue && longitude.HasValue && vendor.Latitude.HasValue && vendor.Longitude.HasValue)
            {
                var distance = CalculateDistance(
                    latitude.Value, longitude.Value,
                    vendor.Latitude.Value, vendor.Longitude.Value);

                if (distance > radiusKm)
                    continue;
            }

            // التحقق من مواعيد التوافر
            var isOpen = IsVendorOpen(vendor);
            var workingHours = ParseWorkingHours(vendor.WorkingHoursJson);

            results.Add(new
            {
                listing.Id,
                listing.Title,
                listing.TitleEn,
                listing.Description,
                listing.Price,
                listing.OriginalPrice,
                DiscountPercent = listing.OriginalPrice > 0 ?
                    Math.Round((1 - listing.Price / listing.OriginalPrice.Value) * 100) : 0,
                listing.ImageUrl,
                Images = listing.ImagesJson != null ?
                    JsonSerializer.Deserialize<List<string>>(listing.ImagesJson) : null,
                listing.CategoryId,
                listing.AvailableQuantity,
                Vendor = new
                {
                    vendor.Id,
                    vendor.Name,
                    vendor.NameEn,
                    vendor.Description,
                    vendor.LogoUrl,
                    vendor.Latitude,
                    vendor.Longitude,
                    Distance = latitude.HasValue && longitude.HasValue && vendor.Latitude.HasValue && vendor.Longitude.HasValue ?
                        Math.Round(CalculateDistance(latitude.Value, longitude.Value, vendor.Latitude.Value, vendor.Longitude.Value), 1) : (double?)null,
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
        var listingRepo = _repositoryFactory.CreateRepository<ProductListing>();
        var vendorRepo = _repositoryFactory.CreateRepository<Vendor>();

        var listing = await listingRepo.GetByIdAsync(id);
        if (listing == null || listing.IsDeleted || !listing.IsActive)
            return NotFound(new { Message = "العرض غير موجود" });

        var vendor = await vendorRepo.GetByIdAsync(listing.VendorId);

        var isOpen = vendor != null && IsVendorOpen(vendor);
        var workingHours = vendor != null ? ParseWorkingHours(vendor.WorkingHoursJson) : null;

        return Ok(new
        {
            listing.Id,
            listing.Title,
            listing.TitleEn,
            listing.Description,
            listing.Price,
            listing.OriginalPrice,
            DiscountPercent = listing.OriginalPrice > 0 ?
                Math.Round((1 - listing.Price / listing.OriginalPrice.Value) * 100) : 0,
            listing.ImageUrl,
            Images = listing.ImagesJson != null ?
                JsonSerializer.Deserialize<List<string>>(listing.ImagesJson) : null,
            listing.CategoryId,
            listing.AvailableQuantity,
            listing.StartDate,
            listing.EndDate,
            Vendor = vendor != null ? new
            {
                vendor.Id,
                vendor.Name,
                vendor.NameEn,
                vendor.Description,
                vendor.LogoUrl,
                vendor.CoverImageUrl,
                vendor.Latitude,
                vendor.Longitude,
                vendor.ContactPhone,
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
        if (string.IsNullOrEmpty(vendor.WorkingHoursJson))
            return true; // افتراضياً مفتوح إذا لم تحدد المواعيد

        try
        {
            var workingHours = JsonSerializer.Deserialize<Dictionary<string, WorkingDay>>(vendor.WorkingHoursJson);
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
    private static Dictionary<string, object>? ParseWorkingHours(string? json)
    {
        if (string.IsNullOrEmpty(json))
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
