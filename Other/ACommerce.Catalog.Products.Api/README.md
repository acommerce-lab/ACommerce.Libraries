# ACommerce.Catalog.Products.Api

Complete ASP.NET Core controllers for product catalog management system.

## Features

✅ **Products Controller** - Full product management with variants  
✅ **Categories Controller** - Hierarchical category system  
✅ **Brands Controller** - Brand management  
✅ **Prices Controller** - Multi-currency, multi-market pricing  
✅ **Inventory Controller** - Stock management with reservations  
✅ **Reviews Controller** - Customer reviews and ratings  
✅ **Complete Integration** - Uses Attributes, Units, and Currencies  

## Installation
```bash
dotnet add package ACommerce.Catalog.Products.Api
```

## Usage

### In your Web API Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Add Controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(ProductsController).Assembly);

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.MapControllers();

app.Run();
```

## Available Endpoints

### Products
- `GET /api/catalog/products/{id}`
- `GET /api/catalog/products/{id}/detail`
- `POST /api/catalog/products`
- `GET /api/catalog/products/by-sku/{sku}`
- `GET /api/catalog/products/by-barcode/{barcode}`
- `GET /api/catalog/products/featured`
- `GET /api/catalog/products/new`
- `POST /api/catalog/products/advanced-search`
- `GET /api/catalog/products/{id}/variants`
- `POST /api/catalog/products/{id}/categories`
- `POST /api/catalog/products/{id}/brands`
- `POST /api/catalog/products/{id}/attributes`

### Categories
- `GET /api/catalog/product-categories/{id}`
- `POST /api/catalog/product-categories`
- `GET /api/catalog/product-categories/by-slug/{slug}`
- `GET /api/catalog/product-categories/root`
- `GET /api/catalog/product-categories/{id}/children`
- `GET /api/catalog/product-categories/{id}/products`

### Inventory
- `GET /api/catalog/product-inventory/{productId}`
- `POST /api/catalog/product-inventory/{productId}/add-stock`
- `POST /api/catalog/product-inventory/{productId}/deduct-stock`
- `POST /api/catalog/product-inventory/{productId}/reserve`
- `POST /api/catalog/product-inventory/{productId}/release`
- `GET /api/catalog/product-inventory/{productId}/check-availability`

### Reviews
- `GET /api/catalog/product-reviews/by-product/{productId}`
- `POST /api/catalog/product-reviews`
- `POST /api/catalog/product-reviews/{id}/approve`

## License

MIT