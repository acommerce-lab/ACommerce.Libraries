using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ACommerce.Messaging.SignalR.Hub.Hubs;

/// <summary>
/// Central SignalR hub for inter-service messaging
/// </summary>
public class MessagingHub(
    IMessagePublisher publisher,
    ILogger<MessagingHub> logger)
    : Hub<IMessagingClient>
{

    // Track service connections: ConnectionId → ServiceName
    private static readonly ConcurrentDictionary<string, string> _serviceConnections = new();

    // Track topic subscriptions: Topic → List of ConnectionIds
    private static readonly ConcurrentDictionary<string, HashSet<string>> _topicSubscriptions = new();

    /// <summary>
    /// Service registers itself with a name
    /// </summary>
    public async Task RegisterService(string serviceName)
    {
        _serviceConnections[Context.ConnectionId] = serviceName;

        logger.LogInformation(
            "[MessagingHub] ✅ Service '{ServiceName}' registered (ConnectionId: {ConnectionId})",
            serviceName,
            Context.ConnectionId);

        await Clients.Caller.OnServiceRegistered(serviceName);
    }

    /// <summary>
    /// Service subscribes to a topic
    /// </summary>
    public async Task SubscribeToTopic(string topic)
    {
        // Add to SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, topic);

        // Track subscription
        _topicSubscriptions.AddOrUpdate(
            topic,
            _ => [Context.ConnectionId],
            (_, set) =>
            {
                set.Add(Context.ConnectionId);
                return set;
            });

        var serviceName = _serviceConnections.GetValueOrDefault(Context.ConnectionId, "unknown");

        logger.LogInformation(
            "[MessagingHub] 🎧 '{ServiceName}' subscribed to '{Topic}' (Subscribers: {Count})",
            serviceName,
            topic,
            _topicSubscriptions[topic].Count);

        await Clients.Caller.OnTopicSubscribed(topic);
    }

    /// <summary>
    /// Service publishes a message to a topic
    /// </summary>
    public async Task PublishMessage(
        string topic,
        string messageType,
        string messageJson,
        MessageMetadata? metadata)
    {
        try
        {
            var serviceName = _serviceConnections.GetValueOrDefault(Context.ConnectionId, "unknown");

            logger.LogInformation(
                "[MessagingHub] 📤 Publishing from '{ServiceName}' to '{Topic}' (Type: {MessageType})",
                serviceName,
                topic,
                messageType.Split('.').Last());

            metadata ??= new MessageMetadata { SourceService = serviceName };

            // ✅ 1. Publish to local InMemory bus (within Messaging Service)
            try
            {
                var type = Type.GetType(messageType);
                if (type != null)
                {
                    var message = JsonSerializer.Deserialize(messageJson, type);
                    if (message != null)
                    {
                        var publishMethod = typeof(IMessagePublisher)
                            .GetMethod(nameof(IMessagePublisher.PublishAsync))!
                            .MakeGenericMethod(type);

                        var task = (Task)publishMethod.Invoke(
                            publisher,
                            [message, topic, metadata, CancellationToken.None])!;

                        await task;

                        logger.LogDebug(
                            "[MessagingHub] ✅ Published to local InMemory bus");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "[MessagingHub] ⚠️ Failed to publish to local bus (non-critical)");
            }

            // ✅ 2. Broadcast to all SignalR subscribers
            var subscribers = _topicSubscriptions.GetValueOrDefault(topic, []).Count;

            await Clients.Group(topic).OnMessageReceived(topic, messageType, messageJson, metadata);

            logger.LogInformation(
                "[MessagingHub] ✅ Broadcasted to '{Topic}' ({Count} subscribers)",
                topic,
                subscribers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[MessagingHub] 💥 Failed to publish to '{Topic}'",
                topic);

            await Clients.Caller.OnPublishFailed(topic, ex.Message);
        }
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation(
            "[MessagingHub] 🔌 Client connected: {ConnectionId}",
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var serviceName = _serviceConnections.GetValueOrDefault(Context.ConnectionId, "unknown");

        logger.LogWarning(
            "[MessagingHub] 🔌 '{ServiceName}' disconnected: {ConnectionId}",
            serviceName,
            Context.ConnectionId);

        // Remove from service connections
        _serviceConnections.TryRemove(Context.ConnectionId, out _);

        // Remove from all topic subscriptions
        foreach (var (topic, connections) in _topicSubscriptions)
        {
            connections.Remove(Context.ConnectionId);
            if (connections.Count == 0)
            {
                _topicSubscriptions.TryRemove(topic, out _);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
