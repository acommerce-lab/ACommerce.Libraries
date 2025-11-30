using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Locations.Api.Controllers;

[ApiController]
[Route("api/locations/countries")]
public class CountriesController : ControllerBase
{
    private readonly ILocationService _locationService;

    public CountriesController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// الحصول على جميع الدول
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CountryResponseDto>>> GetCountries(
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var countries = await _locationService.GetCountriesAsync(activeOnly, ct);
        return Ok(countries);
    }

    /// <summary>
    /// الحصول على دولة بالمعرف
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CountryDetailDto>> GetCountry(
        Guid id,
        CancellationToken ct = default)
    {
        var country = await _locationService.GetCountryByIdAsync(id, ct);
        if (country == null) return NotFound();
        return Ok(country);
    }

    /// <summary>
    /// الحصول على دولة بالرمز
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<CountryResponseDto>> GetCountryByCode(
        string code,
        CancellationToken ct = default)
    {
        var country = await _locationService.GetCountryByCodeAsync(code, ct);
        if (country == null) return NotFound();
        return Ok(country);
    }

    /// <summary>
    /// الحصول على مناطق دولة
    /// </summary>
    [HttpGet("{id:guid}/regions")]
    public async Task<ActionResult<List<RegionResponseDto>>> GetCountryRegions(
        Guid id,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var regions = await _locationService.GetRegionsByCountryAsync(id, activeOnly, ct);
        return Ok(regions);
    }

    /// <summary>
    /// الحصول على مدن دولة (كل المدن)
    /// </summary>
    [HttpGet("{id:guid}/cities")]
    public async Task<ActionResult<List<CityResponseDto>>> GetCountryCities(
        Guid id,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var cities = await _locationService.GetCitiesByCountryAsync(id, activeOnly, ct);
        return Ok(cities);
    }
}
