using Microsoft.AspNetCore.Mvc;
using Restaurant.Core.DTOs.Restaurant;
using Restaurant.Core.Enums;

namespace Restaurant.Customer.Api.Controllers;

/// <summary>
/// متحكم المطاعم (للعميل)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    /// <summary>
    /// البحث عن المطاعم
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RestaurantSearchResultDto>> SearchRestaurants([FromQuery] SearchRestaurantsRequest request)
    {
        var restaurants = GetSampleRestaurants();

        // تصفية حسب البحث
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            restaurants = restaurants.Where(r =>
                r.Name.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                r.CuisineType.Contains(request.Query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // تصفية حسب نوع المطبخ
        if (!string.IsNullOrWhiteSpace(request.CuisineType))
        {
            restaurants = restaurants.Where(r =>
                r.CuisineType.Equals(request.CuisineType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // تصفية حسب التوفر
        if (request.AvailableOnly == true)
        {
            restaurants = restaurants.Where(r => r.CanOrder).ToList();
        }

        // حساب المسافة إذا كان الموقع متاحاً
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            foreach (var r in restaurants)
            {
                r.DistanceKm = 2.5; // TODO: حساب المسافة الفعلية
                r.EstimatedMinutes = (int)(r.DistanceKm * 4) + 20;
            }

            // تصفية حسب المسافة
            if (request.MaxDistanceKm.HasValue)
            {
                restaurants = restaurants.Where(r => r.DistanceKm <= request.MaxDistanceKm.Value).ToList();
            }
        }

        // الترتيب
        restaurants = request.SortBy switch
        {
            RestaurantSortBy.Rating => restaurants.OrderByDescending(r => r.Rating).ToList(),
            RestaurantSortBy.DeliveryTime => restaurants.OrderBy(r => r.EstimatedMinutes).ToList(),
            RestaurantSortBy.MinimumOrder => restaurants.OrderBy(r => r.MinimumOrderAmount).ToList(),
            _ => restaurants.OrderBy(r => r.DistanceKm).ToList()
        };

        // ترقيم الصفحات
        var totalCount = restaurants.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        restaurants = restaurants
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Ok(new RestaurantSearchResultDto
        {
            Restaurants = restaurants,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        });
    }

    /// <summary>
    /// الحصول على تفاصيل مطعم
    /// </summary>
    [HttpGet("{restaurantId}")]
    public async Task<ActionResult<RestaurantDto>> GetRestaurant(Guid restaurantId)
    {
        var restaurant = new RestaurantDto
        {
            Id = restaurantId,
            Name = "مطعم البركة",
            NameEn = "Al Baraka Restaurant",
            Description = "مطعم متخصص في الأكلات الشعبية السعودية الأصيلة",
            CuisineType = "سعودي",
            LogoUrl = "/images/restaurants/baraka-logo.jpg",
            CoverImageUrl = "/images/restaurants/baraka-cover.jpg",
            Phone = "+966112345678",
            Address = "حي العليا، شارع التحلية، الرياض",
            Latitude = 24.7136,
            Longitude = 46.6753,
            AvailabilityStatus = RestaurantAvailabilityStatus.Available,
            StatusText = "متاح",
            StatusColor = "#22C55E",
            Rating = 4.5m,
            RatingsCount = 230,
            MinimumOrderAmount = 25,
            AveragePreparationMinutes = 20,
            CanOrder = true,
            WorkingHours = GetSampleWorkingHours()
        };

        return Ok(restaurant);
    }

    /// <summary>
    /// الحصول على أنواع المطابخ
    /// </summary>
    [HttpGet("cuisines")]
    public async Task<ActionResult<List<CuisineTypeDto>>> GetCuisineTypes()
    {
        var cuisines = new List<CuisineTypeDto>
        {
            new() { Id = "saudi", Name = "سعودي", NameEn = "Saudi", Icon = "🍖", RestaurantsCount = 45 },
            new() { Id = "yemeni", Name = "يمني", NameEn = "Yemeni", Icon = "🍗", RestaurantsCount = 30 },
            new() { Id = "lebanese", Name = "لبناني", NameEn = "Lebanese", Icon = "🥙", RestaurantsCount = 25 },
            new() { Id = "indian", Name = "هندي", NameEn = "Indian", Icon = "🍛", RestaurantsCount = 35 },
            new() { Id = "fastfood", Name = "وجبات سريعة", NameEn = "Fast Food", Icon = "🍔", RestaurantsCount = 50 },
            new() { Id = "pizza", Name = "بيتزا", NameEn = "Pizza", Icon = "🍕", RestaurantsCount = 20 },
            new() { Id = "seafood", Name = "مأكولات بحرية", NameEn = "Seafood", Icon = "🦐", RestaurantsCount = 15 },
            new() { Id = "desserts", Name = "حلويات", NameEn = "Desserts", Icon = "🍰", RestaurantsCount = 40 }
        };

        return Ok(cuisines);
    }

    /// <summary>
    /// الحصول على المطاعم القريبة
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<List<RestaurantSummaryDto>>> GetNearbyRestaurants(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] int limit = 10)
    {
        var restaurants = GetSampleRestaurants()
            .Where(r => r.CanOrder)
            .Take(limit)
            .ToList();

        return Ok(restaurants);
    }

    /// <summary>
    /// الحصول على المطاعم المميزة
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<List<RestaurantSummaryDto>>> GetFeaturedRestaurants()
    {
        var restaurants = GetSampleRestaurants()
            .OrderByDescending(r => r.Rating)
            .Take(5)
            .ToList();

        return Ok(restaurants);
    }

    /// <summary>
    /// الحصول على حالة المطعم
    /// </summary>
    [HttpGet("{restaurantId}/status")]
    public async Task<ActionResult> GetRestaurantStatus(Guid restaurantId)
    {
        return Ok(new
        {
            restaurantId,
            availabilityStatus = RestaurantAvailabilityStatus.Available,
            statusText = "متاح",
            statusColor = "#22C55E",
            canOrder = true,
            estimatedMinutes = 30,
            message = (string?)null
        });
    }

    /// <summary>
    /// الحصول على مناطق التوصيل
    /// </summary>
    [HttpGet("{restaurantId}/delivery-zones")]
    public async Task<ActionResult<List<DeliveryZoneDto>>> GetDeliveryZones(Guid restaurantId)
    {
        var zones = new List<DeliveryZoneDto>
        {
            new() { Id = Guid.NewGuid(), Name = "المنطقة القريبة", MinDistanceKm = 0, MaxDistanceKm = 3, DeliveryFee = 0, AdditionalMinutes = 0, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "المنطقة المتوسطة", MinDistanceKm = 3, MaxDistanceKm = 6, DeliveryFee = 5, AdditionalMinutes = 10, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "المنطقة البعيدة", MinDistanceKm = 6, MaxDistanceKm = 10, DeliveryFee = 10, AdditionalMinutes = 20, IsActive = true }
        };

        return Ok(zones);
    }

    private List<RestaurantSummaryDto> GetSampleRestaurants()
    {
        return new List<RestaurantSummaryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "مطعم البركة",
                NameEn = "Al Baraka",
                CuisineType = "سعودي",
                AvailabilityStatus = RestaurantAvailabilityStatus.Available,
                StatusText = "متاح",
                StatusColor = "#22C55E",
                Rating = 4.5m,
                MinimumOrderAmount = 25,
                DeliveryFee = 0,
                EstimatedMinutes = 30,
                DistanceKm = 2.1,
                CanOrder = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "مطعم الشرق",
                NameEn = "Al Sharq",
                CuisineType = "يمني",
                AvailabilityStatus = RestaurantAvailabilityStatus.Available,
                StatusText = "متاح",
                StatusColor = "#22C55E",
                Rating = 4.2m,
                MinimumOrderAmount = 20,
                DeliveryFee = 5,
                EstimatedMinutes = 35,
                DistanceKm = 3.5,
                CanOrder = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "مطعم النخيل",
                NameEn = "Al Nakheel",
                CuisineType = "لبناني",
                AvailabilityStatus = RestaurantAvailabilityStatus.Busy,
                StatusText = "مشغول",
                StatusColor = "#F59E0B",
                Rating = 4.7m,
                MinimumOrderAmount = 30,
                DeliveryFee = 5,
                EstimatedMinutes = 45,
                DistanceKm = 4.2,
                CanOrder = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "مطعم الأصيل",
                NameEn = "Al Aseel",
                CuisineType = "سعودي",
                AvailabilityStatus = RestaurantAvailabilityStatus.Closed,
                StatusText = "مغلق",
                StatusColor = "#EF4444",
                Rating = 4.3m,
                MinimumOrderAmount = 25,
                DeliveryFee = 0,
                EstimatedMinutes = 0,
                DistanceKm = 1.8,
                CanOrder = false
            }
        };
    }

    private List<WorkingHoursDto> GetSampleWorkingHours()
    {
        var arabicDays = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Sunday, "الأحد" },
            { DayOfWeek.Monday, "الإثنين" },
            { DayOfWeek.Tuesday, "الثلاثاء" },
            { DayOfWeek.Wednesday, "الأربعاء" },
            { DayOfWeek.Thursday, "الخميس" },
            { DayOfWeek.Friday, "الجمعة" },
            { DayOfWeek.Saturday, "السبت" }
        };

        var today = DateTime.Now.DayOfWeek;

        return Enum.GetValues<DayOfWeek>().Select(day => new WorkingHoursDto
        {
            DayOfWeek = day,
            DayName = arabicDays[day],
            OpenTime = day == DayOfWeek.Friday ? "13:00" : "10:00",
            CloseTime = "23:00",
            IsClosed = false,
            IsToday = day == today
        }).ToList();
    }
}
