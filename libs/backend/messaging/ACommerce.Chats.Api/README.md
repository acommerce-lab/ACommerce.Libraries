# ACommerce.Chats.Api

Ultra-lean ASP.NET Core controllers for chat system - powered by SharedKernel!

## Overview

Complete REST API for chat system with minimal code. Inherits most CRUD operations from BaseCrudController and adds custom chat-specific endpoints.

## Key Features

🔥 **BaseCrudController** - All CRUD operations inherited!  
✅ **Real-time Ready** - Integrated with ACommerce.Realtime  
✅ **Authorization** - Built-in user authentication  
✅ **Message Management** - Send, edit, delete, search  
✅ **Participant Management** - Add, remove participants  
✅ **Statistics** - User and chat statistics  
✅ **Ultra Lean** - Only ~400 lines of code!  

## Controllers

### ChatsController
Inherits from BaseCrudController<Chat, ...>

**Inherited Endpoints:**
- GET /api/chats
- POST /api/chats
- GET /api/chats/{id}
- PUT /api/chats/{id}
- PATCH /api/chats/{id}
- DELETE /api/chats/{id}

**Custom Endpoints:**
- GET /api/chats/my-chats
- POST /api/chats/{chatId}/messages
- GET /api/chats/{chatId}/messages
- GET /api/chats/{chatId}/messages/{messageId}
- PUT /api/chats/{chatId}/messages/{messageId}
- DELETE /api/chats/{chatId}/messages/{messageId}
- POST /api/chats/{chatId}/mark-as-read
- GET /api/chats/{chatId}/messages/search
- POST /api/chats/{chatId}/participants
- DELETE /api/chats/{chatId}/participants/{userId}
- GET /api/chats/{chatId}/participants

### ChatStatsController
Statistics endpoints

**Endpoints:**
- GET /api/chats/stats/my-stats
- GET /api/chats/stats/{chatId}

## Installation
```bash
dotnet add package ACommerce.Chats.Api
```

## Dependencies

- ACommerce.Chats.Core
- ACommerce.Chats.Abstractions
- SharedKernel.AspNetCore

## Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddScoped<IChatProvider, DatabaseChatProvider>();
builder.Services.AddScoped<IMessageProvider, DatabaseMessageProvider>();

// Add controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(ChatsController).Assembly);

// Add SignalR
builder.Services.AddSignalR();

// Add Authentication
builder.Services.AddAuthentication(/* ... */);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Map ChatHub
app.MapHub<ChatHub>("/hubs/chat");

app.MapControllers();
app.Run();
```

## API Examples

### Get My Chats
```bash
GET /api/chats/my-chats?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

### Create Chat
```bash
POST /api/chats
Authorization: Bearer {token}
{
  "title": "Team Chat",
  "type": "Group",
  "creatorUserId": "user-1",
  "participantUserIds": ["user-1", "user-2", "user-3"]
}
```

### Send Message
```bash
POST /api/chats/{chatId}/messages
Authorization: Bearer {token}
{
  "content": "Hello team!",
  "type": "Text"
}
```

### Get Messages
```bash
GET /api/chats/{chatId}/messages?pageNumber=1&pageSize=50
Authorization: Bearer {token}
```

### Mark as Read
```bash
POST /api/chats/{chatId}/mark-as-read
Authorization: Bearer {token}
{
  "lastMessageId": "message-guid"
}
```

### Search Messages
```bash
GET /api/chats/{chatId}/messages/search?query=hello&pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

### Add Participant
```bash
POST /api/chats/{chatId}/participants
Authorization: Bearer {token}
{
  "userId": "user-4",
  "role": "Member"
}
```

## Real-time Events

Connect to SignalR hub at `/hubs/chat`:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .build();

// Listen for events
connection.on("MessageReceived", (message) => {
    console.log("New message:", message);
});

connection.on("UserTyping", (indicator) => {
    console.log("User typing:", indicator.userId);
});

connection.on("ParticipantJoined", (participant) => {
    console.log("User joined:", participant.userId);
});

connection.on("UserPresenceChanged", (presence) => {
    console.log("Presence:", presence.userId, presence.isOnline);
});

// Join chat
await connection.invoke("JoinChat", chatId);

// Send typing indicator
await connection.invoke("SendTypingIndicator", chatId, true);
```

## Authorization

All endpoints require authentication. The API extracts userId from JWT token claims:
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

## Statistics

### User Stats
```bash
GET /api/chats/stats/my-stats
```

Response:
```json
{
  "totalChats": 15,
  "totalMessagesSent": 342,
  "unreadMessagesCount": 8
}
```

### Chat Stats
```bash
GET /api/chats/stats/{chatId}
```

Response:
```json
{
  "chatId": "...",
  "participantsCount": 5,
  "totalMessages": 156,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

## License

MIT