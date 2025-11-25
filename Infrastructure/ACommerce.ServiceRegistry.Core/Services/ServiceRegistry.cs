using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace ACommerce.ServiceRegistry.Core.Services;

/// <summary>
/// تنفيذ تسجيل وإدارة الخدمات
/// </summary>
public sealed class ServiceRegistry : IServiceRegistry
{
	private readonly IServiceStore _store;
	private readonly ILogger<ServiceRegistry> _logger;

	public ServiceRegistry(IServiceStore store, ILogger<ServiceRegistry> logger)
	{
		_store = store;
		_logger = logger;
	}

	public async Task<ServiceEndpoint> RegisterAsync(ServiceRegistration registration, CancellationToken cancellationToken = default)
	{
		// تحقق من البيانات المطلوبة
		if (string.IsNullOrWhiteSpace(registration.ServiceName))
			throw new ArgumentException("Service name is required", nameof(registration));

		if (string.IsNullOrWhiteSpace(registration.BaseUrl))
			throw new ArgumentException("Base URL is required", nameof(registration));

		// إنشاء Endpoint جديد
		var endpoint = new ServiceEndpoint
		{
			Id = Guid.NewGuid().ToString(),
			ServiceName = registration.ServiceName,
			Version = registration.Version,
			BaseUrl = registration.BaseUrl.TrimEnd('/'),
			Environment = registration.Environment,
			Weight = registration.Weight,
			Tags = registration.Tags ?? new Dictionary<string, string>(),
			RegisteredAt = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow,
			Health = new ServiceHealth
			{
				Status = HealthStatus.Unknown,
				LastChecked = DateTime.UtcNow
			}
		};

		// إضافة معلومات Health Check إلى Tags
		if (registration.EnableHealthCheck)
		{
			endpoint.Tags["HealthCheckEnabled"] = "true";
			endpoint.Tags["HealthCheckPath"] = registration.HealthCheckPath;
			endpoint.Tags["HealthCheckInterval"] = registration.HealthCheckIntervalSeconds.ToString();
		}

		await _store.SaveAsync(endpoint, cancellationToken);

		_logger.LogInformation(
			"✅ Service registered: {ServiceName} v{Version} at {BaseUrl} (ID: {ServiceId})",
			endpoint.ServiceName, endpoint.Version, endpoint.BaseUrl, endpoint.Id);

		return endpoint;
	}

	public async Task<bool> DeregisterAsync(string serviceId, CancellationToken cancellationToken = default)
	{
		var service = await _store.GetByIdAsync(serviceId, cancellationToken);
		if (service == null)
			return false;

		await _store.DeleteAsync(serviceId, cancellationToken);

		_logger.LogInformation(
			"❌ Service deregistered: {ServiceName} v{Version} (ID: {ServiceId})",
			service.ServiceName, service.Version, serviceId);

		return true;
	}

	public async Task UpdateHealthAsync(string serviceId, ServiceHealth health, CancellationToken cancellationToken = default)
	{
		var service = await _store.GetByIdAsync(serviceId, cancellationToken);
		if (service == null)
			return;

		service.Health = health;
		service.LastUpdated = DateTime.UtcNow;

		await _store.SaveAsync(service, cancellationToken);

		_logger.LogDebug(
			"Health updated for {ServiceName}: {Status} ({ResponseTime}ms)",
			service.ServiceName, health.Status, health.ResponseTimeMs);
	}

	public Task<ServiceEndpoint?> GetByIdAsync(string serviceId, CancellationToken cancellationToken = default)
	{
		return _store.GetByIdAsync(serviceId, cancellationToken);
	}

	public Task<IEnumerable<ServiceEndpoint>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return _store.GetAllAsync(cancellationToken);
	}

	public async Task HeartbeatAsync(string serviceId, CancellationToken cancellationToken = default)
	{
		var service = await _store.GetByIdAsync(serviceId, cancellationToken);
		if (service == null)
			return;

		service.LastUpdated = DateTime.UtcNow;
		await _store.SaveAsync(service, cancellationToken);

		_logger.LogTrace("💓 Heartbeat received from {ServiceName} (ID: {ServiceId})",
			service.ServiceName, serviceId);
	}
}
