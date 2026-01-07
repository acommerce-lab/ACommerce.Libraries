using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Restaurant;
using Restaurant.Core.Enums;

namespace Restaurant.Vendor.Api.Controllers;

/// <summary>
/// متحكم إعدادات المطعم
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    /// <summary>
    /// الحصول على إعدادات المطعم
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RestaurantSettingsDto>> GetSettings()
    {
        var settings = new RestaurantSettingsDto
        {
            Id = Guid.NewGuid(),
            Name = "مطعم البركة",
            NameEn = "Al Baraka Restaurant",
            Description = "مطعم متخصص في الأكلات الشعبية السعودية الأصيلة",
            CuisineType = "سعودي",
            LogoUrl = "/images/restaurants/baraka-logo.jpg",
            CoverImageUrl = "/images/restaurants/baraka-cover.jpg",
            Phone = "+966112345678",
            Email = "info@albaraka.com",
            Address = "حي العليا، شارع التحلية، الرياض",
            Latitude = 24.7136,
            Longitude = 46.6753,
            MinimumOrderAmount = 25,
            AveragePreparationMinutes = 20,
            SupportsDelivery = true,
            SupportsPickup = false,
            WorkingHours = GetSampleWorkingHours(),
            DeliveryZones = GetSampleDeliveryZones()
        };

        return Ok(settings);
    }

    /// <summary>
    /// تحديث إعدادات المطعم
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<RestaurantSettingsDto>> UpdateSettings([FromBody] UpdateRestaurantSettingsRequest request)
    {
        // TODO: تحديث في قاعدة البيانات

        return Ok(new
        {
            message = "تم تحديث الإعدادات بنجاح"
        });
    }

    /// <summary>
    /// تحديث الشعار
    /// </summary>
    [HttpPost("logo")]
    public async Task<ActionResult> UploadLogo(IFormFile file)
    {
        // TODO: رفع الصورة
        return Ok(new
        {
            message = "تم تحديث الشعار",
            logoUrl = "/images/restaurants/new-logo.jpg"
        });
    }

    /// <summary>
    /// تحديث صورة الغلاف
    /// </summary>
    [HttpPost("cover")]
    public async Task<ActionResult> UploadCover(IFormFile file)
    {
        // TODO: رفع الصورة
        return Ok(new
        {
            message = "تم تحديث صورة الغلاف",
            coverUrl = "/images/restaurants/new-cover.jpg"
        });
    }

    /// <summary>
    /// تحديث أوقات العمل
    /// </summary>
    [HttpPut("working-hours")]
    public async Task<ActionResult> UpdateWorkingHours([FromBody] List<WorkingHoursDto> workingHours)
    {
        // TODO: تحديث في قاعدة البيانات

        return Ok(new { message = "تم تحديث أوقات العمل" });
    }

    /// <summary>
    /// الحصول على مناطق التوصيل
    /// </summary>
    [HttpGet("delivery-zones")]
    public async Task<ActionResult<List<DeliveryZoneDto>>> GetDeliveryZones()
    {
        return Ok(GetSampleDeliveryZones());
    }

    /// <summary>
    /// إضافة منطقة توصيل
    /// </summary>
    [HttpPost("delivery-zones")]
    public async Task<ActionResult<DeliveryZoneDto>> AddDeliveryZone([FromBody] DeliveryZoneDto zone)
    {
        zone.Id = Guid.NewGuid();
        return CreatedAtAction(nameof(GetDeliveryZones), zone);
    }

    /// <summary>
    /// تحديث منطقة توصيل
    /// </summary>
    [HttpPut("delivery-zones/{zoneId}")]
    public async Task<ActionResult<DeliveryZoneDto>> UpdateDeliveryZone(Guid zoneId, [FromBody] DeliveryZoneDto zone)
    {
        zone.Id = zoneId;
        return Ok(zone);
    }

    /// <summary>
    /// حذف منطقة توصيل
    /// </summary>
    [HttpDelete("delivery-zones/{zoneId}")]
    public async Task<ActionResult> DeleteDeliveryZone(Guid zoneId)
    {
        return Ok(new { message = "تم حذف منطقة التوصيل" });
    }

    /// <summary>
    /// الحصول على حالة الرادار
    /// </summary>
    [HttpGet("radar-status")]
    public async Task<ActionResult> GetRadarStatus()
    {
        return Ok(new
        {
            status = RadarStatus.Open,
            statusText = "مفتوح",
            openedAt = DateTime.UtcNow.AddHours(-3),
            pendingOrdersCount = 2,
            activeOrdersCount = 5
        });
    }

    /// <summary>
    /// تحديث حالة الرادار
    /// </summary>
    [HttpPut("radar-status")]
    public async Task<ActionResult> UpdateRadarStatus([FromBody] UpdateRadarStatusRequest request)
    {
        var statusText = request.Status switch
        {
            RadarStatus.Open => "مفتوح",
            RadarStatus.Busy => "مشغول",
            RadarStatus.Closed => "مغلق",
            _ => "غير معروف"
        };

        // TODO: إرسال إشعار عبر SignalR للعملاء

        return Ok(new
        {
            message = $"تم تحديث حالة الرادار: {statusText}",
            status = request.Status,
            statusText
        });
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

        return Enum.GetValues<DayOfWeek>().Select(day => new WorkingHoursDto
        {
            DayOfWeek = day,
            DayName = arabicDays[day],
            OpenTime = day == DayOfWeek.Friday ? "13:00" : "10:00",
            CloseTime = "23:00",
            IsClosed = false,
            IsToday = day == DateTime.Now.DayOfWeek
        }).ToList();
    }

    private List<DeliveryZoneDto> GetSampleDeliveryZones()
    {
        return new List<DeliveryZoneDto>
        {
            new() { Id = Guid.NewGuid(), Name = "المنطقة القريبة (0-3 كم)", MinDistanceKm = 0, MaxDistanceKm = 3, DeliveryFee = 0, AdditionalMinutes = 0, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "المنطقة المتوسطة (3-6 كم)", MinDistanceKm = 3, MaxDistanceKm = 6, DeliveryFee = 5, AdditionalMinutes = 10, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "المنطقة البعيدة (6-10 كم)", MinDistanceKm = 6, MaxDistanceKm = 10, DeliveryFee = 10, AdditionalMinutes = 20, IsActive = true }
        };
    }
}
