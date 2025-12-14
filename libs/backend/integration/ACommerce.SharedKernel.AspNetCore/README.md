# ACommerce.SharedKernel.AspNetCore

ASP.NET Core utilities including base controllers, middleware, filters, and extensions.

## Overview

Essential ASP.NET Core components for building clean APIs. Provides base controllers with built-in CRUD operations, exception handling middleware, validation filters, and common extensions.

## Key Features

🔥 **BaseCrudController** - Complete REST API in one inheritance!  
🔥 **Exception Middleware** - Global exception handling  
✅ **Validation Filters** - Automatic model validation  
✅ **API Versioning** - Version management  
✅ **Swagger Integration** - Auto API documentation  
✅ **CORS Support** - Cross-origin configuration  

## Core Components

### BaseCrudController<TEntity, TCreateDto, TUpdateDto, TDto, TPartialUpdateDto>

Complete CRUD controller with all REST operations

**Inherited Endpoints:**
- `GET /api/resource` - Get all (paginated)
- `GET /api/resource/{id}` - Get by ID
- `POST /api/resource` - Create
- `PUT /api/resource/{id}` - Full update
- `PATCH /api/resource/{id}` - Partial update
- `DELETE /api/resource/{id}` - Delete

**Features:**
- ✅ Automatic pagination
- ✅ Filtering support
- ✅ Sorting support
- ✅ Validation integration
- ✅ Consistent error responses
- ✅ Logging integration

**Usage:**
```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : BaseCrudController
    Product,           // Entity
    CreateProductDto,  // Create DTO
    UpdateProductDto,  // Update DTO
    ProductDto,        // Response DTO
    PatchProductDto>   // Patch DTO
{
    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        : base(mediator, logger)
    {
    }
    
    // All CRUD operations inherited!
    // Add custom endpoints as needed:
    
    [HttpGet("featured")]
    public async Task<ActionResult<List<ProductDto>>> GetFeatured()
    {
        var query = new GetFeaturedProductsQuery();
        var products = await _mediator.Send(query);
        return Ok(products);
    }
}
```

### BaseController

Simpler base without CRUD operations
```csharp
public class CustomController : BaseController
{
    public CustomController(IMediator mediator, ILogger<CustomController> logger)
        : base(mediator, logger)
    {
    }
    
    // Full control over endpoints
}
```

## Middleware

### GlobalExceptionMiddleware

Catches all exceptions and returns consistent error responses

**Features:**
- ✅ Handles all exception types
- ✅ Logs exceptions with context
- ✅ Returns ProblemDetails format
- ✅ Development vs Production modes
- ✅ Custom exception mapping

**Setup:**
```csharp
// In Program.cs
app.UseMiddleware<GlobalExceptionMiddleware>();
```

**Response Format:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "traceId": "00-abc123...",
  "errors": {
    "Name": ["Name is required"],
    "Price": ["Price must be greater than 0"]
  }
}
```

### RequestLoggingMiddleware

Logs all incoming requests
```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

## Action Filters

### ValidateModelStateFilter

Automatic model validation
```csharp
services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelStateFilter>();
});
```

### ApiKeyAuthorizationFilter

API key authentication
```csharp
[ApiKeyAuthorization]
public class SecureController : ControllerBase
{
    // Requires X-API-Key header
}
```

## Extensions

### ServiceCollectionExtensions
```csharp
// Add all SharedKernel services
services.AddSharedKernel(configuration);

// Or add individually:
services.AddSharedKernelCQRS();
services.AddSharedKernelRepositories<YourDbContext>();
services.AddSharedKernelSwagger();
services.AddSharedKernelCors();
```

### ApplicationBuilderExtensions
```csharp
// Use all SharedKernel middleware
app.UseSharedKernel();

// Or individually:
app.UseSharedKernelExceptionHandling();
app.UseSharedKernelCors();
app.UseSharedKernelSwagger();
```

## Configuration

### appsettings.json
```json
{
  "SharedKernel": {
    "Cors": {
      "AllowedOrigins": ["http://localhost:3000"],
      "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
      "AllowedHeaders": ["*"]
    },
    "Swagger": {
      "Title": "My API",
      "Version": "v1",
      "Description": "API Documentation"
    },
    "Logging": {
      "IncludeRequestBody": true,
      "IncludeResponseBody": false
    }
  }
}
```

## Complete API Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add SharedKernel
builder.Services.AddSharedKernel(builder.Configuration);

// Add your services
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Use SharedKernel middleware
app.UseSharedKernel();

// Your middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## API Response Examples

### Success Response
```json
{
  "success": true,
  "data": {
    "id": "123",
    "name": "Product"
  },
  "message": "Product created successfully",
  "statusCode": 201
}
```

### Error Response
```json
{
  "success": false,
  "errors": ["Product not found"],
  "message": "Not Found",
  "statusCode": 404
}
```

### Validation Error
```json
{
  "success": false,
  "errors": [
    "Name is required",
    "Price must be positive"
  ],
  "message": "Validation failed",
  "statusCode": 400
}
```

## Installation
```bash
dotnet add package ACommerce.SharedKernel.AspNetCore
```

## Dependencies

- ACommerce.SharedKernel.Abstractions
- ACommerce.SharedKernel.CQRS
- Microsoft.AspNetCore.App
- Swashbuckle.AspNetCore

## Used By

- All Ashare.*.Api projects

## License

MIT