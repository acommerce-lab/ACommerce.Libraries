using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Locations.Api.Controllers;

[ApiController]
[Route("api/locations/neighborhoods")]
public class NeighborhoodsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public NeighborhoodsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// الحصول على حي بالمعرف
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NeighborhoodDetailDto>> GetNeighborhood(
        Guid id,
        CancellationToken ct = default)
    {
        var neighborhood = await _locationService.GetNeighborhoodByIdAsync(id, ct);
        if (neighborhood == null) return NotFound();
        return Ok(neighborhood);
    }
}
