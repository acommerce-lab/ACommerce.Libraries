# ACommerce.Notifications.Channels.InApp

In-app notification channel with real-time delivery via SignalR.

## Overview

Real-time in-app notifications using SignalR for instant delivery. Includes badge count updates, notification center, and read/unread tracking.

## Key Features

✅ **Real-time Delivery** - Instant notification via SignalR  
✅ **Badge Count** - Unread notification counter  
✅ **Notification Center** - Built-in notification list  
✅ **Read Tracking** - Mark as read/unread  
✅ **Persistence** - Store in database  
✅ **Auto-dismiss** - Optional auto-dismiss after duration  

## Configuration

### appsettings.json
```json
{
  "InAppNotificationSettings": {
    "HubPath": "/hubs/notifications",
    "MethodName": "ReceiveNotification",
    "BadgeCountMethodName": "UpdateBadgeCount",
    "SendBadgeCount": true,
    "AutoDismissSeconds": 0,
    "MaxNotificationsPerUser": 100,
    "EnablePersistence": true
  }
}
```

## Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add InApp Notifications
builder.Services.AddInAppNotifications(builder.Configuration);

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Map Notification Hub
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
```

## Hub Implementation
```csharp
public class NotificationHub : BaseRealtimeHub<INotificationClient>
{
    public NotificationHub(ILogger<NotificationHub> logger) 
        : base(logger)
    {
    }
    
    public async Task MarkAsRead(string notificationId)
    {
        var userId = GetUserId();
        
        // Mark as read in database
        await _notificationService.MarkAsReadAsync(Guid.Parse(notificationId), userId);
        
        // Update badge count
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
        await Clients.Caller.ReceiveMessage("UpdateBadgeCount", new { count = unreadCount });
    }
    
    public async Task MarkAllAsRead()
    {
        var userId = GetUserId();
        
        await _notificationService.MarkAllAsReadAsync(userId);
        
        await Clients.Caller.ReceiveMessage("UpdateBadgeCount", new { count = 0 });
    }
    
    protected override async Task OnUserConnectedAsync(string userId)
    {
        // Send unread count on connect
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
        await Clients.Caller.ReceiveMessage("UpdateBadgeCount", new { count = unreadCount });
        
        // Send recent notifications
        var recent = await _notificationService.GetRecentAsync(userId, 10);
        await Clients.Caller.ReceiveMessage("RecentNotifications", recent);
    }
}

public interface INotificationClient : IRealtimeClient
{
    // Inherits ReceiveMessage from IRealtimeClient
}
```

## Backend Usage

### Send In-App Notification
```csharp
var notification = new Notification
{
    UserId = "user-123",
    Title = "New Message",
    Message = "You have a new message",
    Type = NotificationType.Info,
    Channels = new List<NotificationChannel> { NotificationChannel.InApp },
    Data = new Dictionary<string, string>
    {
        ["Icon"] = "message",
        ["Link"] = "/messages/123",
        ["AutoDismiss"] = "5000" // 5 seconds
    }
};

await _notificationService.SendAsync(notification);

// Real-time delivery happens automatically via SignalR!
```

### Update Badge Count
```csharp
public class InAppNotificationService
{
    private readonly IRealtimeHub _realtimeHub;
    
    public async Task SendBadgeCountAsync(string userId, int count)
    {
        await _realtimeHub.SendToUserAsync(
            userId,
            "UpdateBadgeCount",
            new { count });
    }
}
```

## Frontend Integration

### JavaScript/React
```javascript
import * as signalR from "@microsoft/signalr";

// 1. Connect to hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/notifications")
  .withAutomaticReconnect()
  .build();

// 2. Listen for notifications
connection.on("ReceiveMessage", (method, data) => {
  if (method === "ReceiveNotification") {
    showNotification(data);
  } else if (method === "UpdateBadgeCount") {
    updateBadge(data.count);
  } else if (method === "RecentNotifications") {
    displayNotifications(data);
  }
});

// 3. Start connection
await connection.start();

// 4. Mark as read
const markAsRead = async (notificationId) => {
  await connection.invoke("MarkAsRead", notificationId);
};

// 5. Mark all as read
const markAllAsRead = async () => {
  await connection.invoke("MarkAllAsRead");
};

// UI Functions
const showNotification = (notification) => {
  // Show toast notification
  toast({
    title: notification.title,
    message: notification.message,
    type: notification.type,
    duration: notification.autoDismiss || 5000
  });
  
  // Add to notification center
  addToNotificationCenter(notification);
};

const updateBadge = (count) => {
  document.getElementById("badge").textContent = count;
  document.getElementById("badge").style.display = count > 0 ? "block" : "none";
};
```

### Vue.js
```vue
<template>
  <div>
    <!-- Notification Bell -->
    <button @click="toggleNotifications" class="relative">
      <BellIcon />
      <span v-if="unreadCount > 0" class="badge">
        {{ unreadCount }}
      </span>
    </button>
    
    <!-- Notification Center -->
    <div v-if="showNotifications" class="notification-center">
      <div class="header">
        <h3>الإشعارات</h3>
        <button @click="markAllAsRead">تعليم الكل كمقروء</button>
      </div>
      
      <div class="notifications">
        <div
          v-for="notification in notifications"
          :key="notification.id"
          :class="['notification', { unread: !notification.isRead }]"
          @click="handleNotificationClick(notification)"
        >
          <div class="icon">
            <component :is="getIcon(notification.type)" />
          </div>
          <div class="content">
            <h4>{{ notification.title }}</h4>
            <p>{{ notification.message }}</p>
            <span class="time">{{ formatTime(notification.createdAt) }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import * as signalR from '@microsoft/signalr';

const notifications = ref([]);
const unreadCount = ref(0);
const showNotifications = ref(false);
const connection = ref(null);

onMounted(async () => {
  // Connect to SignalR
  connection.value = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notifications')
    .build();
  
  connection.value.on('ReceiveMessage', (method, data) => {
    if (method === 'ReceiveNotification') {
      notifications.value.unshift(data);
      unreadCount.value++;
      showToast(data);
    } else if (method === 'UpdateBadgeCount') {
      unreadCount.value = data.count;
    } else if (method === 'RecentNotifications') {
      notifications.value = data;
    }
  });
  
  await connection.value.start();
});

const markAllAsRead = async () => {
  await connection.value.invoke('MarkAllAsRead');
  notifications.value.forEach(n => n.isRead = true);
};

const handleNotificationClick = async (notification) => {
  if (!notification.isRead) {
    await connection.value.invoke('MarkAsRead', notification.id);
    notification.isRead = true;
    unreadCount.value--;
  }
  
  // Navigate if link exists
  if (notification.data?.Link) {
    router.push(notification.data.Link);
  }
};
</script>
```

### .NET MAUI
```csharp
public class NotificationService
{
    private HubConnection _connection;
    private readonly ObservableCollection<Notification> _notifications = new();
    private int _unreadCount;
    
    public IReadOnlyList<Notification> Notifications => _notifications;
    public int UnreadCount => _unreadCount;
    
    public event EventHandler<Notification> NotificationReceived;
    public event EventHandler<int> BadgeCountChanged;
    
    public async Task ConnectAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://api.ACommerce.sa/hubs/notifications",
                options => options.AccessTokenProvider = GetAccessTokenAsync)
            .WithAutomaticReconnect()
            .Build();
        
        _connection.On<string, object>("ReceiveMessage", (method, data) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (method == "ReceiveNotification")
                {
                    var notification = JsonSerializer.Deserialize<Notification>(
                        JsonSerializer.Serialize(data));
                    
                    _notifications.Insert(0, notification);
                    _unreadCount++;
                    
                    NotificationReceived?.Invoke(this, notification);
                    BadgeCountChanged?.Invoke(this, _unreadCount);
                    
                    ShowLocalNotification(notification);
                }
                else if (method == "UpdateBadgeCount")
                {
                    var badgeData = JsonSerializer.Deserialize<BadgeCountData>(
                        JsonSerializer.Serialize(data));
                    
                    _unreadCount = badgeData.Count;
                    BadgeCountChanged?.Invoke(this, _unreadCount);
                }
                else if (method == "RecentNotifications")
                {
                    var recent = JsonSerializer.Deserialize<List<Notification>>(
                        JsonSerializer.Serialize(data));
                    
                    _notifications.Clear();
                    foreach (var n in recent)
                    {
                        _notifications.Add(n);
                    }
                }
            });
        });
        
        await _connection.StartAsync();
    }
    
    public async Task MarkAsReadAsync(string notificationId)
    {
        await _connection.InvokeAsync("MarkAsRead", notificationId);
    }
    
    public async Task MarkAllAsReadAsync()
    {
        await _connection.InvokeAsync("MarkAllAsRead");
    }
    
    private async Task<string> GetAccessTokenAsync()
    {
        return await SecureStorage.GetAsync("access_token");
    }
    
    private void ShowLocalNotification(Notification notification)
    {
        // Show local notification using MAUI
        LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            Title = notification.Title,
            Description = notification.Message,
            BadgeNumber = _unreadCount
        });
    }
}
```

## UI Components

### Notification Toast
```html
<div class="toast" :class="type">
  <div class="icon">
    <InfoIcon v-if="type === 'info'" />
    <CheckIcon v-if="type === 'success'" />
    <WarningIcon v-if="type === 'warning'" />
    <ErrorIcon v-if="type === 'error'" />
  </div>
  <div class="content">
    <h4>{{ title }}</h4>
    <p>{{ message }}</p>
  </div>
  <button @click="dismiss">×</button>
</div>
```

### Notification Center
```html
<div class="notification-center">
  <div class="header">
    <h3>الإشعارات ({{ unreadCount }})</h3>
    <button @click="markAllAsRead">تعليم الكل كمقروء</button>
  </div>
  
  <div class="filters">
    <button @click="filter = 'all'" :class="{ active: filter === 'all' }">
      الكل
    </button>
    <button @click="filter = 'unread'" :class="{ active: filter === 'unread' }">
      غير مقروءة
    </button>
  </div>
  
  <div class="notifications">
    <div
      v-for="notification in filteredNotifications"
      :key="notification.id"
      :class="['notification', { unread: !notification.isRead }]"
      @click="handleClick(notification)"
    >
      <!-- Notification content -->
    </div>
  </div>
</div>
```

## Installation
```bash
dotnet add package ACommerce.Notifications.Channels.InApp
```

## Dependencies

- ACommerce.Notifications.Abstractions
- ACommerce.Realtime.Abstractions
- ACommerce.Realtime.SignalR

## License

MIT