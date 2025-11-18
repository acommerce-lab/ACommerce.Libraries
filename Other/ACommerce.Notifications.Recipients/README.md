# ACommerce.Notifications.Recipients

Domain and application layer for recipient management in ACommerce notification system.

## Features

✅ **User Recipients** - Manage notification recipients  
✅ **Contact Points** - Email, Phone, Push, WhatsApp, Telegram  
✅ **Recipient Groups** - Organize users into groups  
✅ **Multi-language Support** - Preferred language per user  
✅ **Time Zone Support** - Time zone per user  
✅ **Validation** - FluentValidation for all DTOs  
✅ **Clean Architecture** - Domain-driven design  

## Installation
```bash
dotnet add package ACommerce.Notifications.Recipients
```

## Entities

### UserRecipient
- UserId (unique identifier)
- FullName
- PreferredLanguage (ISO 639-1)
- TimeZone (IANA)
- ContactPoints (collection)
- Groups (collection)
- Metadata (extensible)

### ContactPoint
- Type (Email, Phone, DeviceToken, WhatsApp, Telegram, InApp)
- Value
- IsVerified
- IsPrimary
- Metadata

### RecipientGroup
- Name
- Description
- Members (collection of UserRecipients)
- Metadata

## Usage

See API project for controller implementations.

## License

MIT