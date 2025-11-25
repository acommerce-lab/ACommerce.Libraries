using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;

namespace ACommerce.ServiceRegistry.Server.Services;

/// <summary>
/// Background Service لفحص صحة الخدمات بشكل دوري
/// </summary>
public class HealthCheckBackgroundService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<HealthCheckBackgroundService> _logger;
	private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);
	private readonly TimeSpan _staleThreshold = TimeSpan.FromMinutes(5);

	public HealthCheckBackgroundService(
		IServiceProvider serviceProvider,
		ILogger<HealthCheckBackgroundService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("✅ Health Check Background Service started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await PerformHealthChecksAsync(stoppingToken);
				await CleanupStaleServicesAsync(stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in Health Check Background Service");
			}

			await Task.Delay(_checkInterval, stoppingToken);
		}

		_logger.LogInformation("❌ Health Check Background Service stopped");
	}

	private async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceProvider.CreateScope();
		var registry = scope.ServiceProvider.GetRequiredService<IServiceRegistry>();
		var healthChecker = scope.ServiceProvider.GetRequiredService<IHealthChecker>();

		var services = await registry.GetAllAsync(cancellationToken);
		var servicesToCheck = services
			.Where(s => s.Tags.GetValueOrDefault("HealthCheckEnabled") == "true")
			.ToList();

		if (servicesToCheck.Count == 0)
			return;

		_logger.LogDebug("Checking health of {Count} services", servicesToCheck.Count);

		var healthResults = await healthChecker.CheckMultipleAsync(servicesToCheck, cancellationToken);

		foreach (var (serviceId, health) in healthResults)
		{
			await registry.UpdateHealthAsync(serviceId, health, cancellationToken);

			// إذا فشلت 3 مرات متتالية، اعتبرها Unhealthy
			if (health.ConsecutiveFailures >= 3)
			{
				health.Status = HealthStatus.Unhealthy;
				_logger.LogWarning("Service {ServiceId} marked as Unhealthy after {Failures} consecutive failures",
					serviceId, health.ConsecutiveFailures);
			}
		}
	}

	private async Task CleanupStaleServicesAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceProvider.CreateScope();
		var store = scope.ServiceProvider.GetRequiredService<IServiceStore>();

		await store.CleanupStaleServicesAsync(_staleThreshold, cancellationToken);
	}
}
