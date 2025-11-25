using ACommerce.ServiceRegistry.Abstractions.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.ServiceRegistry.Client.Services;

/// <summary>
/// Background Service لتسجيل الخدمة تلقائياً عند بدء التطبيق وإرسال Heartbeats
/// </summary>
public class ServiceRegistrationHostedService : BackgroundService
{
	private readonly ServiceRegistryClient _client;
	private readonly ServiceRegistrationOptions _options;
	private readonly ILogger<ServiceRegistrationHostedService> _logger;
	private string? _serviceId;

	public ServiceRegistrationHostedService(
		ServiceRegistryClient client,
		IOptions<ServiceRegistrationOptions> options,
		ILogger<ServiceRegistrationHostedService> logger)
	{
		_client = client;
		_options = options.Value;
		_logger = logger;
	}

	public override async Task StartAsync(CancellationToken cancellationToken)
	{
		if (!_options.AutoRegister)
		{
			_logger.LogInformation("Auto-registration disabled");
			return;
		}

		// تسجيل الخدمة
		var registration = new ServiceRegistration
		{
			ServiceName = _options.ServiceName,
			Version = _options.Version,
			BaseUrl = _options.BaseUrl,
			Environment = _options.Environment,
			Weight = _options.Weight,
			Tags = _options.Tags,
			EnableHealthCheck = _options.EnableHealthCheck,
			HealthCheckPath = _options.HealthCheckPath,
			HealthCheckIntervalSeconds = _options.HealthCheckIntervalSeconds
		};

		var endpoint = await _client.RegisterAsync(registration, cancellationToken);
		if (endpoint != null)
		{
			_serviceId = endpoint.Id;
			_logger.LogInformation("✅ Service auto-registered with ID: {ServiceId}", _serviceId);
		}
		else
		{
			_logger.LogError("❌ Failed to auto-register service");
		}

		await base.StartAsync(cancellationToken);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!_options.AutoRegister || string.IsNullOrEmpty(_serviceId))
			return;

		// إرسال Heartbeats دورياً
		var interval = TimeSpan.FromSeconds(_options.HeartbeatIntervalSeconds);

		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(interval, stoppingToken);

			try
			{
				await _client.SendHeartbeatAsync(_serviceId, stoppingToken);
				_logger.LogTrace("💓 Heartbeat sent");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send heartbeat");
			}
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_options.AutoRegister && !string.IsNullOrEmpty(_serviceId))
		{
			// إلغاء التسجيل عند إيقاف التطبيق
			await _client.DeregisterAsync(_serviceId, cancellationToken);
			_logger.LogInformation("❌ Service deregistered");
		}

		await base.StopAsync(cancellationToken);
	}
}

/// <summary>
/// خيارات تسجيل الخدمة
/// </summary>
public class ServiceRegistrationOptions
{
	public bool AutoRegister { get; set; } = false;
	public string ServiceName { get; set; } = string.Empty;
	public string Version { get; set; } = "v1";
	public string BaseUrl { get; set; } = string.Empty;
	public string Environment { get; set; } = "Development";
	public int Weight { get; set; } = 100;
	public Dictionary<string, string>? Tags { get; set; }
	public bool EnableHealthCheck { get; set; } = true;
	public string HealthCheckPath { get; set; } = "/health";
	public int HealthCheckIntervalSeconds { get; set; } = 30;
	public int HeartbeatIntervalSeconds { get; set; } = 30;
}
