using ACommerce.Messaging.Abstractions.Models;

namespace ACommerce.Messaging.Abstractions.Contracts;

/// <summary>
/// Publisher interface for sending messages
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publish a message to a topic
    /// </summary>
    Task<MessageResult> PublishAsync<T>(
        T message,
        string topic,
        MessageMetadata? metadata = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Publish multiple messages to a topic
    /// </summary>
    Task<MessageResult> PublishBatchAsync<T>(
        IEnumerable<T> messages,
        string topic,
        MessageMetadata? metadata = null,
        CancellationToken cancellationToken = default)
        where T : class;
}
