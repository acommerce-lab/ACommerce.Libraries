# ACommerce.Notifications.Core

Core notification service implementation with database storage, template management, and multi-channel support.

## Overview

Complete notification system with database-backed storage, template engine, preference management, and delivery tracking. Orchestrates multiple notification channels (Email, SMS, Push, InApp, WhatsApp, Webhook).

## Key Features

🔥 **Multi-Channel Orchestration** - Route to Email, SMS, Push, InApp, etc.  
✅ **Template Management** - Reusable templates with variables  
✅ **User Preferences** - Per-user channel preferences  
✅ **Delivery Tracking** - Track sent, delivered, read status  
✅ **Retry Logic** - Automatic retry on failure  
✅ **Batch Processing** - Send to multiple users efficiently  
✅ **Priority Queue** - Critical notifications first  

## Domain Entities

### Notification
Main notification entity

**Properties:**
- UserId, Title, Message
- Type (Info, Success, Warning, Error, SystemAlert)
- Priority (Low, Normal, High, Critical)
- Status (Pending, Sent, Delivered, Read, Failed)
- Channels (Email, SMS, Push, InApp, etc.)
- TemplateId, TemplateCode
- Data (key-value pairs for template variables)
- ScheduledFor
- SentAt, DeliveredAt, ReadAt
- Metadata

### NotificationTemplate
Reusable templates

**Properties:**
- Name, Code
- Subject, Body
- Channel, Language
- Variables (placeholder list)
- IsActive
- CreatedAt, UpdatedAt

### NotificationPreference
User channel preferences

**Properties:**
- UserId
- Channel
- IsEnabled
- QuietHoursStart, QuietHoursEnd
- Frequency (Immediate, Daily, Weekly)
- Categories (enabled notification types)

### NotificationDelivery
Delivery tracking per channel

**Properties:**
- NotificationId
- Channel
- Status (Pending, Sent, Delivered, Failed)
- MessageId (from provider)
- Error, ErrorCode
- SentAt, DeliveredAt
- RetryCount, MaxRetries
- Metadata

## Configuration

### appsettings.json
```json
{
  "NotificationSettings": {
    "DefaultPriority": "Normal",
    "MaxRetries": 3,
    "RetryDelaySeconds": 60,
    "EnableBatching": true,
    "BatchSize": 100,
    "EnableQuietHours": true,
    "DefaultQuietHours": {
      "Start": "22:00",
      "End": "08:00"
    },
    "Channels": {
      "Email": {
        "Enabled": true,
        "Priority": 1
      },
      "SMS": {
        "Enabled": true,
        "Priority": 2
      },
      "Push": {
        "Enabled": true,
        "Priority": 3
      },
      "InApp": {
        "Enabled": true,
        "Priority": 4
      }
    }
  }
}
```

## Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Notifications Core
builder.Services.AddNotificationsCore(builder.Configuration);

// Add specific channels
builder.Services.AddEmailNotifications(builder.Configuration);
builder.Services.AddSmsNotifications(builder.Configuration);
builder.Services.AddPushNotifications(builder.Configuration);
builder.Services.AddInAppNotifications(builder.Configuration);

// Add background services
builder.Services.AddHostedService<NotificationProcessorService>();
builder.Services.AddHostedService<NotificationRetryService>();

var app = builder.Build();
app.Run();
```

## Usage

### Send Simple Notification
```csharp
var notification = new Notification
{
    UserId = "user-123",
    Title = "New Message",
    Message = "You have a new message from John",
    Type = NotificationType.Info,
    Priority = NotificationPriority.Normal,
    Channels = new List<NotificationChannel>
    {
        NotificationChannel.Email,
        NotificationChannel.Push,
        NotificationChannel.InApp
    },
    Data = new Dictionary<string, string>
    {
        ["SenderId"] = "john-456",
        ["MessageId"] = "msg-789"
    }
};

var result = await _notificationService.SendAsync(notification);
```

### Send with Template
```csharp
var notification = new Notification
{
    UserId = "user-123",
    TemplateCode = "order_confirmed",
    Channels = new List<NotificationChannel>
    {
        NotificationChannel.Email,
        NotificationChannel.SMS
    },
    Data = new Dictionary<string, string>
    {
        ["OrderNumber"] = "ORD-12345",
        ["TotalAmount"] = "1,250.00 SAR",
        ["TrackingLink"] = "https://ACommerce.sa/track/12345"
    }
};

var result = await _notificationService.SendAsync(notification);
```

### Schedule Notification
```csharp
var notification = new Notification
{
    UserId = "user-123",
    Title = "Appointment Reminder",
    Message = "Your appointment is tomorrow at 10:00 AM",
    Channels = new List<NotificationChannel>
    {
        NotificationChannel.SMS,
        NotificationChannel.Push
    },
    ScheduledFor = DateTime.UtcNow.AddHours(23) // Send tomorrow
};

var result = await _notificationService.ScheduleAsync(notification);
```

### Batch Notifications
```csharp
var notifications = new List<Notification>();

foreach (var user in users)
{
    notifications.Add(new Notification
    {
        UserId = user.Id,
        Title = "Newsletter",
        Message = "Check out our latest updates!",
        Channels = new List<NotificationChannel> { NotificationChannel.Email }
    });
}

var results = await _notificationService.SendBatchAsync(notifications);
```

## Template Management

### Create Template
```csharp
var template = new NotificationTemplate
{
    Name = "Order Confirmation",
    Code = "order_confirmed",
    Channel = NotificationChannel.Email,
    Language = "ar-SA",
    Subject = "تأكيد طلبك رقم {{OrderNumber}}",
    Body = @"
        <h1>شكراً لطلبك!</h1>
        <p>رقم الطلب: <strong>{{OrderNumber}}</strong></p>
        <p>المبلغ الإجمالي: <strong>{{TotalAmount}}</strong></p>
        <p><a href='{{TrackingLink}}'>تتبع طلبك</a></p>
    ",
    Variables = new List<string> { "OrderNumber", "TotalAmount", "TrackingLink" },
    IsActive = true
};

await _templateRepository.AddAsync(template);
```

### Use Template

The service automatically:
1. Loads template by code
2. Replaces variables with data
3. Sends via specified channels

## User Preferences

### Set Preferences
```csharp
var preference = new NotificationPreference
{
    UserId = "user-123",
    Channel = NotificationChannel.Email,
    IsEnabled = true,
    QuietHoursStart = new TimeSpan(22, 0, 0), // 10 PM
    QuietHoursEnd = new TimeSpan(8, 0, 0),    // 8 AM
    Frequency = NotificationFrequency.Immediate,
    Categories = new List<NotificationType>
    {
        NotificationType.Info,
        NotificationType.Success,
        NotificationType.Warning
    }
};

await _preferenceRepository.AddAsync(preference);
```

### Check Preferences

The service automatically:
1. Checks if channel enabled for user
2. Respects quiet hours
3. Applies frequency settings
4. Filters by category

## Notification Flow
```
1. Create Notification
   ↓
2. Check User Preferences
   ↓
3. Apply Quiet Hours
   ↓
4. Load Template (if specified)
   ↓
5. Replace Variables
   ↓
6. Queue for Each Channel
   ↓
7. Channel-Specific Delivery
   ↓
8. Track Delivery Status
   ↓
9. Retry on Failure (if configured)
   ↓
10. Mark as Complete
```

## Background Services

### NotificationProcessorService

Processes pending notifications from queue
```csharp
public class NotificationProcessorService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var notifications = await _repository.GetPendingAsync(100);
            
            foreach (var notification in notifications)
            {
                // Check preferences
                var canSend = await _preferenceService.CanSendAsync(
                    notification.UserId, 
                    notification.Channels);
                
                if (!canSend)
                {
                    await _repository.MarkAsSkippedAsync(notification.Id);
                    continue;
                }
                
                // Process each channel
                foreach (var channel in notification.Channels)
                {
                    var channelHandler = _channelFactory.GetChannel(channel);
                    var result = await channelHandler.SendAsync(notification);
                    
                    // Track delivery
                    await _deliveryRepository.AddAsync(new NotificationDelivery
                    {
                        NotificationId = notification.Id,
                        Channel = channel,
                        Status = result.IsSuccess ? DeliveryStatus.Sent : DeliveryStatus.Failed,
                        MessageId = result.MessageId,
                        Error = result.Error,
                        SentAt = DateTime.UtcNow
                    });
                }
                
                await _repository.MarkAsSentAsync(notification.Id);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

### NotificationRetryService

Retries failed notifications
```csharp
public class NotificationRetryService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var failedDeliveries = await _deliveryRepository.GetFailedForRetryAsync();
            
            foreach (var delivery in failedDeliveries)
            {
                if (delivery.RetryCount >= delivery.MaxRetries)
                {
                    await _deliveryRepository.MarkAsFailedPermanentlyAsync(delivery.Id);
                    continue;
                }
                
                var notification = await _repository.GetByIdAsync(delivery.NotificationId);
                var channel = _channelFactory.GetChannel(delivery.Channel);
                
                var result = await channel.SendAsync(notification);
                
                delivery.RetryCount++;
                delivery.Status = result.IsSuccess ? DeliveryStatus.Sent : DeliveryStatus.Failed;
                delivery.Error = result.Error;
                
                await _deliveryRepository.UpdateAsync(delivery);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

## API Endpoints

### Send Notification
```csharp
[HttpPost]
public async Task<ActionResult<NotificationResult>> Send([FromBody] SendNotificationRequest request)
{
    var notification = new Notification
    {
        UserId = request.UserId,
        Title = request.Title,
        Message = request.Message,
        Type = request.Type,
        Priority = request.Priority,
        Channels = request.Channels,
        Data = request.Data
    };
    
    var result = await _notificationService.SendAsync(notification);
    
    return Ok(result);
}
```

### Get User Notifications
```csharp
[HttpGet("my-notifications")]
[Authorize]
public async Task<ActionResult<PagedResult<NotificationDto>>> GetMyNotifications(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    var notifications = await _repository.GetByUserAsync(userId, pageNumber, pageSize);
    
    return Ok(notifications);
}
```

### Mark as Read
```csharp
[HttpPost("{id}/mark-as-read")]
[Authorize]
public async Task<IActionResult> MarkAsRead(Guid id)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    await _notificationService.MarkAsReadAsync(id, userId);
    
    return NoContent();
}
```

## Installation
```bash
dotnet add package ACommerce.Notifications.Core
```

## Dependencies

- ACommerce.Notifications.Abstractions
- SharedKernel.Abstractions
- SharedKernel.CQRS
- MediatR

## License

MIT