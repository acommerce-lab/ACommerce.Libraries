using ACommerce.Messaging.Abstractions.Models;

namespace ACommerce.Messaging.Abstractions.Contracts;

/// <summary>
/// Consumer interface for receiving messages
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// Subscribe to a topic and handle incoming messages
    /// </summary>
    Task SubscribeAsync<T>(
        string topic,
        Func<T, MessageMetadata, Task<bool>> handler,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Unsubscribe from a topic
    /// </summary>
    Task UnsubscribeAsync(
        string topic,
        CancellationToken cancellationToken = default);
}
