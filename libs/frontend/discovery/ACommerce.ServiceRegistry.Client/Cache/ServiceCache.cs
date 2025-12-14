using ACommerce.ServiceRegistry.Abstractions.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ACommerce.ServiceRegistry.Client.Cache;

/// <summary>
/// Cache محلي لمعلومات الخدمات
/// يقلل الضغط على Registry ويوفر Fallback عند انقطاع الاتصال
/// </summary>
public sealed class ServiceCache
{
	private readonly IMemoryCache _cache;
	private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);
	private readonly TimeSpan _staleExpiration = TimeSpan.FromHours(1);

	public ServiceCache(IMemoryCache cache)
	{
		_cache = cache;
	}

	/// <summary>
	/// حفظ في Cache
	/// </summary>
	public void Set(string serviceName, ServiceEndpoint endpoint)
	{
		var cacheKey = GetCacheKey(serviceName);
		var staleCacheKey = GetStaleCacheKey(serviceName);

		// Cache عادي (5 دقائق)
		_cache.Set(cacheKey, endpoint, _defaultExpiration);

		// Stale cache (1 ساعة) - للاستخدام عند فشل Registry
		_cache.Set(staleCacheKey, endpoint, _staleExpiration);
	}

	/// <summary>
	/// الحصول من Cache
	/// </summary>
	public ServiceEndpoint? Get(string serviceName)
	{
		var cacheKey = GetCacheKey(serviceName);
		return _cache.Get<ServiceEndpoint>(cacheKey);
	}

	/// <summary>
	/// الحصول من Stale Cache (في حالة فشل Registry)
	/// </summary>
	public ServiceEndpoint? GetStale(string serviceName)
	{
		var staleCacheKey = GetStaleCacheKey(serviceName);
		return _cache.Get<ServiceEndpoint>(staleCacheKey);
	}

	/// <summary>
	/// مسح Cache
	/// </summary>
	public void Clear()
	{
		if (_cache is MemoryCache memoryCache)
		{
			memoryCache.Compact(1.0);
		}
	}

	/// <summary>
	/// حذف من Cache
	/// </summary>
	public void Remove(string serviceName)
	{
		var cacheKey = GetCacheKey(serviceName);
		var staleCacheKey = GetStaleCacheKey(serviceName);

		_cache.Remove(cacheKey);
		_cache.Remove(staleCacheKey);
	}

	private static string GetCacheKey(string serviceName) => $"service:{serviceName}";
	private static string GetStaleCacheKey(string serviceName) => $"service:stale:{serviceName}";
}
