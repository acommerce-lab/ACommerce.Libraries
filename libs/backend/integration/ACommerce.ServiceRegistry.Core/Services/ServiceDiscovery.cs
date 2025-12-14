using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace ACommerce.ServiceRegistry.Core.Services;

/// <summary>
/// تنفيذ اكتشاف الخدمات (Service Discovery)
/// </summary>
public sealed class ServiceDiscovery : IServiceDiscovery
{
	private readonly IServiceStore _store;
	private readonly ILogger<ServiceDiscovery> _logger;
	private readonly Random _random = new();

	public ServiceDiscovery(IServiceStore store, ILogger<ServiceDiscovery> logger)
	{
		_store = store;
		_logger = logger;
	}

	public async Task<ServiceEndpoint?> DiscoverAsync(ServiceQuery query, CancellationToken cancellationToken = default)
	{
		var services = await DiscoverAllAsync(query, cancellationToken);
		var list = services.ToList();

		if (list.Count == 0)
		{
			_logger.LogWarning("No services found for: {ServiceName}", query.ServiceName);
			return null;
		}

		// إذا كان هناك خدمة واحدة فقط
		if (list.Count == 1)
			return list[0];

		// Load Balancing: اختيار عشوائي بناءً على الوزن (Weighted Random)
		var selected = SelectByWeight(list);

		_logger.LogDebug("Selected service: {ServiceName} at {BaseUrl}",
			selected.ServiceName, selected.BaseUrl);

		return selected;
	}

	public async Task<IEnumerable<ServiceEndpoint>> DiscoverAllAsync(ServiceQuery query, CancellationToken cancellationToken = default)
	{
		var services = await _store.FindByNameAsync(query.ServiceName, cancellationToken);

		// تطبيق الفلاتر
		var filtered = services.AsEnumerable();

		// فلتر: Version محدد؟
		if (!string.IsNullOrWhiteSpace(query.Version))
		{
			filtered = filtered.Where(s => s.Version.Equals(query.Version, StringComparison.OrdinalIgnoreCase));
		}

		// فلتر: Environment محدد؟
		if (!string.IsNullOrWhiteSpace(query.Environment))
		{
			filtered = filtered.Where(s => s.Environment.Equals(query.Environment, StringComparison.OrdinalIgnoreCase));
		}

		// فلتر: فقط Healthy؟
		if (query.OnlyHealthy)
		{
			filtered = filtered.Where(s => s.Health.Status == HealthStatus.Healthy);
		}

		// فلتر: Tags محددة؟
		if (query.Tags != null && query.Tags.Any())
		{
			filtered = filtered.Where(s => MatchesTags(s, query.Tags));
		}

		return filtered.ToList();
	}

	public async Task<ServiceEndpoint?> GetServiceAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		// لا نفلتر بالصحة هنا - نريد أي خدمة متاحة
		return await DiscoverAsync(new ServiceQuery { ServiceName = serviceName, OnlyHealthy = false }, cancellationToken);
	}

	public async Task<IEnumerable<ServiceEndpoint>> GetAllInstancesAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		return await DiscoverAllAsync(new ServiceQuery { ServiceName = serviceName, OnlyHealthy = false }, cancellationToken);
	}

	// ===== Helper Methods =====

	/// <summary>
	/// اختيار خدمة بناءً على الوزن (Weighted Random Selection)
	/// </summary>
	private ServiceEndpoint SelectByWeight(List<ServiceEndpoint> services)
	{
		var totalWeight = services.Sum(s => s.Weight);
		var randomValue = _random.Next(1, totalWeight + 1);

		var cumulativeWeight = 0;
		foreach (var service in services)
		{
			cumulativeWeight += service.Weight;
			if (randomValue <= cumulativeWeight)
				return service;
		}

		return services[0]; // Fallback
	}

	/// <summary>
	/// التحقق من تطابق Tags
	/// </summary>
	private static bool MatchesTags(ServiceEndpoint service, Dictionary<string, string> requiredTags)
	{
		return requiredTags.All(tag =>
			service.Tags.TryGetValue(tag.Key, out var value) &&
			value.Equals(tag.Value, StringComparison.OrdinalIgnoreCase));
	}
}
