# ACommerce.Transactions.Core.Api

ASP.NET Core controllers for transaction and document management system.

## Features

✅ **Document Types Management** - Configure document types with custom attributes  
✅ **Operations Management** - Define available operations per document type  
✅ **Notifications System** - Configure notifications per operation  
✅ **Relations System** - Sequential and compositional document relations  
✅ **Built on Catalog** - Uses Attributes, Units, and Currencies from catalog  

## Installation
```bash
dotnet add package ACommerce.Transactions.Core.Api
```

## Usage

### In your Web API Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(DocumentTypesController).Assembly);

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.MapControllers();

app.Run();
```

## Available Endpoints

### Document Types
- `GET /api/transactions/document-types`
- `POST /api/transactions/document-types`
- `GET /api/transactions/document-types/{id}`
- `GET /api/transactions/document-types/by-code/{code}`
- `GET /api/transactions/document-types/by-category/{category}`
- `GET /api/transactions/document-types/{id}/operations`
- `GET /api/transactions/document-types/{id}/attributes`

### Document Operations
- `GET /api/transactions/document-operations`
- `POST /api/transactions/document-operations`
- `GET /api/transactions/document-operations/by-document-type/{documentTypeId}`
- `GET /api/transactions/document-operations/by-operation-type/{operationType}`

### Operation Notifications
- `GET /api/transactions/operation-notifications`
- `POST /api/transactions/operation-notifications`
- `GET /api/transactions/operation-notifications/by-type/{type}`

### Document Type Relations
- `GET /api/transactions/document-type-relations`
- `POST /api/transactions/document-type-relations`
- `GET /api/transactions/document-type-relations/by-source/{sourceId}`
- `GET /api/transactions/document-type-relations/by-target/{targetId}`

## License

MIT