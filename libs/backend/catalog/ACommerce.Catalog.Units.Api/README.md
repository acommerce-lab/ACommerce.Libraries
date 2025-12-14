# ACommerce.Catalog.Units.Api

ASP.NET Core controllers for ACommerce units and measurement system.

## Features

✅ **Measurement Systems Controller** - Manage measurement systems (Metric, Imperial)  
✅ **Measurement Categories Controller** - Manage categories (Weight, Length, Volume)  
✅ **Units Controller** - Manage units (kg, g, m, cm, etc.)  
✅ **Unit Conversions Controller** - Convert between units  
✅ **Extends Attributes System** - Reuses attribute infrastructure  
✅ **Smart Conversion Service** - Automatic conversions via base unit  

## Installation
```bash
dotnet add package ACommerce.Catalog.Units.Api
```

## Usage

### In your Web API Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Unit Conversion Service
builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();

// Add Controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(UnitsController).Assembly);

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.MapControllers();

app.Run();
```

## Available Endpoints

### Measurement Systems
- `GET /api/catalog/measurement-systems/{id}`
- `POST /api/catalog/measurement-systems`
- `GET /api/catalog/measurement-systems/by-code/{code}`
- `GET /api/catalog/measurement-systems/default`
- `GET /api/catalog/measurement-systems/{id}/units`
- `POST /api/catalog/measurement-systems/{id}/set-default`

### Measurement Categories
- `GET /api/catalog/measurement-categories/{id}`
- `POST /api/catalog/measurement-categories`
- `GET /api/catalog/measurement-categories/by-code/{code}`
- `GET /api/catalog/measurement-categories/{id}/units`
- `GET /api/catalog/measurement-categories/{id}/base-unit`

### Units
- `GET /api/catalog/units/{id}`
- `POST /api/catalog/units`
- `GET /api/catalog/units/by-symbol/{symbol}`
- `GET /api/catalog/units/by-code/{code}`
- `GET /api/catalog/units/standard`
- `GET /api/catalog/units/{id}/compatible`

### Conversions
- `POST /api/catalog/unit-conversions/convert`
- `GET /api/catalog/unit-conversions/factor`
- `GET /api/catalog/unit-conversions/can-convert`
- `POST /api/catalog/unit-conversions/batch-convert`

## Example: Convert Units
```bash
POST /api/catalog/unit-conversions/convert
{
  "value": 5,
  "fromUnitId": "kilogram-guid",
  "toUnitId": "pound-guid"
}

Response:
{
  "originalValue": 5,
  "fromUnit": "Kilogram",
  "fromSymbol": "kg",
  "convertedValue": 11.0231,
  "toUnit": "Pound",
  "toSymbol": "lb",
  "conversionFactor": 2.20462,
  "formula": "1 kg = 2.20462 lb"
}
```

## License

MIT