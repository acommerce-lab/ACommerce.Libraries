using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.Entities;

namespace Restaurant.Vendor.Api.Controllers;

/// <summary>
/// متحكم إدارة السائقين
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    /// <summary>
    /// الحصول على قائمة السائقين
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DriverDto>>> GetDrivers([FromQuery] DriverStatus? status)
    {
        var drivers = GetSampleDrivers();

        if (status.HasValue)
            drivers = drivers.Where(d => d.Status == status.Value).ToList();

        return Ok(drivers);
    }

    /// <summary>
    /// الحصول على سائق محدد
    /// </summary>
    [HttpGet("{driverId}")]
    public async Task<ActionResult<DriverDto>> GetDriver(Guid driverId)
    {
        var driver = GetSampleDrivers().FirstOrDefault();
        if (driver == null)
            return NotFound(new { message = "السائق غير موجود" });

        driver.Id = driverId;
        return Ok(driver);
    }

    /// <summary>
    /// إضافة سائق جديد
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DriverDto>> AddDriver([FromBody] CreateDriverRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "اسم السائق مطلوب" });

        if (string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest(new { message = "رقم الهاتف مطلوب" });

        var driver = new DriverDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Phone = request.Phone,
            Status = DriverStatus.Offline,
            Rating = 0,
            TotalDeliveries = 0,
            IsActive = true
        };

        return CreatedAtAction(nameof(GetDriver), new { driverId = driver.Id }, driver);
    }

    /// <summary>
    /// تحديث بيانات سائق
    /// </summary>
    [HttpPut("{driverId}")]
    public async Task<ActionResult<DriverDto>> UpdateDriver(Guid driverId, [FromBody] UpdateDriverRequest request)
    {
        var driver = new DriverDto
        {
            Id = driverId,
            Name = request.Name ?? "سائق",
            Phone = request.Phone ?? "",
            IsActive = request.IsActive ?? true,
            Status = DriverStatus.Available
        };

        return Ok(driver);
    }

    /// <summary>
    /// تعطيل/تفعيل سائق
    /// </summary>
    [HttpPatch("{driverId}/toggle-active")]
    public async Task<ActionResult> ToggleDriverActive(Guid driverId)
    {
        return Ok(new
        {
            message = "تم تحديث حالة السائق",
            driverId,
            isActive = true
        });
    }

    /// <summary>
    /// حذف سائق
    /// </summary>
    [HttpDelete("{driverId}")]
    public async Task<ActionResult> DeleteDriver(Guid driverId)
    {
        return Ok(new { message = "تم حذف السائق" });
    }

    /// <summary>
    /// الحصول على السائقين المتاحين
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult<List<DriverDto>>> GetAvailableDrivers()
    {
        var drivers = GetSampleDrivers()
            .Where(d => d.Status == DriverStatus.Available)
            .ToList();

        return Ok(drivers);
    }

    /// <summary>
    /// الحصول على موقع السائقين
    /// </summary>
    [HttpGet("locations")]
    public async Task<ActionResult<List<DriverLocationDto>>> GetDriversLocations()
    {
        var locations = GetSampleDrivers()
            .Where(d => d.Status != DriverStatus.Offline && d.LastLatitude.HasValue)
            .Select(d => new DriverLocationDto
            {
                DriverId = d.Id,
                DriverName = d.Name,
                Latitude = d.LastLatitude ?? 0,
                Longitude = d.LastLongitude ?? 0,
                Status = d.Status,
                LastUpdate = d.LastLocationUpdate ?? DateTime.UtcNow
            })
            .ToList();

        return Ok(locations);
    }

    /// <summary>
    /// إحصائيات السائقين
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetDriversStats()
    {
        return Ok(new
        {
            totalDrivers = 5,
            availableDrivers = 3,
            busyDrivers = 1,
            offlineDrivers = 1,
            todayDeliveries = 45,
            averageDeliveryTime = 28 // بالدقائق
        });
    }

    private List<DriverDto> GetSampleDrivers()
    {
        return new List<DriverDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "أحمد محمد",
                Phone = "+966501111111",
                PhotoUrl = "/images/drivers/ahmed.jpg",
                Status = DriverStatus.Available,
                Rating = 4.8m,
                TotalDeliveries = 150,
                IsActive = true,
                LastLatitude = 24.7200,
                LastLongitude = 46.6800,
                LastLocationUpdate = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "خالد سعد",
                Phone = "+966502222222",
                Status = DriverStatus.Busy,
                Rating = 4.5m,
                TotalDeliveries = 120,
                IsActive = true,
                LastLatitude = 24.7150,
                LastLongitude = 46.6750,
                LastLocationUpdate = DateTime.UtcNow.AddMinutes(-2),
                CurrentOrderId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "عبدالله علي",
                Phone = "+966503333333",
                Status = DriverStatus.Available,
                Rating = 4.9m,
                TotalDeliveries = 200,
                IsActive = true,
                LastLatitude = 24.7180,
                LastLongitude = 46.6780,
                LastLocationUpdate = DateTime.UtcNow.AddMinutes(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "فهد عبدالرحمن",
                Phone = "+966504444444",
                Status = DriverStatus.Offline,
                Rating = 4.6m,
                TotalDeliveries = 80,
                IsActive = true
            }
        };
    }
}

/// <summary>
/// بيانات السائق
/// </summary>
public class DriverDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DriverStatus Status { get; set; }
    public decimal Rating { get; set; }
    public int TotalDeliveries { get; set; }
    public bool IsActive { get; set; }
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public Guid? CurrentOrderId { get; set; }
}

/// <summary>
/// موقع السائق
/// </summary>
public class DriverLocationDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DriverStatus Status { get; set; }
    public DateTime LastUpdate { get; set; }
}

/// <summary>
/// طلب إضافة سائق
/// </summary>
public class CreateDriverRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// طلب تحديث سائق
/// </summary>
public class UpdateDriverRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public bool? IsActive { get; set; }
}
