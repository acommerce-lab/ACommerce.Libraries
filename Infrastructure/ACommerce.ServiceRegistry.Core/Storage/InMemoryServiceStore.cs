using System.Collections.Concurrent;
using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;

namespace ACommerce.ServiceRegistry.Core.Storage;

/// <summary>
/// تخزين الخدمات في الذاكرة (InMemory)
/// مناسب للـ Development والتطبيقات الصغيرة
/// </summary>
public sealed class InMemoryServiceStore : IServiceStore
{
	private readonly ConcurrentDictionary<string, ServiceEndpoint> _services = new();

	public Task SaveAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken = default)
	{
		endpoint.LastUpdated = DateTime.UtcNow;
		_services.AddOrUpdate(endpoint.Id, endpoint, (_, _) => endpoint);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(string serviceId, CancellationToken cancellationToken = default)
	{
		_services.TryRemove(serviceId, out _);
		return Task.CompletedTask;
	}

	public Task<ServiceEndpoint?> GetByIdAsync(string serviceId, CancellationToken cancellationToken = default)
	{
		_services.TryGetValue(serviceId, out var service);
		return Task.FromResult(service);
	}

	public Task<IEnumerable<ServiceEndpoint>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult<IEnumerable<ServiceEndpoint>>(_services.Values.ToList());
	}

	public Task<IEnumerable<ServiceEndpoint>> FindByNameAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		var services = _services.Values
			.Where(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
			.ToList();

		return Task.FromResult<IEnumerable<ServiceEndpoint>>(services);
	}

	public Task CleanupStaleServicesAsync(TimeSpan threshold, CancellationToken cancellationToken = default)
	{
		var cutoff = DateTime.UtcNow - threshold;
		var staleServices = _services.Values
			.Where(s => s.LastUpdated < cutoff)
			.Select(s => s.Id)
			.ToList();

		foreach (var id in staleServices)
		{
			_services.TryRemove(id, out _);
		}

		return Task.CompletedTask;
	}
}
