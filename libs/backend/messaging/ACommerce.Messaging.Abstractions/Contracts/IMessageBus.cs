namespace ACommerce.Messaging.Abstractions.Contracts;

/// <summary>
/// Complete message bus interface (Publisher + Consumer + Requestor)
/// </summary>
public interface IMessageBus : IMessagePublisher, IMessageConsumer, IMessageRequestor
{
    /// <summary>
    /// Connection status
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Start the message bus
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop the message bus
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}