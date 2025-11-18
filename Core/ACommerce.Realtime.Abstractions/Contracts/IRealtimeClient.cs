namespace ACommerce.Realtime.Abstractions.Contracts;

/// <summary>
/// Base interface for SignalR client methods
/// </summary>
public interface IRealtimeClient
{
	Task ReceiveMessage(string method, object data);
}

