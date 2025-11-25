using ACommerce.ServiceRegistry.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace ACommerce.Client.Realtime;

/// <summary>
/// Client للتواصل Realtime باستخدام SignalR
/// </summary>
public sealed class RealtimeClient : IAsyncDisposable
{
	private readonly ServiceRegistryClient _registryClient;
	private readonly ILogger<RealtimeClient> _logger;
	private HubConnection? _connection;
	private bool _isConnected;

	public RealtimeClient(
		ServiceRegistryClient registryClient,
		ILogger<RealtimeClient> logger)
	{
		_registryClient = registryClient;
		_logger = logger;
	}

	/// <summary>
	/// الاتصال بـ SignalR Hub
	/// </summary>
	public async Task ConnectAsync(
		string serviceName = "Marketplace",
		string hubPath = "/hubs/notifications",
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

		_connection = new HubConnectionBuilder()
			.WithUrl(hubUrl)
			.WithAutomaticReconnect()
			.Build();

		_connection.Closed += OnConnectionClosed;
		_connection.Reconnecting += OnReconnecting;
		_connection.Reconnected += OnReconnected;

		await _connection.StartAsync(cancellationToken);
		_isConnected = true;

		_logger.LogInformation("✅ Connected to SignalR hub");
	}

	/// <summary>
	/// الاستماع لحدث معين
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
