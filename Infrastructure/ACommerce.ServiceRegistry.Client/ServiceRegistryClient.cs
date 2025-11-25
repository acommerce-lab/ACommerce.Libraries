using System.Net.Http.Json;
using ACommerce.ServiceRegistry.Abstractions.Models;
using ACommerce.ServiceRegistry.Client.Cache;
using Microsoft.Extensions.Logging;

namespace ACommerce.ServiceRegistry.Client;

/// <summary>
/// Client للتواصل مع Service Registry
/// يستخدم من قبل التطبيقات لاكتشاف الخدمات
/// </summary>
public sealed class ServiceRegistryClient
{
	private readonly HttpClient _httpClient;
	private readonly ServiceCache _cache;
	private readonly ILogger<ServiceRegistryClient> _logger;

	public ServiceRegistryClient(
		HttpClient httpClient,
		ServiceCache cache,
		ILogger<ServiceRegistryClient> logger)
	{
		_httpClient = httpClient;
		_cache = cache;
		_logger = logger;
	}

	/// <summary>
	/// تسجيل خدمة في Registry
	/// </summary>
	public async Task<ServiceEndpoint?> RegisterAsync(ServiceRegistration registration, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync("/api/registry/register", registration, cancellationToken);
			response.EnsureSuccessStatusCode();

			var endpoint = await response.Content.ReadFromJsonAsync<ServiceEndpoint>(cancellationToken);
			_logger.LogInformation("✅ Service registered: {ServiceName} (ID: {ServiceId})",
				registration.ServiceName, endpoint?.Id);

			return endpoint;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to register service: {ServiceName}", registration.ServiceName);
			return null;
		}
	}

	/// <summary>
	/// إلغاء تسجيل خدمة
	/// </summary>
	public async Task<bool> DeregisterAsync(string serviceId, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _httpClient.DeleteAsync($"/api/registry/{serviceId}", cancellationToken);
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to deregister service: {ServiceId}", serviceId);
			return false;
		}
	}

	/// <summary>
	/// اكتشاف خدمة (مع Cache)
	/// </summary>
	public async Task<ServiceEndpoint?> DiscoverAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		// تحقق من Cache أولاً
		var cached = _cache.Get(serviceName);
		if (cached != null)
		{
			_logger.LogDebug("Cache hit for service: {ServiceName}", serviceName);
			return cached;
		}

		// اطلب من Registry
		try
		{
			var response = await _httpClient.GetAsync($"/api/discovery/{serviceName}", cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("Service not found: {ServiceName}", serviceName);
				return null;
			}

			var endpoint = await response.Content.ReadFromJsonAsync<ServiceEndpoint>(cancellationToken);

			if (endpoint != null)
			{
				_cache.Set(serviceName, endpoint);
				_logger.LogDebug("Service discovered: {ServiceName} at {BaseUrl}", serviceName, endpoint.BaseUrl);
			}

			return endpoint;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to discover service: {ServiceName}", serviceName);

			// إذا فشل الطلب، حاول استخدام Cache القديم
			var stale = _cache.GetStale(serviceName);
			if (stale != null)
			{
				_logger.LogWarning("Using stale cache for service: {ServiceName}", serviceName);
				return stale;
			}

			return null;
		}
	}

	/// <summary>
	/// اكتشاف خدمة بناءً على استعلام متقدم
	/// </summary>
	public async Task<ServiceEndpoint?> DiscoverAsync(ServiceQuery query, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync("/api/discovery/discover", query, cancellationToken);

			if (!response.IsSuccessStatusCode)
				return null;

			var endpoint = await response.Content.ReadFromJsonAsync<ServiceEndpoint>(cancellationToken);

			if (endpoint != null)
			{
				_cache.Set(query.ServiceName, endpoint);
			}

			return endpoint;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to discover service with query: {ServiceName}", query.ServiceName);
			return null;
		}
	}

	/// <summary>
	/// الحصول على جميع نسخ خدمة
	/// </summary>
	public async Task<IEnumerable<ServiceEndpoint>> GetAllInstancesAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _httpClient.GetAsync($"/api/discovery/{serviceName}/instances", cancellationToken);

			if (!response.IsSuccessStatusCode)
				return Enumerable.Empty<ServiceEndpoint>();

			var endpoints = await response.Content.ReadFromJsonAsync<IEnumerable<ServiceEndpoint>>(cancellationToken);
			return endpoints ?? Enumerable.Empty<ServiceEndpoint>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get all instances: {ServiceName}", serviceName);
			return Enumerable.Empty<ServiceEndpoint>();
		}
	}

	/// <summary>
	/// إرسال Heartbeat للخدمة
	/// </summary>
	public async Task<bool> SendHeartbeatAsync(string serviceId, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _httpClient.PostAsync($"/api/registry/{serviceId}/heartbeat", null, cancellationToken);
			return response.IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// مسح Cache
	/// </summary>
	public void ClearCache()
	{
		_cache.Clear();
		_logger.LogInformation("Cache cleared");
	}
}
