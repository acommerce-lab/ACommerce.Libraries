# ACommerce.Catalog.Products

Complete product catalog system with variants, inventory, pricing, and reviews.

## Overview

Enterprise-grade product management system supporting simple and variable products, multi-currency pricing, inventory tracking, categories, brands, and customer reviews. Integrates seamlessly with Attributes, Units, and Currencies libraries.

## Packages

- **ACommerce.Catalog.Products** (Domain)
- **ACommerce.Catalog.Products.Api** (Controllers)

## Key Features

✅ **Product Types** - Simple, Variable, Grouped, Bundle  
✅ **Product Variants** - Size, Color, and custom variations  
✅ **Multi-Currency Pricing** - Different prices per currency/market  
✅ **Inventory Management** - Stock tracking with reservations  
✅ **Categories & Brands** - Hierarchical organization  
✅ **Customer Reviews** - Ratings and feedback  
✅ **Product Relations** - Related, cross-sell, up-sell  
✅ **Custom Attributes** - Dynamic product specifications  

## Domain Entities

### Product
Main product entity

**Properties:**
- Name, Sku, Barcode
- Type (Simple, Variable, Grouped, Bundle)
- Status (Draft, Active, Inactive, OutOfStock)
- ShortDescription, LongDescription
- Images, FeaturedImage
- Weight, Dimensions
- IsFeatured, IsNew
- ParentProductId (for variants)
- Metadata

### ProductCategory
Category definition with hierarchy

**Properties:**
- Name, Slug, Description
- ParentCategoryId
- Image, Icon
- SortOrder, IsActive

### ProductBrand
Brand definition

**Properties:**
- Name, Slug, Description
- Logo, Website, Country
- SortOrder, IsActive

### ProductPrice
Multi-currency pricing

**Properties:**
- ProductId, CurrencyId
- BasePrice, SalePrice
- DiscountPercentage
- SaleStartDate, SaleEndDate
- Market, CustomerSegment
- MinQuantity, MaxQuantity

### ProductInventory
Stock management

**Properties:**
- ProductId
- QuantityInStock, QuantityReserved
- LowStockThreshold
- TrackInventory, AllowBackorder
- Status (InStock, LowStock, OutOfStock)
- Warehouse, Location

### ProductAttribute
Product specifications

**Properties:**
- ProductId
- AttributeDefinitionId
- AttributeValueId (for predefined)
- CustomValue (for free-text)
- IsVariant (for product variations)

### ProductReview
Customer reviews

**Properties:**
- ProductId, UserId
- Rating (1-5), Title, Comment
- IsRecommended, IsVerifiedPurchase
- IsApproved
- HelpfulVotes, UnhelpfulVotes
- Images

### ProductRelation
Related products

**Properties:**
- SourceProductId
- RelatedProductId
- RelationType (related, cross-sell, up-sell)
- SortOrder

## API Endpoints

### Products
- `GET /api/catalog/products`
- `POST /api/catalog/products`
- `GET /api/catalog/products/{id}`
- `GET /api/catalog/products/{id}/detail` - Full details
- `PUT /api/catalog/products/{id}`
- `DELETE /api/catalog/products/{id}`
- `GET /api/catalog/products/by-sku/{sku}`
- `GET /api/catalog/products/by-barcode/{barcode}`
- `GET /api/catalog/products/featured`
- `GET /api/catalog/products/new`
- `GET /api/catalog/products/by-status/{status}`
- `GET /api/catalog/products/{id}/variants`
- `POST /api/catalog/products/advanced-search`

### Categories
- `GET /api/catalog/product-categories`
- `POST /api/catalog/product-categories`
- `GET /api/catalog/product-categories/by-slug/{slug}`
- `GET /api/catalog/product-categories/root`
- `GET /api/catalog/product-categories/{id}/children`
- `GET /api/catalog/product-categories/{id}/products`

### Brands
- `GET /api/catalog/product-brands`
- `POST /api/catalog/product-brands`
- `GET /api/catalog/product-brands/by-slug/{slug}`
- `GET /api/catalog/product-brands/by-country/{country}`

### Prices
- `GET /api/catalog/product-prices`
- `POST /api/catalog/product-prices`
- `GET /api/catalog/product-prices/effective` - Get effective price

### Inventory
- `GET /api/catalog/product-inventory/{productId}`
- `POST /api/catalog/product-inventory/{productId}/add-stock`
- `POST /api/catalog/product-inventory/{productId}/deduct-stock`
- `POST /api/catalog/product-inventory/{productId}/reserve`
- `POST /api/catalog/product-inventory/{productId}/release`
- `GET /api/catalog/product-inventory/{productId}/check-availability`

### Reviews
- `GET /api/catalog/product-reviews`
- `POST /api/catalog/product-reviews`
- `GET /api/catalog/product-reviews/by-product/{productId}`
- `POST /api/catalog/product-reviews/{id}/approve`
- `POST /api/catalog/product-reviews/{id}/vote-helpful`

## Usage Example
```csharp
// 1. Create product
POST /api/catalog/products
{
  "name": "Cotton T-Shirt",
  "sku": "TS-001",
  "type": "Variable",
  "status": "Active",
  "shortDescription": "Comfortable cotton t-shirt",
  "images": ["url1", "url2"]
}

// 2. Add category
POST /api/catalog/products/{id}/categories
{
  "categoryId": "cat-id",
  "isPrimary": true
}

// 3. Add price
POST /api/catalog/product-prices
{
  "productId": "prod-id",
  "currencyId": "sar-id",
  "basePrice": 99.99,
  "salePrice": 79.99
}

// 4. Set inventory
POST /api/catalog/product-inventory/{id}/add-stock
{
  "quantity": 100,
  "warehouse": "Main"
}

// 5. Create variant
POST /api/catalog/products
{
  "name": "Cotton T-Shirt - Large",
  "sku": "TS-001-L",
  "type": "Simple",
  "parentProductId": "parent-id"
}
```

## Dependencies

- ACommerce.Catalog.Attributes
- ACommerce.Catalog.Units
- ACommerce.Catalog.Currencies
- SharedKernel.Abstractions
- SharedKernel.CQRS

## Installation
```bash
dotnet add package ACommerce.Catalog.Products
dotnet add package ACommerce.Catalog.Products.Api
```

## License

MIT