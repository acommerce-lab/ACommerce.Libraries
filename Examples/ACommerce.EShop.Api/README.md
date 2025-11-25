# 🛒 ACommerce E-Shop API

Complete E-Commerce Backend API built with **ACommerce Libraries v2.0**

## 🎯 Overview

This is a **production-ready** e-commerce backend that demonstrates the power of the completely refactored ACommerce architecture.

## ✨ Features

### 🔐 Authentication & Authorization
- OpenIddict integration
- JWT Bearer authentication
- Role-based access control
- Two-factor authentication support

### 📦 Catalog System (v2.0 - Refactored)
- **Independent Systems** with Separation of Concerns:
  - ✅ Attributes System (dynamic product properties)
  - ✅ Units System (measurement units)
  - ✅ Currency System (multi-currency support)
  - ✅ Products System (uses all above via composition)

### 🛍️ Sales & Orders
- Shopping cart management
- Order processing
- Order tracking
- Order history

### 💳 Payment Processing
- Multiple payment gateways
- Payment methods management
- Transaction tracking

### 🚚 Shipping & Delivery
- Shipping methods
- Shipping providers
- Tracking integration

### 👥 Marketplace
- Multi-vendor support
- Vendor management
- Vendor products

### 💬 Communication
- Real-time chat (SignalR)
- Push notifications
- Email notifications

### 📧 Contact Management
- Customer contact points
- Address management
- Communication preferences

### 📊 User Management
- User profiles
- Profile customization
- User preferences

## 🏗️ Architecture

### Clean Architecture + DDD
- **Domain Layer**: Pure business logic
- **Application Layer**: Use cases (CQRS)
- **Infrastructure Layer**: EF Core, external services
- **Presentation Layer**: API Controllers

### Design Patterns
- ✅ Repository Pattern
- ✅ Unit of Work
- ✅ CQRS (MediatR)
- ✅ Dependency Injection
- ✅ Separation of Concerns

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQLite (included)

### Run the Application

```bash
cd Examples/ACommerce.EShop.Api
dotnet run
```

### Access Swagger UI
Open browser at: **https://localhost:5001**

## 📖 API Documentation

All endpoints are documented in Swagger UI with:
- Request/Response examples
- Authentication requirements
- Validation rules

### Main API Groups

#### Catalog APIs
- `/api/catalog/attributes` - Product attributes
- `/api/catalog/unit-categories` - Unit categories (NEW)
- `/api/catalog/units` - Measurement units
- `/api/catalog/currencies` - Currencies & exchange rates
- `/api/catalog/products` - Product catalog

#### Sales APIs
- `/api/cart` - Shopping cart
- `/api/orders` - Order management

#### Payment APIs
- `/api/payments` - Payment processing

#### Shipping APIs
- `/api/shipping` - Shipping management

#### Vendor APIs
- `/api/vendors` - Vendor marketplace

#### User APIs
- `/api/profiles` - User profiles
- `/api/contact-points` - Contact management

#### Communication APIs
- `/hubs/chat` - Real-time chat (SignalR)
- `/hubs/notifications` - Push notifications (SignalR)

## 🗄️ Database

Uses **SQLite** for simplicity. The database is created automatically on first run.

Database file: `eshop.db`

### Migrations
```bash
# Add migration (if needed)
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

## 🔧 Configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=eshop.db"
  },
  "Authentication": {
    "Issuer": "https://localhost:5001",
    "Audience": "eshop_api"
  }
}
```

## 📊 Statistics

### Backend Code
- **Total Lines**: ~15,000+ lines
- **Projects**: 50+ libraries
- **Controllers**: 30+ API controllers
- **Entities**: 100+ domain entities
- **Configuration Files**: 50+ EF Core configurations

### Architecture Highlights
- ✅ **4 Independent Domain Systems** (Attributes, Units, Currencies, Products)
- ✅ **Zero Circular Dependencies**
- ✅ **Clean Separation of Concerns**
- ✅ **EF Core Compatible** (no cascade path issues)

## 🎉 What Makes This Special?

### Before Refactoring (v1.x)
❌ Currency inherited from Unit
❌ Unit inherited from AttributeDefinition
❌ Circular dependencies everywhere
❌ EF Core migrations failed
❌ Tight coupling

### After Refactoring (v2.0)
✅ Complete Separation of Concerns
✅ Composition over Inheritance
✅ Independent, scalable systems
✅ EF Core works perfectly
✅ Production-ready architecture

## 📝 License

Part of ACommerce Libraries - Multi-Vendor E-Commerce Platform

---

**Built with ❤️ using ACommerce Libraries v2.0**
