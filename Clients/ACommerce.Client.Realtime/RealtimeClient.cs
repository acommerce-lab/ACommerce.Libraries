using ACommerce.ServiceRegistry.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Client.Realtime;

/// <summary>
/// خيارات RealtimeClient
/// </summary>
public class RealtimeClientOptions
{
	/// <summary>
	/// تجاوز التحقق من شهادة SSL (للتطوير فقط!)
	/// </summary>
	public bool BypassSslValidation { get; set; } = false;
}

/// <summary>
/// Client للتواصل Realtime باستخدام SignalR
/// </summary>
public sealed class RealtimeClient : IAsyncDisposable
{
	private readonly ServiceRegistryClient _registryClient;
	private readonly ILogger<RealtimeClient> _logger;
	private readonly RealtimeClientOptions _options;
    private HubConnection? _connection;
	private bool _isConnected;

	public RealtimeClient(
		ServiceRegistryClient registryClient,
		ILogger<RealtimeClient> logger,
		IOptions<RealtimeClientOptions>? options = null)
	{
		_registryClient = registryClient;
		_logger = logger;
		_options = options?.Value ?? new RealtimeClientOptions();
	}

    /// <summary>
    /// الاتصال بـ SignalR Hub
    /// </summary>
	/// <param name="serviceName">اسم الخدمة</param>
	/// <param name="hubPath">مسار الـ Hub</param>
	/// <param name="accessToken">رمز المصادقة (اختياري)</param>
	/// <param name="cancellationToken">رمز الإلغاء</param>
    public async Task ConnectAsync(
		string serviceName = "Marketplace",
		string hubPath = "/hubs/notifications",
		string? accessToken = null,
		CancellationToken cancellationToken = default)
	{
		if (_isConnected)
		{
			_logger.LogWarning("Already connected to SignalR hub");
			return;
		}

		// اكتشاف الخدمة
		var endpoint = await _registryClient.DiscoverAsync(serviceName, cancellationToken);
		if (endpoint == null)
		{
			throw new InvalidOperationException($"Service not found: {serviceName}");
		}

		var hubUrl = $"{endpoint.BaseUrl.TrimEnd('/')}{hubPath}";
		_logger.LogInformation("Connecting to SignalR hub: {HubUrl}", hubUrl);

		var hubConnectionBuilder = new HubConnectionBuilder()
			.WithUrl(hubUrl, options =>
			{
				// تجاوز SSL في التطوير
				if (_options.BypassSslValidation)
				{
					options.HttpMessageHandlerFactory = _ => new HttpClientHandler
					{
						ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
					};
				}

				// إرسال رمز المصادقة مع الاتصال
				if (!string.IsNullOrEmpty(accessToken))
				{
					options.AccessTokenProvider = () => Task.FromResult(accessToken);
				}
			})
			.WithAutomaticReconnect();

		_connection = hubConnectionBuilder.Build();

		_connection.Closed += OnConnectionClosed;
		_connection.Reconnecting += OnReconnecting;
		_connection.Reconnected += OnReconnected;

		await _connection.StartAsync(cancellationToken);
		_isConnected = true;

		_logger.LogInformation("✅ Connected to SignalR hub");
	}

	/// <summary>
	/// الاستماع لحدث معين مع معامل واحد
	/// </summary>
	public IDisposable On<T>(string eventName, Action<T> handler)
	{
		if (_connection == null)
		{
			throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
		}

		return _connection.On(eventName, handler);
	}

	/// <summary>
	/// الاستماع لحدث معين مع معاملين (مثل ReceiveMessage)
	/// </summary>
	public IDisposable On<T1, T2>(string eventName, Action<T1, T2> handler)
	{
		if (_connection == null)
		{
			throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
		}

		return _connection.On(eventName, handler);
	}

	/// <summary>
	/// إرسال رسالة للـ Hub
	/// </summary>
	public async Task SendAsync(string methodName, object? arg1 = null, CancellationToken cancellationToken = default)
	{
		if (_connection == null)
		{
			throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
		}

		if (arg1 != null)
		{
			await _connection.SendAsync(methodName, arg1, cancellationToken);
		}
		else
		{
			await _connection.SendAsync(methodName, cancellationToken);
		}
	}

	/// <summary>
	/// استدعاء method في الـ Hub مع استلام Response
	/// </summary>
	public async Task<TResult?> InvokeAsync<TResult>(
		string methodName,
		object? arg1 = null,
		CancellationToken cancellationToken = default)
	{
		if (_connection == null)
		{
			throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
		}

		if (arg1 != null)
		{
			return await _connection.InvokeAsync<TResult>(methodName, arg1, cancellationToken);
		}
		else
		{
			return await _connection.InvokeAsync<TResult>(methodName, cancellationToken);
		}
	}

	/// <summary>
	/// قطع الاتصال
	/// </summary>
	public async Task DisconnectAsync(CancellationToken cancellationToken = default)
	{
		if (_connection != null && _isConnected)
		{
			await _connection.StopAsync(cancellationToken);
			_isConnected = false;
			_logger.LogInformation("❌ Disconnected from SignalR hub");
		}
	}

	// Events
	private Task OnConnectionClosed(Exception? exception)
	{
		_isConnected = false;
		_logger.LogWarning(exception, "SignalR connection closed");
		return Task.CompletedTask;
	}

	private Task OnReconnecting(Exception? exception)
	{
		_logger.LogWarning(exception, "SignalR reconnecting...");
		return Task.CompletedTask;
	}

	private Task OnReconnected(string? connectionId)
	{
		_isConnected = true;
		_logger.LogInformation("✅ SignalR reconnected (ConnectionId: {ConnectionId})", connectionId);
		return Task.CompletedTask;
	}

	public async ValueTask DisposeAsync()
	{
		if (_connection != null)
		{
			await DisconnectAsync();
			await _connection.DisposeAsync();
		}
	}
}
