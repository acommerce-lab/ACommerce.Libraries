using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using ACommerce.Locations.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Locations.Services;

public class LocationService : ILocationService
{
    private readonly DbContext _context;

    public LocationService(DbContext context)
    {
        _context = context;
    }

    // ══════════════════════════════════════════════════════════════════
    // Countries
    // ══════════════════════════════════════════════════════════════════

    public async Task<List<CountryResponseDto>> GetCountriesAsync(
        bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = _context.Set<Country>().AsNoTracking();

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => MapToCountryResponse(x))
            .ToListAsync(ct);
    }

    public async Task<CountryDetailDto?> GetCountryByIdAsync(Guid id, CancellationToken ct = default)
    {
        var country = await _context.Set<Country>()
            .AsNoTracking()
            .Include(x => x.Regions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (country == null) return null;

        return new CountryDetailDto
        {
            Id = country.Id,
            Name = country.Name,
            NameEn = country.NameEn,
            Code = country.Code,
            Code3 = country.Code3,
            NumericCode = country.NumericCode,
            PhoneCode = country.PhoneCode,
            CurrencyCode = country.CurrencyCode,
            CurrencyName = country.CurrencyName,
            CurrencySymbol = country.CurrencySymbol,
            Flag = country.Flag,
            Latitude = country.Latitude,
            Longitude = country.Longitude,
            Timezone = country.Timezone,
            IsActive = country.IsActive,
            SortOrder = country.SortOrder,
            RegionsCount = country.Regions.Count
        };
    }

    public async Task<CountryResponseDto?> GetCountryByCodeAsync(string code, CancellationToken ct = default)
    {
        var country = await _context.Set<Country>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.ToUpper(), ct);

        return country == null ? null : MapToCountryResponse(country);
    }

    // ══════════════════════════════════════════════════════════════════
    // Regions
    // ══════════════════════════════════════════════════════════════════

    public async Task<List<RegionResponseDto>> GetRegionsByCountryAsync(
        Guid countryId,
        bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = _context.Set<Region>()
            .AsNoTracking()
            .Include(x => x.Country)
            .Where(x => x.CountryId == countryId);

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => MapToRegionResponse(x))
            .ToListAsync(ct);
    }

    public async Task<RegionDetailDto?> GetRegionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var region = await _context.Set<Region>()
            .AsNoTracking()
            .Include(x => x.Country)
            .Include(x => x.Cities)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (region == null) return null;

        return new RegionDetailDto
        {
            Id = region.Id,
            Name = region.Name,
            NameEn = region.NameEn,
            Code = region.Code,
            Type = region.Type,
            CountryId = region.CountryId,
            CountryName = region.Country?.Name,
            Latitude = region.Latitude,
            Longitude = region.Longitude,
            Timezone = region.Timezone,
            IsActive = region.IsActive,
            SortOrder = region.SortOrder,
            CitiesCount = region.Cities.Count,
            Country = region.Country == null ? null : MapToCountryResponse(region.Country)
        };
    }

    // ══════════════════════════════════════════════════════════════════
    // Cities
    // ══════════════════════════════════════════════════════════════════

    public async Task<List<CityResponseDto>> GetCitiesByRegionAsync(
        Guid regionId,
        bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = _context.Set<City>()
            .AsNoTracking()
            .Include(x => x.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.RegionId == regionId);

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => MapToCityResponse(x))
            .ToListAsync(ct);
    }

    public async Task<List<CityResponseDto>> GetCitiesByCountryAsync(
        Guid countryId,
        bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = _context.Set<City>()
            .AsNoTracking()
            .Include(x => x.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.Region!.CountryId == countryId);

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => MapToCityResponse(x))
            .ToListAsync(ct);
    }

    public async Task<CityDetailDto?> GetCityByIdAsync(Guid id, CancellationToken ct = default)
    {
        var city = await _context.Set<City>()
            .AsNoTracking()
            .Include(x => x.Region)
            .ThenInclude(r => r!.Country)
            .Include(x => x.Neighborhoods)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (city == null) return null;

        return new CityDetailDto
        {
            Id = city.Id,
            Name = city.Name,
            NameEn = city.NameEn,
            Code = city.Code,
            RegionId = city.RegionId,
            RegionName = city.Region?.Name,
            CountryId = city.Region?.CountryId,
            CountryName = city.Region?.Country?.Name,
            Latitude = city.Latitude,
            Longitude = city.Longitude,
            PostalCode = city.PostalCode,
            Population = city.Population,
            Timezone = city.Timezone,
            IsActive = city.IsActive,
            IsCapital = city.IsCapital,
            SortOrder = city.SortOrder,
            NeighborhoodsCount = city.Neighborhoods.Count,
            Region = city.Region == null ? null : MapToRegionResponse(city.Region)
        };
    }

    // ══════════════════════════════════════════════════════════════════
    // Neighborhoods
    // ══════════════════════════════════════════════════════════════════

    public async Task<List<NeighborhoodResponseDto>> GetNeighborhoodsByCityAsync(
        Guid cityId,
        bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = _context.Set<Neighborhood>()
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(c => c!.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.CityId == cityId);

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => MapToNeighborhoodResponse(x))
            .ToListAsync(ct);
    }

    public async Task<NeighborhoodDetailDto?> GetNeighborhoodByIdAsync(Guid id, CancellationToken ct = default)
    {
        var neighborhood = await _context.Set<Neighborhood>()
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(c => c!.Region)
            .ThenInclude(r => r!.Country)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (neighborhood == null) return null;

        return new NeighborhoodDetailDto
        {
            Id = neighborhood.Id,
            Name = neighborhood.Name,
            NameEn = neighborhood.NameEn,
            Code = neighborhood.Code,
            CityId = neighborhood.CityId,
            CityName = neighborhood.City?.Name,
            RegionId = neighborhood.City?.RegionId,
            RegionName = neighborhood.City?.Region?.Name,
            CountryId = neighborhood.City?.Region?.CountryId,
            CountryName = neighborhood.City?.Region?.Country?.Name,
            Latitude = neighborhood.Latitude,
            Longitude = neighborhood.Longitude,
            PostalCode = neighborhood.PostalCode,
            Boundaries = neighborhood.Boundaries,
            IsActive = neighborhood.IsActive,
            SortOrder = neighborhood.SortOrder,
            City = neighborhood.City == null ? null : MapToCityResponse(neighborhood.City)
        };
    }

    // ══════════════════════════════════════════════════════════════════
    // Hierarchy & Search
    // ══════════════════════════════════════════════════════════════════

    public async Task<LocationHierarchyDto?> GetLocationHierarchyAsync(
        Guid? neighborhoodId = null,
        Guid? cityId = null,
        Guid? regionId = null,
        Guid? countryId = null,
        CancellationToken ct = default)
    {
        var result = new LocationHierarchyDto();

        if (neighborhoodId.HasValue)
        {
            var neighborhood = await GetNeighborhoodByIdAsync(neighborhoodId.Value, ct);
            if (neighborhood != null)
            {
                result.Neighborhood = neighborhood;
                cityId = neighborhood.CityId;
            }
        }

        if (cityId.HasValue)
        {
            var city = await GetCityByIdAsync(cityId.Value, ct);
            if (city != null)
            {
                result.City = city;
                regionId = city.RegionId;
            }
        }

        if (regionId.HasValue)
        {
            var region = await GetRegionByIdAsync(regionId.Value, ct);
            if (region != null)
            {
                result.Region = region;
                countryId = region.CountryId;
            }
        }

        if (countryId.HasValue)
        {
            var country = await GetCountryByIdAsync(countryId.Value, ct);
            if (country != null)
            {
                result.Country = country;
            }
        }

        return result.Country == null ? null : result;
    }

    public async Task<List<LocationSearchResult>> SearchLocationsAsync(
        string query,
        Guid? countryId = null,
        int limit = 20,
        CancellationToken ct = default)
    {
        var results = new List<LocationSearchResult>();
        var searchTerm = query.ToLower();

        // Search Neighborhoods
        var neighborhoodsQuery = _context.Set<Neighborhood>()
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(c => c!.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.IsActive &&
                (x.Name.ToLower().Contains(searchTerm) ||
                 (x.NameEn != null && x.NameEn.ToLower().Contains(searchTerm))));

        if (countryId.HasValue)
            neighborhoodsQuery = neighborhoodsQuery.Where(x => x.City!.Region!.CountryId == countryId);

        var neighborhoods = await neighborhoodsQuery.Take(limit / 4).ToListAsync(ct);

        results.AddRange(neighborhoods.Select(n => new LocationSearchResult
        {
            Id = n.Id,
            Name = n.Name,
            NameEn = n.NameEn,
            Level = LocationLevel.Neighborhood,
            ParentName = n.City?.Name,
            FullPath = $"{n.Name}, {n.City?.Name}, {n.City?.Region?.Name}"
        }));

        // Search Cities
        var citiesQuery = _context.Set<City>()
            .AsNoTracking()
            .Include(x => x.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.IsActive &&
                (x.Name.ToLower().Contains(searchTerm) ||
                 (x.NameEn != null && x.NameEn.ToLower().Contains(searchTerm))));

        if (countryId.HasValue)
            citiesQuery = citiesQuery.Where(x => x.Region!.CountryId == countryId);

        var cities = await citiesQuery.Take(limit / 4).ToListAsync(ct);

        results.AddRange(cities.Select(c => new LocationSearchResult
        {
            Id = c.Id,
            Name = c.Name,
            NameEn = c.NameEn,
            Level = LocationLevel.City,
            ParentName = c.Region?.Name,
            FullPath = $"{c.Name}, {c.Region?.Name}, {c.Region?.Country?.Name}"
        }));

        return results.Take(limit).ToList();
    }

    // ══════════════════════════════════════════════════════════════════
    // Mapping Helpers
    // ══════════════════════════════════════════════════════════════════

    private static CountryResponseDto MapToCountryResponse(Country x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        NameEn = x.NameEn,
        Code = x.Code,
        Code3 = x.Code3,
        PhoneCode = x.PhoneCode,
        CurrencyCode = x.CurrencyCode,
        CurrencySymbol = x.CurrencySymbol,
        Flag = x.Flag,
        Latitude = x.Latitude,
        Longitude = x.Longitude,
        IsActive = x.IsActive,
        SortOrder = x.SortOrder
    };

    private static RegionResponseDto MapToRegionResponse(Region x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        NameEn = x.NameEn,
        Code = x.Code,
        Type = x.Type,
        CountryId = x.CountryId,
        CountryName = x.Country?.Name,
        Latitude = x.Latitude,
        Longitude = x.Longitude,
        IsActive = x.IsActive,
        SortOrder = x.SortOrder
    };

    private static CityResponseDto MapToCityResponse(City x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        NameEn = x.NameEn,
        Code = x.Code,
        RegionId = x.RegionId,
        RegionName = x.Region?.Name,
        CountryId = x.Region?.CountryId,
        CountryName = x.Region?.Country?.Name,
        Latitude = x.Latitude,
        Longitude = x.Longitude,
        IsActive = x.IsActive,
        IsCapital = x.IsCapital,
        SortOrder = x.SortOrder
    };

    private static NeighborhoodResponseDto MapToNeighborhoodResponse(Neighborhood x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        NameEn = x.NameEn,
        Code = x.Code,
        CityId = x.CityId,
        CityName = x.City?.Name,
        RegionId = x.City?.RegionId,
        RegionName = x.City?.Region?.Name,
        CountryId = x.City?.Region?.CountryId,
        CountryName = x.City?.Region?.Country?.Name,
        Latitude = x.Latitude,
        Longitude = x.Longitude,
        PostalCode = x.PostalCode,
        IsActive = x.IsActive,
        SortOrder = x.SortOrder
    };
}
