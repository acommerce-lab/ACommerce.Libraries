namespace ACommerce.Messaging.SignalR;

/// <summary>
/// Configuration options for SignalR messaging
/// </summary>
public class SignalRMessagingOptions
{
    /// <summary>
    /// Name of the service (used for registration and logging)
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// URL of the central Messaging Service
    /// </summary>
    public required string MessagingServiceUrl { get; set; }
}