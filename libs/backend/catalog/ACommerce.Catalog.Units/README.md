# ACommerce.Catalog.Units

Comprehensive unit of measurement system with conversion support.

## Overview

A complete unit management system supporting multiple measurement categories (Weight, Length, Volume, etc.) with automatic unit conversion capabilities. Built on top of ACommerce.Catalog.Attributes for maximum flexibility.

## Packages

- **ACommerce.Catalog.Units** (Domain)
- **ACommerce.Catalog.Units.Api** (Controllers)

## Key Features

✅ **Multiple Categories** - Weight, Length, Volume, Area, Temperature, Time  
✅ **Measurement Systems** - Metric, Imperial, US Customary  
✅ **Unit Conversion** - Automatic conversion between compatible units  
✅ **Standard Units** - Define base units for each category  
✅ **Composite Units** - Support for complex units (e.g., kg/m²)  
✅ **Inherits Attributes** - Uses attribute system for metadata  

## Domain Entities

### MeasurementCategory
Groups related units (Weight, Length, etc.)

**Properties:**
- Name, Code, CategoryCode
- StandardUnitId
- Description

### MeasurementSystem
Defines measurement systems (Metric, Imperial)

**Properties:**
- Name, Code
- IsDefault
- Description

### Unit
Individual unit of measurement

**Properties:**
- Name, Symbol, Code
- MeasurementCategoryId
- MeasurementSystemId
- ConversionToBase (multiplier to standard unit)
- IsStandard
- Abbreviation

### UnitConversion
Stores conversion rules between units

**Properties:**
- FromUnitId, ToUnitId
- ConversionFactor
- ConversionOffset (for temperature)

### CompositeUnit
Complex units made of multiple units

**Properties:**
- Name, Symbol
- Components (list of units with exponents)

## API Endpoints

### Measurement Categories
- `GET /api/catalog/measurement-categories`
- `POST /api/catalog/measurement-categories`
- `GET /api/catalog/measurement-categories/by-code/{code}`

### Measurement Systems
- `GET /api/catalog/measurement-systems`
- `POST /api/catalog/measurement-systems`
- `GET /api/catalog/measurement-systems/default`

### Units
- `GET /api/catalog/units`
- `POST /api/catalog/units`
- `GET /api/catalog/units/by-category/{categoryId}`
- `GET /api/catalog/units/by-system/{systemId}`
- `GET /api/catalog/units/convert` - Convert between units

### Unit Conversions
- `GET /api/catalog/unit-conversions`
- `POST /api/catalog/unit-conversions`

## Usage Example
```csharp
// 1. Create category
POST /api/catalog/measurement-categories
{
  "name": "Weight",
  "code": "weight",
  "categoryCode": "Mass"
}

// 2. Create standard unit
POST /api/catalog/units
{
  "name": "Kilogram",
  "symbol": "kg",
  "code": "kilogram",
  "categoryId": "cat-id",
  "systemId": "metric-id",
  "conversionToBase": 1,
  "isStandard": true
}

// 3. Create other units
POST /api/catalog/units
{
  "name": "Gram",
  "symbol": "g",
  "conversionToBase": 0.001
}

// 4. Convert
GET /api/catalog/units/convert?value=1000&fromUnitId=gram-id&toUnitId=kg-id
// Returns: 1 kg
```

## Dependencies

- ACommerce.Catalog.Attributes
- SharedKernel.Abstractions
- SharedKernel.CQRS

## Installation
```bash
dotnet add package ACommerce.Catalog.Units
dotnet add package ACommerce.Catalog.Units.Api
```

## License

MIT