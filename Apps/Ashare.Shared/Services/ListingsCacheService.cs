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
    
    void InvalidateListings();
    void InvalidateListing(Guid listingId);
    void InvalidateCategories();
    void InvalidateAll();
    
    bool IsWarmedUp { get; }
    DateTime LastWarmupTime { get; }
}

public class ListingsCacheService : IListingsCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ListingsCacheService> _logger;
    private readonly SemaphoreSlim _featuredLock = new(1, 1);
    private readonly SemaphoreSlim _newLock = new(1, 1);
    private readonly SemaphoreSlim _allLock = new(1, 1);
    private readonly SemaphoreSlim _categoriesLock = new(1, 1);
    
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SingleItemCacheDuration = TimeSpan.FromMinutes(5);
    
    private const string FeaturedKey = "cache_featured_listings";
    private const string NewKey = "cache_new_listings";
    private const string AllKey = "cache_all_listings";
    private const string CategoriesKey = "cache_categories";
    private const string ListingByIdPrefix = "cache_listing_";
    private const string ListingsByCategoryPrefix = "cache_listings_category_";
    
    private readonly HashSet<string> _categoryKeys = new();
    private readonly HashSet<string> _listingKeys = new();
    private readonly object _keysLock = new();
    
    public bool IsWarmedUp { get; private set; }
    public DateTime LastWarmupTime { get; private set; }

    public ListingsCacheService(IMemoryCache cache, ILogger<ListingsCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<SpaceItem>> GetFeaturedAsync(Func<Task<List<SpaceItem>>> fetchFunc)
    {
        if (_cache.TryGetValue(FeaturedKey, out List<SpaceItem>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit: Featured listings");
            return cached;
        }

        await _featuredLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(FeaturedKey, out cached) && cached != null)
                return cached;

            _logger.LogInformation("Cache miss: Fetching featured listings from API...");
            var items = await fetchFunc();
            _cache.Set(FeaturedKey, items, DefaultCacheDuration);
            
            UpdateWarmupStatus();
            return items;
        }
        finally
        {
            _featuredLock.Release();
        }
    }

    public async Task<List<SpaceItem>> GetNewSpacesAsync(Func<Task<List<SpaceItem>>> fetchFunc)
    {
        if (_cache.TryGetValue(NewKey, out List<SpaceItem>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit: New listings");
            return cached;
        }

        await _newLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(NewKey, out cached) && cached != null)
                return cached;

            _logger.LogInformation("Cache miss: Fetching new listings from API...");
            var items = await fetchFunc();
            _cache.Set(NewKey, items, DefaultCacheDuration);
            
            UpdateWarmupStatus();
            return items;
        }
        finally
        {
            _newLock.Release();
        }
    }

    public async Task<List<SpaceItem>> GetAllSpacesAsync(Func<Task<List<SpaceItem>>> fetchFunc)
    {
        if (_cache.TryGetValue(AllKey, out List<SpaceItem>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit: All listings");
            return cached;
        }

        await _allLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(AllKey, out cached) && cached != null)
                return cached;

            _logger.LogInformation("Cache miss: Fetching all listings from API...");
            var items = await fetchFunc();
            _cache.Set(AllKey, items, DefaultCacheDuration);
            
            UpdateWarmupStatus();
            return items;
        }
        finally
        {
            _allLock.Release();
        }
    }

    public async Task<List<SpaceCategory>> GetCategoriesAsync(Func<Task<List<SpaceCategory>>> fetchFunc)
    {
        if (_cache.TryGetValue(CategoriesKey, out List<SpaceCategory>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit: Categories");
            return cached;
        }

        await _categoriesLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(CategoriesKey, out cached) && cached != null)
                return cached;

            _logger.LogInformation("Cache miss: Fetching categories from API...");
            var items = await fetchFunc();
            _cache.Set(CategoriesKey, items, DefaultCacheDuration);
            return items;
        }
        finally
        {
            _categoriesLock.Release();
        }
    }

    public async Task<SpaceItem?> GetSpaceByIdAsync(Guid id, Func<Task<SpaceItem?>> fetchFunc)
    {
        var key = $"{ListingByIdPrefix}{id}";
        
        if (_cache.TryGetValue(key, out SpaceItem? cached))
        {
            _logger.LogDebug("Cache hit: Listing {Id}", id);
            return cached;
        }

        _logger.LogDebug("Cache miss: Fetching listing {Id} from API...", id);
        var item = await fetchFunc();
        
        if (item != null)
        {
            _cache.Set(key, item, SingleItemCacheDuration);
            lock (_keysLock) { _listingKeys.Add(key); }
        }
        
        return item;
    }

    public async Task<List<SpaceItem>> GetSpacesByCategoryAsync(Guid categoryId, Func<Task<List<SpaceItem>>> fetchFunc)
    {
        var key = $"{ListingsByCategoryPrefix}{categoryId}";
        
        if (_cache.TryGetValue(key, out List<SpaceItem>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit: Listings for category {CategoryId}", categoryId);
            return cached;
        }

        _logger.LogDebug("Cache miss: Fetching listings for category {CategoryId} from API...", categoryId);
        var items = await fetchFunc();
        _cache.Set(key, items, SingleItemCacheDuration);
        lock (_keysLock) { _categoryKeys.Add(key); }
        
        return items;
    }

    public void InvalidateListings()
    {
        _logger.LogInformation("Invalidating all listings cache");
        _cache.Remove(FeaturedKey);
        _cache.Remove(NewKey);
        _cache.Remove(AllKey);
        
        lock (_keysLock)
        {
            foreach (var key in _categoryKeys)
                _cache.Remove(key);
            foreach (var key in _listingKeys)
                _cache.Remove(key);
            
            _categoryKeys.Clear();
            _listingKeys.Clear();
        }
        
        IsWarmedUp = false;
    }

    public void InvalidateListing(Guid listingId)
    {
        _logger.LogInformation("Invalidating listing {ListingId} and related caches", listingId);
        _cache.Remove($"{ListingByIdPrefix}{listingId}");
        InvalidateListings();
    }

    public void InvalidateCategories()
    {
        _logger.LogInformation("Invalidating categories cache");
        _cache.Remove(CategoriesKey);
    }

    public void InvalidateAll()
    {
        _logger.LogInformation("Invalidating all caches");
        InvalidateListings();
        InvalidateCategories();
    }

    private void UpdateWarmupStatus()
    {
        var hasFeatured = _cache.TryGetValue(FeaturedKey, out _);
        var hasNew = _cache.TryGetValue(NewKey, out _);
        var hasAll = _cache.TryGetValue(AllKey, out _);
        
        if (hasFeatured && hasNew && hasAll && !IsWarmedUp)
        {
            IsWarmedUp = true;
            LastWarmupTime = DateTime.UtcNow;
            _logger.LogInformation("Cache fully warmed up at {Time}", LastWarmupTime);
        }
    }
}
