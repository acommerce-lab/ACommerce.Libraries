using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Ashare.Shared.Models;

namespace Ashare.Shared.Services;

public interface IListingsCacheService
{
    Task<List<SpaceItem>> GetFeaturedAsync(Func<Task<List<SpaceItem>>> fetchFunc);
    Task<List<SpaceItem>> GetNewSpacesAsync(Func<Task<List<SpaceItem>>> fetchFunc);
    Task<List<SpaceItem>> GetAllSpacesAsync(Func<Task<List<SpaceItem>>> fetchFunc);
    Task<List<SpaceCategory>> GetCategoriesAsync(Func<Task<List<SpaceCategory>>> fetchFunc);
    Task<SpaceItem?> GetSpaceByIdAsync(Guid id, Func<Task<SpaceItem?>> fetchFunc);
    Task<List<SpaceItem>> GetSpacesByCategoryAsync(Guid categoryId, Func<Task<List<SpaceItem>>> fetchFunc);
    Task<int> GetBookingsCountAsync(Func<Task<int>> fetchFunc);
    
    void InvalidateListings();
    void InvalidateBookingsCount();
    void InvalidateListing(Guid listingId);
    void InvalidateCategories();
    void InvalidateAll();
    
    bool IsWarmedUp { get; }
    DateTime LastWarmupTime { get; }
}

public class ListingsCacheService : IListingsCacheService
{
    private readonly ILogger<ListingsCacheService> _logger;
    
    public bool IsWarmedUp => true;
    public DateTime LastWarmupTime => DateTime.UtcNow;

    public ListingsCacheService(IMemoryCache cache, ILogger<ListingsCacheService> logger)
    {
        _logger = logger;
    }

    public async Task<List<SpaceItem>> GetFeaturedAsync(Func<Task<List<SpaceItem>>> fetchFunc)
    {
        _logger.LogInformation("Fetching featured listings from API (no cache)...");
        return await fetchFunc();
    }

    public async Task<List<SpaceItem>> GetNewSpacesAsync(Func<Task<List<SpaceItem>>> fetchFunc)
    {
        _logger.LogInformation("Fetching new listings from API (no cache)...");
        return await fetchFunc();
    }

    public async Task<List<SpaceItem>> GetAllSpacesAsync(Func<Task<List<SpaceItem>>> fetchFunc)
    {
        _logger.LogInformation("Fetching all listings from API (no cache)...");
        return await fetchFunc();
    }

    public async Task<List<SpaceCategory>> GetCategoriesAsync(Func<Task<List<SpaceCategory>>> fetchFunc)
    {
        _logger.LogInformation("Fetching categories from API (no cache)...");
        return await fetchFunc();
    }

    public async Task<SpaceItem?> GetSpaceByIdAsync(Guid id, Func<Task<SpaceItem?>> fetchFunc)
    {
        _logger.LogDebug("Fetching listing {Id} from API (no cache)...", id);
        return await fetchFunc();
    }

    public async Task<List<SpaceItem>> GetSpacesByCategoryAsync(Guid categoryId, Func<Task<List<SpaceItem>>> fetchFunc)
    {
        _logger.LogDebug("Fetching listings for category {CategoryId} from API (no cache)...", categoryId);
        return await fetchFunc();
    }

    public async Task<int> GetBookingsCountAsync(Func<Task<int>> fetchFunc)
    {
        _logger.LogInformation("Fetching bookings count from API (no cache)...");
        return await fetchFunc();
    }

    public void InvalidateBookingsCount() { }
    public void InvalidateListings() { }
    public void InvalidateListing(Guid listingId) { }
    public void InvalidateCategories() { }
    public void InvalidateAll() { }
}
