# ACommerce.Realtime.Abstractions

Core abstractions for real-time communication systems using SignalR.

## Overview

Provider-agnostic abstractions for real-time communication. Defines contracts for SignalR hubs and clients without implementation details. Perfect for building real-time features like notifications, chat, live updates, and authentication flows.

## Key Features

✅ **Provider Agnostic** - Interface-based design  
✅ **SignalR Ready** - Built for SignalR infrastructure  
✅ **Lightweight** - Only interfaces (~100 lines)  
✅ **Extensible** - Easy to implement custom hubs  
✅ **Type Safe** - Strongly-typed client methods  

## Core Interfaces

### IRealtimeHub
Main interface for real-time hub service

**Methods:**
- `SendToUserAsync(userId, method, data)` - Send to specific user
- `SendToGroupAsync(groupName, method, data)` - Send to group
- `SendToAllAsync(method, data)` - Broadcast to all
- `AddToGroupAsync(userId, groupName)` - Add user to group
- `RemoveFromGroupAsync(userId, groupName)` - Remove user from group

### IRealtimeClient
Base interface for SignalR client methods

**Methods:**
- `ReceiveMessage(method, data)` - Generic receive method

## Usage

### Implement Custom Hub Service
```csharp
public class MyCustomHub : IRealtimeHub
{
    public async Task SendToUserAsync(
        string userId, 
        string method, 
        object data, 
        CancellationToken cancellationToken = default)
    {
        // Your implementation
    }
    
    // Implement other methods...
}
```

### Implement Custom Client
```csharp
public interface IMyCustomClient : IRealtimeClient
{
    Task OnNotificationReceived(Notification notification);
    Task OnStatusChanged(string status);
}
```

## Available Implementations

- **ACommerce.Realtime.SignalR** - SignalR implementation
- *(Add your own implementations)*

## Installation
```bash
dotnet add package ACommerce.Realtime.Abstractions
```

## Dependencies

None - Pure interfaces

## Integration Pattern
```csharp
// Register in DI
services.AddScoped<IRealtimeHub, SignalRRealtimeHub<MyHub, IMyClient>>();

// Use in services
public class NotificationService
{
    private readonly IRealtimeHub _realtimeHub;
    
    public async Task SendNotification(string userId, string message)
    {
        await _realtimeHub.SendToUserAsync(userId, "ReceiveNotification", message);
    }
}
```

## License

MIT