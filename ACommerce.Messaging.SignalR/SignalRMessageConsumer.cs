using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ACommerce.Messaging.SignalR;

/// <summary>
/// SignalR-based message consumer for real-time messaging
/// </summary>
public class SignalRMessageConsumer : IMessageConsumer, IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly SignalRMessagingOptions _options;
    private readonly ILogger<SignalRMessageConsumer> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly ConcurrentDictionary<string, List<SubscriptionHandler>> _handlers = new();

    public SignalRMessageConsumer(
        SignalRMessagingOptions options,
        ILogger<SignalRMessageConsumer> logger)
    {
        _options = options;
        _logger = logger;

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

        // Setup message receiver
        _connection.On<string, string, string, MessageMetadata>(
            "OnMessageReceived",
            HandleReceivedMessage);

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
                "[SignalR Consumer] ✅ Connected to Messaging Hub at {Url}",
                _options.MessagingServiceUrl);

            // Register service
            await _connection.InvokeAsync("RegisterService", _options.ServiceName);

            _logger.LogInformation(
                "[SignalR Consumer] 📝 Registered as '{ServiceName}'",
                _options.ServiceName);

            // Re-subscribe to all topics
            foreach (var topic in _handlers.Keys)
            {
                await _connection.InvokeAsync("SubscribeToTopic", topic);
                _logger.LogInformation(
                    "[SignalR Consumer] 🔄 Re-subscribed to topic '{Topic}'",
                    topic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[SignalR Consumer] ❌ Failed to connect to Messaging Hub");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task SubscribeAsync<TMessage>(
        string topic,
        Func<TMessage, MessageMetadata, Task<bool>> handler,
        CancellationToken cancellationToken = default)
        where TMessage : class
    {
        await EnsureConnectedAsync();

        // Store handler
        var handlerWrapper = new SubscriptionHandler
        {
            MessageType = typeof(TMessage),
            Handler = async (messageJson, metadata) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<TMessage>(messageJson);
                    if (message != null)
                    {
                        return await handler(message, metadata);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[SignalR Consumer] 💥 Error in handler for topic '{Topic}'",
                        topic);
                    return false;
                }
            }
        };

        _handlers.AddOrUpdate(
            topic,
            _ => new List<SubscriptionHandler> { handlerWrapper },
            (_, list) =>
            {
                list.Add(handlerWrapper);
                return list;
            });

        // Subscribe to topic via SignalR
        await _connection.InvokeAsync("SubscribeToTopic", topic, cancellationToken);

        _logger.LogInformation(
            "[SignalR Consumer] 🎧 Subscribed to topic '{Topic}' for type '{MessageType}'",
            topic,
            typeof(TMessage).Name);
    }

    private async void HandleReceivedMessage(
        string topic,
        string messageType,
        string messageJson,
        MessageMetadata metadata)
    {
        try
        {
            _logger.LogDebug(
                "[SignalR Consumer] 📨 Received message from topic '{Topic}', Type: {MessageType}",
                topic,
                messageType);

            if (!_handlers.TryGetValue(topic, out var handlers))
            {
                _logger.LogWarning(
                    "[SignalR Consumer] ⚠️ No handlers for topic '{Topic}'",
                    topic);
                return;
            }

            // Execute all handlers for this topic
            foreach (var handler in handlers)
            {
                try
                {
                    var success = await handler.Handler(messageJson, metadata);

                    if (success)
                    {
                        _logger.LogDebug(
                            "[SignalR Consumer] ✅ Handler executed successfully for topic '{Topic}'",
                            topic);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "[SignalR Consumer] ⚠️ Handler returned false for topic '{Topic}'",
                            topic);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[SignalR Consumer] 💥 Handler failed for topic '{Topic}'",
                        topic);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[SignalR Consumer] 💥 Error handling received message from topic '{Topic}'",
                topic);
        }
    }

    private Task OnReconnecting(Exception? exception)
    {
        _logger.LogWarning(
            "[SignalR Consumer] 🔄 Reconnecting to Messaging Hub...");
        return Task.CompletedTask;
    }

    private async Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation(
            "[SignalR Consumer] ✅ Reconnected to Messaging Hub");

        await _connection.InvokeAsync("RegisterService", _options.ServiceName);

        // Re-subscribe to all topics
        foreach (var topic in _handlers.Keys)
        {
            await _connection.InvokeAsync("SubscribeToTopic", topic);
        }
    }

    private Task OnClosed(Exception? exception)
    {
        _logger.LogWarning(exception,
            "[SignalR Consumer] ⚠️ Connection closed");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _connectionLock.Dispose();
        await _connection.DisposeAsync();
    }

    private class SubscriptionHandler
    {
        public required Type MessageType { get; init; }
        public required Func<string, MessageMetadata, Task<bool>> Handler { get; init; }
    }
}
