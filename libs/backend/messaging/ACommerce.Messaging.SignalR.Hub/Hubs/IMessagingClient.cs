using ACommerce.Messaging.Abstractions.Models;

namespace ACommerce.Messaging.SignalR.Hub.Hubs;

/// <summary>
/// Client interface for SignalR callbacks
/// </summary>
public interface IMessagingClient
{
    Task OnServiceRegistered(string serviceName);
    Task OnTopicSubscribed(string topic);
    Task OnMessageReceived(string topic, string messageType, string messageJson, MessageMetadata metadata);
    Task OnPublishFailed(string topic, string error);
}