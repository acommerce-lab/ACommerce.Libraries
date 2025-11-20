using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ACommerce.Messaging.InMemory;

/// <summary>
/// In-memory message bus for development and testing
/// Supports: Pub/Sub, Request-Response, Topic Wildcards
/// </summary>
public class InMemoryMessageBus(
    ILogger<InMemoryMessageBus> logger,
    string serviceName)
    : IMessageBus
{
    private readonly ILogger<InMemoryMessageBus> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));

    // Topic → Handlers
    private static readonly ConcurrentDictionary<string, List<Func<object, MessageMetadata, Task<bool>>>>
        _subscribers = new();

    // CorrelationId → Pending Request
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<object>>
        _pendingRequests = new();

    // Message History (for debugging)
    private static readonly ConcurrentQueue<MessageHistoryEntry>
        _messageHistory = new();

    public bool IsConnected => true;

    // ========================================
    // IMessagePublisher
    // ========================================

    public async Task<MessageResult> PublishAsync<T>(
        T message,
        string topic,
        MessageMetadata? metadata = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            metadata ??= new MessageMetadata();
            metadata = metadata with { SourceService = _serviceName };

            var envelope = new MessageEnvelope<T>
            {
                Topic = topic,
                Payload = message,
                SourceService = _serviceName,
                Metadata = metadata
            };

            // Add to history
            AddToHistory(topic, envelope);

            _logger.LogDebug(
                "[{Service}] Publishing to topic '{Topic}', CorrelationId: {CorrelationId}",
                _serviceName, topic, metadata.CorrelationId);

            // Check if this is a reply to pending request
            if (!string.IsNullOrEmpty(metadata.CorrelationId) &&
                _pendingRequests.TryRemove(metadata.CorrelationId, out var tcs))
            {
                _logger.LogDebug(
                    "[{Service}] Completing pending request {CorrelationId}",
                    _serviceName, metadata.CorrelationId);

                tcs.SetResult(message);
            }

            // Find matching subscribers
            var handlers = FindMatchingHandlers(topic);

            if (handlers.Count == 0)
            {
                _logger.LogWarning(
                    "[{Service}] No subscribers for topic '{Topic}'",
                    _serviceName, topic);

                return MessageResult.Ok(envelope.Id.ToString(), 0);
            }

            // Notify all handlers
            var tasks = handlers.Select(handler => Task.Run(async () =>
            {
                try
                {
                    await handler(envelope, metadata);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[{Service}] Handler failed for topic '{Topic}'",
                        _serviceName, topic);
                }
            }, cancellationToken));

            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "[{Service}] Published message {MessageId} to topic '{Topic}', {Count} subscribers notified",
                _serviceName, envelope.Id, topic, handlers.Count);

            return MessageResult.Ok(envelope.Id.ToString(), handlers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[{Service}] Failed to publish to topic '{Topic}'",
                _serviceName, topic);

            return MessageResult.Fail(ex.Message);
        }
    }

    public async Task<MessageResult> PublishBatchAsync<T>(
        IEnumerable<T> messages,
        string topic,
        MessageMetadata? metadata = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var tasks = messages.Select(m =>
            PublishAsync(m, topic, metadata, cancellationToken));

        await Task.WhenAll(tasks);

        return MessageResult.Ok(Guid.NewGuid().ToString());
    }

    // ========================================
    // IMessageConsumer
    // ========================================

    public Task SubscribeAsync<T>(
        string topic,
        Func<T, MessageMetadata, Task<bool>> handler,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var wrappedHandler = new Func<object, MessageMetadata, Task<bool>>(async (msg, metadata) =>
        {
            try
            {
                if (msg is MessageEnvelope<T> envelope)
                {
                    return await handler(envelope.Payload, metadata);
                }
                else if (msg is T typedMsg)
                {
                    return await handler(typedMsg, metadata);
                }

                _logger.LogWarning(
                    "[{Service}] Message type mismatch for topic '{Topic}'. Expected {Expected}, got {Actual}",
                    _serviceName, topic, typeof(T).Name, msg.GetType().Name);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[{Service}] Handler exception for topic '{Topic}'",
                    _serviceName, topic);
                return false;
            }
        });

        _subscribers.AddOrUpdate(
            topic,
            new List<Func<object, MessageMetadata, Task<bool>>> { wrappedHandler },
            (_, list) =>
            {
                list.Add(wrappedHandler);
                return list;
            });

        _logger.LogInformation(
            "[{Service}] Subscribed to topic '{Topic}'",
            _serviceName, topic);

        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(
        string topic,
        CancellationToken cancellationToken = default)
    {
        _subscribers.TryRemove(topic, out _);

        _logger.LogInformation(
            "[{Service}] Unsubscribed from topic '{Topic}'",
            _serviceName, topic);

        return Task.CompletedTask;
    }

    // ========================================
    // IMessageRequestor
    // ========================================

    public async Task<TResponse?> RequestAsync<TRequest, TResponse>(
        TRequest request,
        string targetService,
        string requestType,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<object>();

        _pendingRequests[correlationId] = tcs;

        try
        {
            var topic = $"{targetService}.{requestType}";

            _logger.LogDebug(
                "[{Service}] Sending request {CorrelationId} to '{Topic}'",
                _serviceName, correlationId, topic);

            // Publish request
            await PublishAsync(
                request,
                topic: topic,
                metadata: new MessageMetadata
                {
                    SourceService = _serviceName,
                    CorrelationId = correlationId,
                    Headers = new Dictionary<string, string>
                    {
                        ["RequestType"] = "Query",
                        ["ResponseType"] = typeof(TResponse).Name
                    }
                },
                cancellationToken);

            // Wait for response
            var timeoutTask = Task.Delay(timeout ?? TimeSpan.FromSeconds(5), cancellationToken);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _logger.LogWarning(
                    "[{Service}] Request {CorrelationId} to '{Topic}' timed out",
                    _serviceName, correlationId, topic);

                throw new TimeoutException(
                    $"Request to {topic} timed out after {timeout?.TotalSeconds ?? 5}s");
            }

            var result = await tcs.Task;

            _logger.LogDebug(
                "[{Service}] Received response for request {CorrelationId}",
                _serviceName, correlationId);

            return result as TResponse;
        }
        finally
        {
            _pendingRequests.TryRemove(correlationId, out _);
        }
    }

    // ========================================
    // IMessageBus
    // ========================================

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[{Service}] InMemory Message Bus started",
            _serviceName);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[{Service}] InMemory Message Bus stopped",
            _serviceName);

        return Task.CompletedTask;
    }

    // ========================================
    // Private Helpers
    // ========================================

    private List<Func<object, MessageMetadata, Task<bool>>> FindMatchingHandlers(string topic)
    {
        var handlers = new List<Func<object, MessageMetadata, Task<bool>>>();

        foreach (var kvp in _subscribers)
        {
            // Support wildcards: auth.events.* matches auth.events.user.created
            if (IsTopicMatch(kvp.Key, topic))
            {
                handlers.AddRange(kvp.Value);
            }
        }

        return handlers;
    }

    private static bool IsTopicMatch(string pattern, string topic)
    {
        if (pattern == topic) return true;
        if (!pattern.Contains('*')) return false;

        var patternParts = pattern.Split('.');
        var topicParts = topic.Split('.');

        if (patternParts.Length != topicParts.Length) return false;

        for (int i = 0; i < patternParts.Length; i++)
        {
            if (patternParts[i] != "*" && patternParts[i] != topicParts[i])
            {
                return false;
            }
        }

        return true;
    }

    private static void AddToHistory(string topic, object envelope)
    {
        _messageHistory.Enqueue(new MessageHistoryEntry
        {
            Topic = topic,
            Message = envelope,
            Timestamp = DateTimeOffset.UtcNow
        });

        // Keep only last 1000 messages
        while (_messageHistory.Count > 1000)
        {
            _messageHistory.TryDequeue(out _);
        }
    }

    // ========================================
    // Static Debug Helpers
    // ========================================

    /// <summary>
    /// Get message history (for debugging)
    /// </summary>
    public static IEnumerable<MessageHistoryEntry> GetMessageHistory()
        => _messageHistory.ToArray();

    /// <summary>
    /// Clear all subscribers (for testing)
    /// </summary>
    public static void ClearSubscribers()
        => _subscribers.Clear();

    /// <summary>
    /// Clear message history
    /// </summary>
    public static void ClearHistory()
        => _messageHistory.Clear();
}

/// <summary>
/// Message history entry for debugging
/// </summary>
public record MessageHistoryEntry
{
    public required string Topic { get; init; }
    public required object Message { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}