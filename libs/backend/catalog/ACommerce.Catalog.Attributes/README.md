# ACommerce.Catalog.Attributes

Flexible attribute system for dynamic properties on any entity.

## Overview

This library provides a powerful and flexible attribute system that allows you to define custom properties dynamically without modifying database schemas. Perfect for e-commerce catalogs, product specifications, and any scenario requiring dynamic metadata.

## Packages

- **ACommerce.Catalog.Attributes** (Domain)
- **ACommerce.Catalog.Attributes.Api** (Controllers)

## Key Features

✅ **Dynamic Attributes** - Define custom properties at runtime  
✅ **Multiple Types** - Text, Number, Boolean, Date, SingleSelect, MultiSelect  
✅ **Predefined Values** - Create dropdown lists with AttributeValue  
✅ **Validation Rules** - Min/Max values, Required fields  
✅ **Sorting & Display** - Control order and visibility  
✅ **CRUD Operations** - Full REST API via BaseCrudController  

## Domain Entities

### AttributeDefinition
Defines the structure of an attribute (e.g., "Color", "Size", "Brand")

**Properties:**
- Name, Code, Type
- IsRequired, IsSearchable, IsFilterable
- MinValue, MaxValue
- SortOrder, DisplayOrder
- Metadata

### AttributeValue
Predefined values for SingleSelect/MultiSelect attributes

**Properties:**
- AttributeDefinitionId
- Value, Code
- ColorHex, ImageUrl
- IsDefault, SortOrder

### AttributeMapping
Maps attributes to entities (products, categories, etc.)

**Properties:**
- AttributeDefinitionId
- AttributeValueId (for predefined values)
- CustomValue (for free-text input)
- EntityId, EntityType

## API Endpoints

### Attribute Definitions
- `GET /api/catalog/attribute-definitions`
- `POST /api/catalog/attribute-definitions`
- `GET /api/catalog/attribute-definitions/{id}`
- `PUT /api/catalog/attribute-definitions/{id}`
- `DELETE /api/catalog/attribute-definitions/{id}`
- `GET /api/catalog/attribute-definitions/by-code/{code}`
- `GET /api/catalog/attribute-definitions/by-type/{type}`

### Attribute Values
- `GET /api/catalog/attribute-values`
- `POST /api/catalog/attribute-values`
- `GET /api/catalog/attribute-values/by-definition/{definitionId}`

## Usage Example
```csharp
// 1. Create attribute definition
POST /api/catalog/attribute-definitions
{
  "name": "Color",
  "code": "color",
  "type": "SingleSelect",
  "isRequired": true,
  "isFilterable": true
}

// 2. Add predefined values
POST /api/catalog/attribute-values
{
  "attributeDefinitionId": "attr-id",
  "value": "Red",
  "code": "red",
  "colorHex": "#FF0000"
}

// 3. Map to entity
POST /api/catalog/attribute-mappings
{
  "attributeDefinitionId": "attr-id",
  "attributeValueId": "value-id",
  "entityId": "product-id",
  "entityType": "Product"
}
```

## Dependencies

- SharedKernel.Abstractions
- SharedKernel.CQRS
- SharedKernel.AspNetCore
- MediatR
- FluentValidation

## Installation
```bash
dotnet add package ACommerce.Catalog.Attributes
dotnet add package ACommerce.Catalog.Attributes.Api
```

## Integration
```csharp
// Add to your API
builder.Services.AddControllers()
    .AddApplicationPart(typeof(AttributeDefinitionsController).Assembly);
```

## License

MIT