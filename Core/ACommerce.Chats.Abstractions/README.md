# ACommerce.Chats.Abstractions

Provider-agnostic abstractions and interfaces for chat systems.

## Overview

Lightweight abstractions layer that defines interfaces for chat systems. Allows you to implement any chat backend (Database, Firebase, Twilio, etc.) without changing your application code.

## Key Features

✅ **Provider Agnostic** - Work with any chat backend  
✅ **Lightweight** - Only interfaces and DTOs (~400 lines)  
✅ **Extensible** - Easy to implement custom providers  
✅ **Real-time Ready** - Built-in real-time abstractions  
✅ **Event Driven** - MediatR events for extensibility  

## Core Interfaces

### IChatProvider
Chat management
- CreateChatAsync
- GetChatAsync
- GetUserChatsAsync
- UpdateChatAsync
- DeleteChatAsync
- AddParticipantAsync
- RemoveParticipantAsync
- GetParticipantsAsync

### IMessageProvider
Message management
- SendMessageAsync
- GetMessagesAsync
- GetMessageAsync
- UpdateMessageAsync
- DeleteMessageAsync
- MarkAsReadAsync
- SearchMessagesAsync

### IRealtimeChatProvider
Real-time delivery
- SendMessageToChat
- SendTypingIndicator
- SendParticipantJoined
- SendParticipantLeft
- SendUserPresenceUpdate
- SendMessageRead

### IPresenceProvider
User presence
- UpdateUserPresenceAsync
- GetUserPresenceAsync
- GetUsersPresenceAsync

## DTOs

### ChatDto
- Id, Title, Type (Direct, Group, Channel)
- ParticipantsCount, UnreadMessagesCount
- LastMessage, CreatedAt

### MessageDto
- Id, ChatId, SenderId
- Content, Type (Text, Image, File, Voice, Video)
- ReplyToMessageId, Attachments
- IsEdited, ReadByCount, CreatedAt

### ParticipantDto
- Id, ChatId, UserId
- Role (Owner, Admin, Member, Guest)
- IsOnline, LastSeenAt
- UnreadMessagesCount, IsMuted, IsPinned

## Events

### ChatCreatedEvent
Fired when a chat is created

### MessageSentEvent
Fired when a message is sent

### ParticipantJoinedEvent
Fired when a participant joins

### MessageReadEvent
Fired when a message is read

### And more...

## Usage

Implement your own provider:
```csharp
public class MyCustomChatProvider : IChatProvider
{
    public async Task<ChatDto> CreateChatAsync(
        CreateChatDto dto, 
        CancellationToken ct)
    {
        // Your implementation here
        // Could be: Firebase, Azure, Database, etc.
    }
    
    // Implement other methods...
}
```

Register in DI:
```csharp
services.AddScoped<IChatProvider, MyCustomChatProvider>();
```

## Available Implementations

- **ACommerce.Chats.Core** - Database implementation (default)
- **ACommerce.Chats.Providers.Firebase** - Firebase backend
- **ACommerce.Chats.Providers.Azure** - Azure Communication Services
- *(Add your own!)*

## Installation
```bash
dotnet add package ACommerce.Chats.Abstractions
```

## Dependencies

- SharedKernel.Abstractions
- MediatR

## License

MIT