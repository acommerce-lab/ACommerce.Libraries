using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Core.DTOs;
using Restaurant.Core.Entities;
using Restaurant.Core.Enums;
using Restaurant.Core.Services;

namespace Restaurant.Customer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly RestaurantDbContext _context;
    private readonly DeliveryCalculator _deliveryCalculator;
    private readonly RestaurantAvailabilityService _availabilityService;

    public RestaurantsController(
        RestaurantDbContext context,
        DeliveryCalculator deliveryCalculator,
        RestaurantAvailabilityService availabilityService)
    {
        _context = context;
        _deliveryCalculator = deliveryCalculator;
        _availabilityService = availabilityService;
    }

    /// <summary>
    /// الحصول على قائمة المطاعم القريبة مع حالة كل مطعم
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RestaurantListItemDto>>> GetRestaurants(
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] decimal? maxRadius = 15,
        [FromQuery] string? cuisineType = null)
    {
        var query = _context.RestaurantProfiles
            .Include(r => r.WeeklySchedule)
            .Include(r => r.DeliveryZones)
            .AsQueryable();

        if (!string.IsNullOrEmpty(cuisineType))
        {
            query = query.Where(r => r.CuisineType.Contains(cuisineType));
        }

        var restaurants = await query.ToListAsync();

        var result = restaurants.Select(r =>
        {
            // حساب حالة المطعم
            var availability = _availabilityService.GetAvailabilityInfo(r);

            // حساب المسافة والتوصيل إذا تم توفير الموقع
            decimal? distance = null;
            decimal? deliveryFee = null;
            string? deliveryFeeDisplay = null;
            string? estimatedTime = null;

            if (lat.HasValue && lng.HasValue)
            {
                var deliveryCalc = _deliveryCalculator.Calculate(r, lat.Value, lng.Value);
                if (deliveryCalc.IsAvailable)
                {
                    distance = deliveryCalc.DistanceKm;
                    deliveryFee = deliveryCalc.DeliveryFee;
                    deliveryFeeDisplay = deliveryCalc.GetDeliveryFeeDisplay();
                    estimatedTime = deliveryCalc.GetEstimatedTimeDisplay();
                }
                else
                {
                    distance = deliveryCalc.DistanceKm;
                }
            }

            return new RestaurantListItemDto
            {
                Id = r.Id,
                VendorId = r.VendorId,
                Name = r.CuisineType, // TODO: استبدالها باسم المطعم من Vendor
                LogoUrl = null, // TODO: من Vendor
                CuisineType = r.CuisineType,
                Description = r.Description,

                // الحالة - النقطة الأهم
                AvailabilityStatus = availability.Status,
                StatusMessage = availability.StatusMessage,
                StatusColor = availability.StatusColor,
                CanOrder = availability.CanOrder,

                // التقييم
                Rating = null, // TODO: من Reviews
                ReviewCount = 0,

                // التوصيل
                DistanceKm = distance,
                DeliveryFee = deliveryFee,
                DeliveryFeeDisplay = deliveryFeeDisplay,
                EstimatedTime = estimatedTime,

                // الحد الأدنى
                MinimumOrderAmount = r.MinimumOrderAmount
            };
        })
        .OrderBy(r => r.CanOrder ? 0 : 1) // المتاحون أولاً
        .ThenBy(r => r.DistanceKm ?? decimal.MaxValue)
        .ToList();

        // تصفية حسب المسافة القصوى إذا تم تحديد الموقع
        if (lat.HasValue && lng.HasValue && maxRadius.HasValue)
        {
            result = result
                .Where(r => r.DistanceKm == null || r.DistanceKm <= maxRadius.Value)
                .ToList();
        }

        return Ok(result);
    }

    /// <summary>
    /// الحصول على تفاصيل مطعم محدد مع حالته
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantDetailsDto>> GetRestaurant(
        Guid id,
        [FromQuery] double? lat,
        [FromQuery] double? lng)
    {
        var restaurant = await _context.RestaurantProfiles
            .Include(r => r.WeeklySchedule)
            .Include(r => r.DeliveryZones.Where(z => z.IsActive).OrderBy(z => z.SortOrder))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound(new { message = "المطعم غير موجود" });
        }

        // حساب حالة المطعم
        var availability = _availabilityService.GetAvailabilityInfo(restaurant);

        // حساب التوصيل
        decimal? distance = null;
        decimal? deliveryFee = null;
        string? deliveryFeeDisplay = null;
        string? estimatedTime = null;

        if (lat.HasValue && lng.HasValue)
        {
            var deliveryCalc = _deliveryCalculator.Calculate(restaurant, lat.Value, lng.Value);
            distance = deliveryCalc.DistanceKm;
            if (deliveryCalc.IsAvailable)
            {
                deliveryFee = deliveryCalc.DeliveryFee;
                deliveryFeeDisplay = deliveryCalc.GetDeliveryFeeDisplay();
                estimatedTime = deliveryCalc.GetEstimatedTimeDisplay();
            }
        }

        // تحويل الجدول
        var scheduleDto = restaurant.WeeklySchedule
            .OrderBy(s => s.DayOfWeek == DayOfWeek.Saturday ? 0 :
                         s.DayOfWeek == DayOfWeek.Sunday ? 1 :
                         (int)s.DayOfWeek + 1)
            .Select(s => new ScheduleDto
            {
                DayOfWeek = s.DayOfWeek,
                DayName = s.GetArabicDayName(),
                IsOpen = s.IsOpen,
                OpenTime = s.IsOpen ? s.OpenTime.ToString(@"hh\:mm") : null,
                CloseTime = s.IsOpen ? s.CloseTime.ToString(@"hh\:mm") : null,
                BreakTime = s.BreakStartTime.HasValue && s.BreakEndTime.HasValue
                    ? $"{s.BreakStartTime.Value:hh\\:mm} - {s.BreakEndTime.Value:hh\\:mm}"
                    : null,
                FormattedHours = s.GetFormattedHours()
            }).ToList();

        // تحويل نطاقات التوصيل
        var zonesDto = restaurant.DeliveryZones.Select(z => new DeliveryZoneDto
        {
            Id = z.Id,
            Name = z.Name,
            MinRadiusKm = z.MinRadiusKm,
            MaxRadiusKm = z.MaxRadiusKm,
            DeliveryFee = z.DeliveryFee,
            DeliveryFeeDisplay = z.GetDeliveryFeeDisplay(),
            EstimatedMinutesMin = z.EstimatedMinutesMin,
            EstimatedMinutesMax = z.EstimatedMinutesMax,
            EstimatedTimeDisplay = z.GetEstimatedTimeDisplay(),
            IsCustomerInThisZone = distance.HasValue && z.ContainsDistance(distance.Value)
        }).ToList();

        return Ok(new RestaurantDetailsDto
        {
            Id = restaurant.Id,
            VendorId = restaurant.VendorId,
            Name = restaurant.CuisineType, // TODO: من Vendor
            LogoUrl = null,
            CuisineType = restaurant.CuisineType,
            Description = restaurant.Description,

            // الحالة
            AvailabilityStatus = availability.Status,
            StatusMessage = availability.StatusMessage,
            StatusColor = availability.StatusColor,
            CanOrder = availability.CanOrder,

            // الموقع
            FullAddress = restaurant.FullAddress,
            City = restaurant.City,
            Latitude = restaurant.Latitude,
            Longitude = restaurant.Longitude,

            // التوصيل
            DistanceKm = distance,
            DeliveryFee = deliveryFee,
            DeliveryFeeDisplay = deliveryFeeDisplay,
            EstimatedTime = estimatedTime,

            // معلومات إضافية
            AveragePreparationTime = restaurant.AveragePreparationTime,
            MinimumOrderAmount = restaurant.MinimumOrderAmount,
            SupportsDelivery = restaurant.SupportsDelivery,
            SupportsPickup = restaurant.SupportsPickup,

            // الجداول
            WeeklySchedule = scheduleDto,
            DeliveryZones = zonesDto
        });
    }

    /// <summary>
    /// التحقق من حالة المطعم فقط (للتحديث السريع)
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<ActionResult<object>> GetRestaurantStatus(Guid id)
    {
        var restaurant = await _context.RestaurantProfiles
            .Include(r => r.WeeklySchedule)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound(new { message = "المطعم غير موجود" });
        }

        var availability = _availabilityService.GetAvailabilityInfo(restaurant);

        return Ok(new
        {
            id = restaurant.Id,
            status = availability.Status,
            statusMessage = availability.StatusMessage,
            statusColor = availability.StatusColor,
            canOrder = availability.CanOrder,
            cannotOrderReason = availability.CannotOrderReason,
            nextOpenTime = availability.NextOpenTime,
            closingTime = availability.ClosingTime?.ToString(@"hh\:mm"),
            minutesUntilClose = availability.MinutesUntilClose,
            isClosingSoon = availability.IsClosingSoon
        });
    }

    /// <summary>
    /// حساب تكلفة التوصيل لموقع محدد
    /// </summary>
    [HttpGet("{id}/delivery-cost")]
    public async Task<ActionResult<DeliveryCalculationResponseDto>> CalculateDeliveryCost(
        Guid id,
        [FromQuery] double lat,
        [FromQuery] double lng)
    {
        var restaurant = await _context.RestaurantProfiles
            .Include(r => r.DeliveryZones.Where(z => z.IsActive))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound(new { message = "المطعم غير موجود" });
        }

        var result = _deliveryCalculator.Calculate(restaurant, lat, lng);

        return Ok(new DeliveryCalculationResponseDto
        {
            IsAvailable = result.IsAvailable,
            DistanceKm = result.DistanceKm,
            DeliveryFee = result.DeliveryFee,
            DeliveryFeeDisplay = result.GetDeliveryFeeDisplay(),
            EstimatedMinutesMin = result.EstimatedMinutesMin,
            EstimatedMinutesMax = result.EstimatedMinutesMax,
            EstimatedTimeDisplay = result.GetEstimatedTimeDisplay(),
            ErrorMessage = result.ErrorMessage
        });
    }
}
