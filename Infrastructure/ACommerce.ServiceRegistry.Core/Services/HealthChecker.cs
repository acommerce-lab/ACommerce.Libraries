using System.Diagnostics;
using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace ACommerce.ServiceRegistry.Core.Services;

/// <summary>
/// تنفيذ فحص صحة الخدمات (Health Checker)
/// </summary>
public sealed class HealthChecker : IHealthChecker
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<HealthChecker> _logger;

	public HealthChecker(IHttpClientFactory httpClientFactory, ILogger<HealthChecker> logger)
	{
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	public async Task<ServiceHealth> CheckHealthAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken = default)
	{
		var health = new ServiceHealth
		{
			LastChecked = DateTime.UtcNow
		};

		try
		{
			// الحصول على مسار Health Check
			var healthCheckPath = endpoint.Tags.GetValueOrDefault("HealthCheckPath", "/health");
			var healthCheckUrl = $"{endpoint.BaseUrl}{healthCheckPath}";

			var stopwatch = Stopwatch.StartNew();
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(5);

			var response = await client.GetAsync(healthCheckUrl, cancellationToken);
			stopwatch.Stop();

			health.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

			if (response.IsSuccessStatusCode)
			{
				health.Status = HealthStatus.Healthy;
				health.ConsecutiveFailures = 0;
				health.Message = "OK";
			}
			else
			{
				health.Status = HealthStatus.Degraded;
				health.ConsecutiveFailures = endpoint.Health.ConsecutiveFailures + 1;
				health.Message = $"HTTP {(int)response.StatusCode}";
			}

			_logger.LogDebug("Health check: {ServiceName} at {Url} - {Status} ({ResponseTime}ms)",
				endpoint.ServiceName, healthCheckUrl, health.Status, health.ResponseTimeMs);
		}
		catch (TaskCanceledException)
		{
			health.Status = HealthStatus.Unhealthy;
			health.ConsecutiveFailures = endpoint.Health.ConsecutiveFailures + 1;
			health.Message = "Timeout";

			_logger.LogWarning("Health check timeout: {ServiceName} at {Url}",
				endpoint.ServiceName, endpoint.BaseUrl);
		}
		catch (Exception ex)
		{
			health.Status = HealthStatus.Unhealthy;
			health.ConsecutiveFailures = endpoint.Health.ConsecutiveFailures + 1;
			health.Message = ex.Message;

			_logger.LogError(ex, "Health check failed: {ServiceName} at {Url}",
				endpoint.ServiceName, endpoint.BaseUrl);
		}

		return health;
	}

	public async Task<Dictionary<string, ServiceHealth>> CheckMultipleAsync(IEnumerable<ServiceEndpoint> endpoints, CancellationToken cancellationToken = default)
	{
		var tasks = endpoints.Select(async endpoint =>
		{
			var health = await CheckHealthAsync(endpoint, cancellationToken);
			return new { endpoint.Id, Health = health };
		});

		var results = await Task.WhenAll(tasks);

		return results.ToDictionary(r => r.Id, r => r.Health);
	}
}
