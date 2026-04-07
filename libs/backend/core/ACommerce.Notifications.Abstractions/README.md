# ACommerce.Notifications.Abstractions

Multi-channel notification system abstractions with flexible routing and templates.

## Overview

Comprehensive notification abstractions supporting multiple delivery channels (Email, SMS, Push, InApp, WhatsApp, Webhook). Provider-agnostic design allows easy integration with any notification service.

## Key Features

✅ **Multi-Channel** - Email, SMS, Push, InApp, WhatsApp, Webhook  
✅ **Provider Agnostic** - Interface-based design  
✅ **Templates** - Reusable notification templates  
✅ **Priority Levels** - Low, Normal, High, Critical  
✅ **Scheduling** - Send now or schedule for later  
✅ **Batch Operations** - Send multiple notifications  
✅ **Event Driven** - Publish/Subscribe pattern  

## Core Interfaces

### INotificationService
Main notification service

**Methods:**
- `SendAsync(notification)` - Send single notification
- `SendBatchAsync(notifications)` - Send multiple notifications
- `ScheduleAsync(notification)` - Schedule for later delivery

### INotificationChannel
Channel-specific delivery

**Properties:**
- `Channel` - Channel type (Email, SMS, etc.)

**Methods:**
- `SendAsync(notification)` - Send via this channel
- `ValidateAsync(notification)` - Validate before sending

### INotificationPublisher
Message queue publisher for microservices

**Methods:**
- `PublishAsync(notificationEvent)` - Publish to queue
- `PublishBatchAsync(notificationEvents)` - Publish multiple

## Domain Models

### Notification
Main notification model

**Properties:**
- UserId, Title, Message
- Type (Info, Success, Warning, Error, SystemAlert)
- Priority (Low, Normal, High, Critical)
- Channels (Email, SMS, Push, InApp, etc.)
- Data (custom key-value pairs)
- ScheduledFor (optional scheduling)

### NotificationTemplate
Reusable templates

**Properties:**
- Name, Code
- Subject, Body
- Channel, Language
- Variables (placeholders)
- IsActive

### NotificationResult
Delivery result

**Properties:**
- IsSuccess
- Channel
- MessageId (from provider)
- Error, ErrorCode
- SentAt, DeliveredAt
- Metadata

### NotificationEvent
For message queue publishing

**Properties:**
- NotificationId
- Notification
- PublishedAt

## Notification Types

- **Info** - General information
- **Success** - Success messages
- **Warning** - Warning alerts
- **Error** - Error notifications
- **SystemAlert** - Critical system alerts

## Notification Channels

- **Email** - Email delivery
- **SMS** - SMS/Text messages
- **Push** - Mobile push notifications
- **InApp** - In-app notifications
- **WhatsApp** - WhatsApp messages
- **Webhook** - HTTP webhooks

## Priority Levels

- **Low** - Can be delayed
- **Normal** - Standard delivery
- **High** - Prioritized delivery
- **Critical** - Immediate delivery

## Usage Example
```csharp
// Create notification
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
        ["MessageId"] = "msg-456",
        ["SenderName"] = "John Doe"
    }
};

// Send notification
var result = await _notificationService.SendAsync(notification);

// Check result
if (result.IsSuccess)
{
    Console.WriteLine($"Sent via {result.Channel}: {result.MessageId}");
}
```

## Template Example
```csharp
var template = new NotificationTemplate
{
    Name = "Welcome Email",
    Code = "welcome_email",
    Channel = NotificationChannel.Email,
    Subject = "Welcome to {{AppName}}, {{UserName}}!",
    Body = "Dear {{UserName}},\n\nWelcome to {{AppName}}...",
    Variables = new List<string> { "AppName", "UserName" }
};

// Use template
var notification = new Notification
{
    UserId = "user-123",
    TemplateCode = "welcome_email",
    Data = new Dictionary<string, string>
    {
        ["AppName"] = "ACommerce",
        ["UserName"] = "Ahmed"
    }
};
```

## Available Implementations

- **ACommerce.Notifications.Channels.Email** - Email via SMTP/SendGrid/AWS SES
- **ACommerce.Notifications.Channels.SMS** - SMS via Twilio/AWS SNS
- **ACommerce.Notifications.Channels.Push** - Push via Firebase/OneSignal
- **ACommerce.Notifications.Channels.InApp** - InApp via SignalR
- **ACommerce.Notifications.Channels.WhatsApp** - WhatsApp via Twilio
- **ACommerce.Notifications.Channels.Webhook** - HTTP webhooks

## Installation
```bash
dotnet add package ACommerce.Notifications.Abstractions
```

## Dependencies

- None - Pure abstractions

## License

MIT