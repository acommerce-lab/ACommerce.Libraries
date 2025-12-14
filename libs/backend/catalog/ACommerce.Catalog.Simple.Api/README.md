# ACommerce.Catalog.Simple.Api

Simplified catalog API for quick setup - perfect for simple applications and beginners.

## Overview

A streamlined interface that wraps the complete catalog system into easy-to-use endpoints. Perfect for developers who need basic catalog functionality without dealing with the full complexity of the underlying system.

## Key Features

🔥 **3 Simple Controllers** - Categories, Products, Setup  
🔥 **Tree Structure** - Category → Attributes → Values  
🔥 **Auto-Setup** - One-click initialization  
🔥 **Single Unit & Currency** - No complex configurations  
🔥 **Beginner Friendly** - Easy to understand and use  
🔥 **Production Ready** - Built on robust catalog system  

## Quick Start

### 1. Auto Setup (One-time)
```bash
POST /api/simple-catalog/setup/auto
{}
```

Response:
```json
{
  "isSuccess": true,
  "message": "Setup completed successfully",
  "currency": {
    "id": "...",
    "code": "SAR",
    "symbol": "﷼"
  },
  "unit": {
    "id": "...",
    "name": "Piece",
    "symbol": "pc"
  }
}
```

### 2. Create Category with Attributes
```bash
POST /api/simple-catalog/categories
{
  "name": "T-Shirts",
  "description": "Cotton t-shirts",
  "attributes": [
    {
      "name": "Size",
      "values": ["Small", "Medium", "Large", "XL"]
    },
    {
      "name": "Color",
      "values": ["Red", "Blue", "Green"]
    }
  ]
}
```

### 3. Create Product
```bash
POST /api/simple-catalog/products
{
  "name": "Cotton T-Shirt",
  "sku": "TS-001",
  "description": "Comfortable cotton t-shirt",
  "price": 99.99,
  "salePrice": 79.99,
  "categoryId": "category-guid",
  "image": "https://...",
  "stock": 100,
  "isFeatured": true,
  "attributes": {
    "Size": "Large",
    "Color": "Blue"
  }
}
```

### 4. Get Products
```bash
GET /api/simple-catalog/products
GET /api/simple-catalog/products?categoryId=...
GET /api/simple-catalog/products?inStockOnly=true
GET /api/simple-catalog/products?featuredOnly=true
```

## API Endpoints

### Setup
- `POST /api/simple-catalog/setup/auto` - Auto setup
- `GET /api/simple-catalog/setup/check` - Check setup status

### Categories
- `GET /api/simple-catalog/categories` - Get all with attributes
- `GET /api/simple-catalog/categories/{id}` - Get single
- `POST /api/simple-catalog/categories` - Create with attributes
- `POST /api/simple-catalog/categories/{id}/attributes` - Add attribute
- `POST /api/simple-catalog/attributes/{id}/values` - Add value

### Products
- `GET /api/simple-catalog/products` - Get all
- `GET /api/simple-catalog/products/{id}` - Get single
- `POST /api/simple-catalog/products` - Create
- `PUT /api/simple-catalog/products/{id}` - Update
- `DELETE /api/simple-catalog/products/{id}` - Delete

## Data Structure
```
Category
  ├── Attribute 1
  │     ├── Value 1
  │     ├── Value 2
  │     └── Value 3
  └── Attribute 2
        ├── Value A
        └── Value B

Product
  ├── Basic Info (name, sku, price)
  ├── Single Category
  ├── Single Unit (Piece)
  ├── Single Currency (SAR)
  └── Attributes (key-value pairs)
```

## Use Cases

✅ Small e-commerce websites  
✅ Internal inventory systems  
✅ Prototype and MVP development  
✅ Learning and tutorials  
✅ Simple product catalogs  

## Migration Path

When you outgrow the simple API, easily migrate to the full catalog system:
```csharp
// Simple API (current)
POST /api/simple-catalog/products

// Full API (when needed)
POST /api/catalog/products
POST /api/catalog/product-prices
POST /api/catalog/product-inventory
```

All data is compatible - no migration needed!

## Dependencies

- ACommerce.Catalog.Products
- ACommerce.Catalog.Attributes
- ACommerce.Catalog.Units
- ACommerce.Catalog.Currencies

## Installation
```bash
dotnet add package ACommerce.Catalog.Simple.Api
```

## License

MIT