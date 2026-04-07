using ACommerce.ServiceMessaging.Operations.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ACommerce.ServiceMessaging.InMemory;

/// <summary>
/// ناقل رسائل الخدمات في الذاكرة.
/// يطبق IMessageBus من ACommerce.ServiceMessaging.Operations.
///
/// يدعم:
/// - Publish/Subscribe (Pub/Sub)
/// - Request/Reply (مزامن داخل العملية)
/// - Wildcard topics: "service.*" يطابق "service.created"
///
/// مناسب لـ:
/// - التطوير المحلي
/// - الاختبارات
/// - النشر الأحادي (Monolith)
/// </summary>
public class InMemoryServiceMessageBus : IMessageBus
{
    private readonly ILogger<InMemoryServiceMessageBus> _logger;

    // topic → handlers
    private readonly ConcurrentDictionary<string, List<Func<MessageEnvelope, Task>>>
        _subscribers = new(StringComparer.OrdinalIgnoreCase);

    // correlationId → pending reply
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<MessageEnvelope?>>
        _pendingReplies = new();

    public InMemoryServiceMessageBus(ILogger<InMemoryServiceMessageBus> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // =============================================
    // IMessageBus: Publish
    // =============================================

    public async Task PublishAsync(MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (envelope == null) throw new ArgumentNullException(nameof(envelope));

        _logger.LogDebug("[InMemoryBus] Publishing to topic '{Topic}' from '{Source}'",
            envelope.Topic, envelope.SourceService);

        // تحقق إذا كانت ردّاً على طلب معلّق
        if (envelope.CorrelationId.HasValue &&
            _pendingReplies.TryRemove(envelope.CorrelationId.Value, out var tcs))
        {
            _logger.LogDebug("[InMemoryBus] Completing pending request {CorrelationId}", envelope.CorrelationId);
            tcs.TrySetResult(envelope);
            return;
        }

        var handlers = FindHandlers(envelope.Topic);

        if (handlers.Count == 0)
        {
            _logger.LogDebug("[InMemoryBus] No subscribers for topic '{Topic}'", envelope.Topic);
            return;
        }

        var tasks = handlers.Select(h => InvokeHandler(h, envelope, ct));
        await Task.WhenAll(tasks);

        _logger.LogDebug("[InMemoryBus] Delivered to {Count} subscriber(s) on topic '{Topic}'",
            handlers.Count, envelope.Topic);
    }

    // =============================================
    // IMessageBus: Subscribe
    // =============================================

    public Task SubscribeAsync(
        string topic,
        Func<MessageEnvelope, Task> handler,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic required", nameof(topic));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        _subscribers.AddOrUpdate(
            topic,
            _ => new List<Func<MessageEnvelope, Task>> { handler },
            (_, list) => { lock (list) { list.Add(handler); } return list; });

        _logger.LogDebug("[InMemoryBus] Subscribed to topic '{Topic}'", topic);
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string topic, CancellationToken ct = default)
    {
        _subscribers.TryRemove(topic, out _);
        _logger.LogDebug("[InMemoryBus] Unsubscribed from topic '{Topic}'", topic);
        return Task.CompletedTask;
    }

    // =============================================
    // IMessageBus: Request/Reply
    // =============================================

    public async Task<MessageEnvelope?> RequestAsync(
        MessageEnvelope request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var correlationId = request.CorrelationId ?? Guid.NewGuid();
        request.CorrelationId = correlationId;

        var tcs = new TaskCompletionSource<MessageEnvelope?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingReplies[correlationId] = tcs;

        try
        {
            _logger.LogDebug("[InMemoryBus] Request {CorrelationId} to topic '{Topic}'",
                correlationId, request.Topic);

            await PublishAsync(request, ct);

            var delay = timeout ?? TimeSpan.FromSeconds(10);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(delay);

            var timeoutTask = Task.Delay(delay, cts.Token);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _logger.LogWarning("[InMemoryBus] Request {CorrelationId} timed out after {Timeout}",
                    correlationId, delay);
                return null;
            }

            return await tcs.Task;
        }
        finally
        {
            _pendingReplies.TryRemove(correlationId, out _);
        }
    }

    // =============================================
    // Private Helpers
    // =============================================

    private List<Func<MessageEnvelope, Task>> FindHandlers(string topic)
    {
        var result = new List<Func<MessageEnvelope, Task>>();

        foreach (var kvp in _subscribers)
        {
            if (TopicMatches(kvp.Key, topic))
            {
                lock (kvp.Value)
                    result.AddRange(kvp.Value);
            }
        }

        return result;
    }

    /// <summary>
    /// مطابقة الـ topic مع دعم wildcard (*):
    ///   "service.*"   يطابق "service.created", "service.deleted"
    ///   "*.events"    يطابق "auth.events", "order.events"
    ///   "*"           يطابق كل شيء
    /// </summary>
    private static bool TopicMatches(string pattern, string topic)
    {
        if (pattern == topic) return true;
        if (pattern == "*") return true;
        if (!pattern.Contains('*')) return false;

        var patternParts = pattern.Split('.');
        var topicParts = topic.Split('.');

        if (patternParts.Length != topicParts.Length) return false;

        for (int i = 0; i < patternParts.Length; i++)
        {
            if (patternParts[i] != "*" && !patternParts[i].Equals(topicParts[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private async Task InvokeHandler(
        Func<MessageEnvelope, Task> handler,
        MessageEnvelope envelope,
        CancellationToken ct)
    {
        try
        {
            await handler(envelope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[InMemoryBus] Handler threw on topic '{Topic}'", envelope.Topic);
        }
    }

    // =============================================
    // Debug / Testing Helpers
    // =============================================

    /// <summary>عدد المشتركين الحاليين</summary>
    public int SubscriberCount => _subscribers.Values.Sum(l => l.Count);

    /// <summary>الـ topics المسجلة</summary>
    public IEnumerable<string> RegisteredTopics => _subscribers.Keys;

    /// <summary>مسح كل المشتركين (للاختبار)</summary>
    public void ClearSubscribers() => _subscribers.Clear();
}
