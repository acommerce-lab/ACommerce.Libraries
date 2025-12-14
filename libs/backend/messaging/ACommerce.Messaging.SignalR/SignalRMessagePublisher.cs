using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ACommerce.Messaging.SignalR;

/// <summary>
/// SignalR-based message publisher for real-time messaging
/// </summary>
public class SignalRMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly SignalRMessagingOptions _options;
    private readonly ILogger<SignalRMessagePublisher> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public SignalRMessagePublisher(
        SignalRMessagingOptions options,
        ILogger<SignalRMessagePublisher> logger)
    {
        _options = options;
        _logger = logger;

        // Create persistent SignalR connection
        _connection = new HubConnectionBuilder()
            .WithUrl($"{options.MessagingServiceUrl}/hubs/messaging")
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        // Setup event handlers
        _connection.Reconnecting += OnReconnecting;
        _connection.Reconnected += OnReconnected;
        _connection.Closed += OnClosed;

        // Start connection
        _ = EnsureConnectedAsync();
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            return;

        await _connectionLock.WaitAsync();
        try
        {
            if (_connection.State == HubConnectionState.Connected)
                return;

            await _connection.StartAsync();

            _logger.LogInformation(
                "[SignalR Publisher] ✅ Connected to Messaging Hub at {Url}",
                _options.MessagingServiceUrl);

            // Register service with hub
            await _connection.InvokeAsync("RegisterService", _options.ServiceName);

            _logger.LogInformation(
                "[SignalR Publisher] 📝 Registered as '{ServiceName}'",
                _options.ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[SignalR Publisher] ❌ Failed to connect to Messaging Hub");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task<MessageResult> PublishAsync<TMessage>(
    TMessage message,
    string topic,
    MessageMetadata? metadata = null,
    CancellationToken cancellationToken = default)
    where TMessage : class
    {
        try
        {
            await EnsureConnectedAsync();

            var messageType = typeof(TMessage).AssemblyQualifiedName!;
            var messageJson = JsonSerializer.Serialize(message);

            metadata ??= new MessageMetadata
            {
                SourceService = _options.ServiceName,
                CorrelationId = Guid.NewGuid().ToString()
            };

            _logger.LogDebug(
                "[SignalR Publisher] 📤 Publishing to topic '{Topic}', Type: {MessageType}",
                topic,
                typeof(TMessage).Name);

            // استدعاء الهب لنشر الرسالة
            var messageId = Guid.NewGuid().ToString(); // أو القيمة التي يرجعها السيرفر إذا عندك
            await _connection.InvokeAsync(
                "PublishMessage",
                topic,
                messageType,
                messageJson,
                metadata,
                cancellationToken);

            _logger.LogDebug(
                "[SignalR Publisher] ✅ Published to topic '{Topic}'",
                topic);

            // ترجع نتيجة ناجحة باستخدام الـ factory method
            return MessageResult.Ok(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[SignalR Publisher] 💥 Failed to publish to topic '{Topic}'",
                topic);

            return MessageResult.Fail(ex.Message);
        }
    }

    public async Task<MessageResult> PublishBatchAsync<TMessage>(
    IEnumerable<TMessage> messages,
    string topic,
    MessageMetadata? metadata = null,
    CancellationToken cancellationToken = default)
    where TMessage : class
    {
        try
        {
            await EnsureConnectedAsync();

            var messageList = messages.ToList();
            var messageJson = JsonSerializer.Serialize(messageList);

            metadata ??= new MessageMetadata
            {
                SourceService = _options.ServiceName,
                CorrelationId = Guid.NewGuid().ToString()
            };

            var messageId = Guid.NewGuid().ToString(); // أو قيمة من السيرفر
            await _connection.InvokeAsync(
                "PublishBatch",
                topic,
                typeof(TMessage).AssemblyQualifiedName!,
                messageJson,
                metadata,
                cancellationToken);

            return MessageResult.Ok(messageId, subscriberCount: messageList.Count);
        }
        catch (Exception ex)
        {
            return MessageResult.Fail(ex.Message);
        }
    }

    private Task OnReconnecting(Exception? exception)
    {
        _logger.LogWarning(
            "[SignalR Publisher] 🔄 Reconnecting to Messaging Hub...");
        return Task.CompletedTask;
    }

    private async Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation(
            "[SignalR Publisher] ✅ Reconnected to Messaging Hub");

        // Re-register service after reconnection
        await _connection.InvokeAsync("RegisterService", _options.ServiceName);
    }

    private Task OnClosed(Exception? exception)
    {
        _logger.LogWarning(exception,
            "[SignalR Publisher] ⚠️ Connection closed");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _connectionLock.Dispose();
        await _connection.DisposeAsync();
    }
}