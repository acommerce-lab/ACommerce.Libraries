using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Locations.Api.Controllers;

[ApiController]
[Route("api/locations/cities")]
public class CitiesController : ControllerBase
{
    private readonly ILocationService _locationService;

    public CitiesController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// الحصول على مدينة بالمعرف
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CityDetailDto>> GetCity(
        Guid id,
        CancellationToken ct = default)
    {
        var city = await _locationService.GetCityByIdAsync(id, ct);
        if (city == null) return NotFound();
        return Ok(city);
    }

    /// <summary>
    /// الحصول على أحياء مدينة
    /// </summary>
    [HttpGet("{id:guid}/neighborhoods")]
    public async Task<ActionResult<List<NeighborhoodResponseDto>>> GetCityNeighborhoods(
        Guid id,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var neighborhoods = await _locationService.GetNeighborhoodsByCityAsync(id, activeOnly, ct);
        return Ok(neighborhoods);
    }
}
