using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Locations.Api.Controllers;

[ApiController]
[Route("api/locations/regions")]
public class RegionsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public RegionsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// الحصول على منطقة بالمعرف
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RegionDetailDto>> GetRegion(
        Guid id,
        CancellationToken ct = default)
    {
        var region = await _locationService.GetRegionByIdAsync(id, ct);
        if (region == null) return NotFound();
        return Ok(region);
    }

    /// <summary>
    /// الحصول على مدن منطقة
    /// </summary>
    [HttpGet("{id:guid}/cities")]
    public async Task<ActionResult<List<CityResponseDto>>> GetRegionCities(
        Guid id,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var cities = await _locationService.GetCitiesByRegionAsync(id, activeOnly, ct);
        return Ok(cities);
    }
}
