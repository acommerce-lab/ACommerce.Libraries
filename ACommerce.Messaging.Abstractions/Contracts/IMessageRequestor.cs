namespace ACommerce.Messaging.Abstractions.Contracts;

/// <summary>
/// Request-Response pattern interface
/// </summary>
public interface IMessageRequestor
{
    /// <summary>
    /// Send a request and wait for response
    /// </summary>
    Task<TResponse?> RequestAsync<TRequest, TResponse>(
        TRequest request,
        string targetService,
        string requestType,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;
}
