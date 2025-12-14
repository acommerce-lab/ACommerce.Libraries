# ACommerce.Notifications.Recipients.Api

ASP.NET Core controllers for ACommerce recipient management.

## Features

✅ **Recipients Controller** - Complete CRUD + custom endpoints  
✅ **Contact Points Controller** - Manage contact points  
✅ **Recipient Groups Controller** - Manage groups  
✅ **Swagger Documentation** - Full OpenAPI support  
✅ **Validation** - Automatic FluentValidation  
✅ **Error Handling** - Global exception middleware  

## Installation
```bash
dotnet add package ACommerce.Notifications.Recipients.Api
```

## Usage

### In your Web API Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(RecipientsController).Assembly);

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.MapControllers();

app.Run();
```

## Available Endpoints

### Recipients
- `GET /api/notifications/recipients/{id}`
- `POST /api/notifications/recipients/search`
- `POST /api/notifications/recipients`
- `PUT /api/notifications/recipients/{id}`
- `PATCH /api/notifications/recipients/{id}`
- `DELETE /api/notifications/recipients/{id}`
- `GET /api/notifications/recipients/by-user/{userId}`
- `GET /api/notifications/recipients/active`
- `POST /api/notifications/recipients/{id}/contact-points`
- `POST /api/notifications/recipients/{recipientId}/groups/{groupId}`

### Contact Points
- `GET /api/notifications/contact-points/{id}`
- `POST /api/notifications/contact-points`
- `GET /api/notifications/contact-points/user/{userId}`
- `GET /api/notifications/contact-points/user/{userId}/type/{type}`
- `POST /api/notifications/contact-points/{id}/verify`
- `POST /api/notifications/contact-points/{id}/set-primary`

### Recipient Groups
- `GET /api/notifications/recipient-groups/{id}`
- `POST /api/notifications/recipient-groups`
- `GET /api/notifications/recipient-groups/active`
- `GET /api/notifications/recipient-groups/by-name/{name}`
- `POST /api/notifications/recipient-groups/{groupId}/members/{recipientId}`

## License

MIT