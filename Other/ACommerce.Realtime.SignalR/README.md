# ACommerce.Realtime.SignalR

SignalR implementation of real-time communication abstractions.

## Overview

Production-ready SignalR implementation with base hub classes, connection management, and generic service wrappers. Implements ACommerce.Realtime.Abstractions for real-time communication.

## Key Features

🔥 **BaseRealtimeHub** - Reusable base hub with auto-features  
🔥 **Generic Implementation** - Works with any hub/client pair  
✅ **Connection Management** - Auto user tracking  
✅ **Group Management** - Built-in group operations  
✅ **Presence Tracking** - User online/offline detection  
✅ **Strongly Typed** - Type-safe client methods  

## Core Components

### BaseRealtimeHub<TClient>
Abstract base class for SignalR hubs

**Features:**
- ✅ Auto user connection tracking
- ✅ Personal user groups (userId as group name)
- ✅ OnConnectedAsync / OnDisconnectedAsync hooks
- ✅ User ID extraction from claims
- ✅ Logging integration

**Virtual Methods:**
- `OnUserConnectedAsync(userId)` - Override for custom logic
- `OnUserDisconnectedAsync(userId, exception)` - Override for cleanup
- `GetUserId()` - Override for custom user ID extraction

### SignalRRealtimeHub<THub, TClient>
Generic IRealtimeHub implementation

**Features:**
- ✅ Implements IRealtimeHub interface
- ✅ Works with any Hub<TClient> pair
- ✅ Type-safe message delivery
- ✅ Automatic logging

## Usage

### 1. Create Your Hub
```csharp
using ACommerce.Realtime.Abstractions.Contracts;
using ACommerce.Realtime.SignalR.Hubs;

public class NotificationHub : BaseRealtimeHub<INotificationClient>
{
    public NotificationHub(ILogger<NotificationHub> logger) 
        : base(logger)
    {
    }
    
    // Add custom methods
    public async Task MarkAsRead(string notificationId)
    {
        var userId = GetUserId();
        // Your logic here
    }
    
    // Override lifecycle events
    protected override async Task OnUserConnectedAsync(string userId)
    {
        Logger.LogInformation("User {UserId} connected to notifications", userId);
        // Load unread notifications, etc.
    }
}

public interface INotificationClient : IRealtimeClient
{
    Task OnNotificationReceived(Notification notification);
}
```

### 2. Register Services
```csharp
// In Program.cs
services.AddSignalR();

// Register the hub implementation
services.AddScoped<IRealtimeHub, SignalRRealtimeHub<NotificationHub, INotificationClient>>();
```

### 3. Map Hub Endpoint
```csharp
app.MapHub<NotificationHub>("/hubs/notifications");
```

### 4. Use in Services
```csharp
public class NotificationService
{
    private readonly IRealtimeHub _realtimeHub;
    
    public NotificationService(IRealtimeHub realtimeHub)
    {
        _realtimeHub = realtimeHub;
    }
    
    public async Task SendNotification(string userId, Notification notification)
    {
        // Send to specific user
        await _realtimeHub.SendToUserAsync(
            userId, 
            "OnNotificationReceived", 
            notification);
    }
    
    public async Task BroadcastAnnouncement(string message)
    {
        // Send to all connected users
        await _realtimeHub.SendToAllAsync(
            "OnNotificationReceived", 
            new { Type = "Announcement", Message = message });
    }
}
```

## Client Integration

### JavaScript/TypeScript
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")
    .withAutomaticReconnect()
    .build();

// Listen for messages
connection.on("ReceiveMessage", (method, data) => {
    if (method === "OnNotificationReceived") {
        console.log("Notification:", data);
    }
});

// Connect
await connection.start();

// Call hub methods
await connection.invoke("MarkAsRead", notificationId);
```

### .NET MAUI / Blazor
```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://api.example.com/hubs/notifications")
    .Build();

connection.On<Notification>("ReceiveMessage", (method, data) =>
{
    if (method == "OnNotificationReceived")
    {
        // Handle notification
    }
});

await connection.StartAsync();
```

## Advanced Features

### Group Operations
```csharp
// In your hub
public async Task JoinProjectGroup(string projectId)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
}

// In your service
await _realtimeHub.SendToGroupAsync($"project_{projectId}", "ProjectUpdated", data);
```

### User Presence
```csharp
public class PresenceHub : BaseRealtimeHub<IPresenceClient>
{
    private readonly IPresenceTracker _presenceTracker;
    
    protected override async Task OnUserConnectedAsync(string userId)
    {
        await _presenceTracker.UserConnected(userId);
        await Clients.All.ReceiveMessage("UserOnline", new { UserId = userId });
    }
    
    protected override async Task OnUserDisconnectedAsync(string userId, Exception? ex)
    {
        await _presenceTracker.UserDisconnected(userId);
        await Clients.All.ReceiveMessage("UserOffline", new { UserId = userId });
    }
}
```

## Installation
```bash
dotnet add package ACommerce.Realtime.SignalR
```

## Dependencies

- ACommerce.Realtime.Abstractions
- Microsoft.AspNetCore.SignalR
- Microsoft.Extensions.Logging

## License

MIT