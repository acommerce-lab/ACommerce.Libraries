# ACommerce.Catalog.Attributes.Api

ASP.NET Core controllers for ACommerce catalog attributes system.

## Features

✅ **Attribute Definitions Controller** - Manage attribute definitions  
✅ **Attribute Values Controller** - Manage attribute values  
✅ **Relationships Controller** - Manage value relationships  
✅ **Cross-Attribute Constraints Controller** - Manage constraints  
✅ **Full CRUD operations**  
✅ **Advanced search and filtering**  

## Installation
```bash
dotnet add package ACommerce.Catalog.Attributes.Api
```

## Usage

### In your Web API Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(AttributeDefinitionsController).Assembly);

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.MapControllers();

app.Run();
```

## Available Endpoints

### Attribute Definitions
- `GET /api/catalog/attribute-definitions/{id}`
- `POST /api/catalog/attribute-definitions`
- `GET /api/catalog/attribute-definitions/by-code/{code}`
- `GET /api/catalog/attribute-definitions/filterable`
- `GET /api/catalog/attribute-definitions/{id}/values`
- `POST /api/catalog/attribute-definitions/{id}/values`

### Attribute Values
- `GET /api/catalog/attribute-values/{id}`
- `POST /api/catalog/attribute-values`
- `GET /api/catalog/attribute-values/active`
- `GET /api/catalog/attribute-values/search-by-value`
- `PATCH /api/catalog/attribute-values/{id}/toggle-active`

### Relationships & Constraints
- `POST /api/catalog/attribute-relationships`
- `GET /api/catalog/attribute-relationships/parent/{id}/children`
- `POST /api/catalog/cross-attribute-constraints`
- `POST /api/catalog/cross-attribute-constraints/validate`

## License

MIT