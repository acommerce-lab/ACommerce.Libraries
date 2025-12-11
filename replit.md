# ACommerce.Libraries

## Overview

ACommerce.Libraries is a comprehensive .NET 9.0 ecosystem for building multi-vendor e-commerce platforms with minimal code. The project demonstrates a revolutionary approach where a complete marketplace backend can be created in approximately 84 lines of code by leveraging 97+ pre-built, modular libraries.

The system follows clean architecture principles with CQRS pattern, repository pattern, and provider pattern throughout. It supports multi-tenancy, dynamic attributes, real-time communication via SignalR, and integrates with Saudi-specific services like Nafath authentication and Moyasar payments.

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Core Foundation

The architecture is built on a shared kernel that provides:

- **Entity System**: All entities inherit from `IBaseEntity` providing `Id`, `CreatedAt`, `UpdatedAt`, and `IsDeleted` for soft deletion
- **Repository Pattern**: Generic `IBaseAsyncRepository<T>` with full CRUD operations, filtering, and pagination
- **CQRS Implementation**: Commands and queries separated using MediatR, with base classes for handlers and validators
- **Auto-Discovery DbContext**: The `ApplicationDbContext` automatically discovers and registers all entities from loaded assemblies that implement `IBaseEntity`, eliminating manual entity configuration

### Authentication & Authorization

Multi-provider authentication system supporting:

- **JWT Bearer Authentication**: Primary authentication method with token generation, validation, and refresh capabilities
- **OpenIddict Integration**: OAuth 2.0 and OpenID Connect flows for identity server scenarios
- **Two-Factor Authentication**: Supports SMS, Email, and Nafath (Saudi National SSO)
- **Nafath Integration**: Complete webhook handling for Saudi Arabia's national authentication platform with real-time status updates via SignalR

### Identity & Profiles

Profile system supporting multiple user types:

- **Profile Types**: Customer, Vendor, Admin, Employee, Support
- **Multi-Tenant Support**: Profiles can be scoped to specific tenants
- **Contact Points**: Flexible contact information system (Email, Phone, WhatsApp, etc.)

### Catalog System (v2.0 - Modular)

Independent, composable catalog components:

- **Attributes System**: Dynamic property definitions with multiple types (Text, Number, Boolean, Date, SingleSelect, MultiSelect)
- **Units System**: Measurement units with automatic conversion (Weight, Length, Volume, Area, Temperature, Time)
- **Currency System**: Multi-currency support extending the units system, with exchange rates and conversion
- **Products System**: Product management using attributes, units, and currencies via composition
- **Product Listings**: Vendor-specific product offerings in multi-vendor scenarios

### Marketplace (Multi-Vendor)

- **Vendor Management**: Vendor registration, profiles, and settings
- **Commission System**: Flexible commission models (Percentage, Fixed, Hybrid)
- **Balance Tracking**: Available and pending balances for vendor payouts
- **Vendor Reviews**: Rating and feedback system

### Sales & Orders

- **Shopping Cart**: Supports both authenticated users and guest checkout
- **Order Processing**: Complete order lifecycle with 11+ status states
- **Order Tracking**: Shipment tracking integration
- **Commission Calculation**: Automatic vendor commission calculation on orders

### Payment Processing

Provider-based payment system:

- **Moyasar Provider**: Saudi payment gateway integration (included)
- **Extensible Design**: `IPaymentProvider` interface allows integration with Stripe, PayPal, Tabby, Tamara, etc.

### Shipping & Delivery

Provider-based shipping system:

- **Mock Provider**: For testing and development
- **Extensible Design**: `IShippingProvider` interface ready for Aramex, SMSA, DHL, etc.

### Real-time Communication

- **SignalR Infrastructure**: Generic `IRealtimeHub` abstraction with base hub classes
- **Chat System**: Database-backed chat with real-time delivery
- **Notifications**: Multi-channel notification system (Email, SMS, Push, InApp, WhatsApp, Webhook)
- **Presence Tracking**: User online/offline status

### Service Discovery

Custom service registry pattern (alternative to API Gateway):

- **Dynamic Service Registration**: Services register themselves with the registry
- **Client-side Discovery**: Clients cache service URLs locally
- **No Single Point of Failure**: Unlike API Gateway, if registry goes down, cached URLs keep the system operational
- **No Bottleneck**: Traffic flows directly to services, not through a centralized gateway

### Additional Modules

- **Reviews System**: Product and vendor reviews with ratings
- **Localization**: Multi-language support with dynamic translations
- **Accounting System**: 4-dimensional accounting (Account + Currency + Unit + CostCenter)
- **Transaction Documents**: Configurable document types with operations and workflows

### Client SDKs

14 .NET client SDKs for consuming APIs from various platforms:

- **Core SDK**: `DynamicHttpClient` with interceptors for authentication, retry, and localization
- **Platform Support**: .NET MAUI, Blazor (WebAssembly/Server/Hybrid), WPF, WinForms, Console Apps
- **Service Integration**: Automatically discovers service URLs from Service Registry
- **Typed Clients**: Strongly-typed clients for all major services (Auth, Profiles, Products, Cart, Orders, etc.)

### Data Storage

Flexible storage abstraction:

- **Entity Framework Core**: Primary implementation with PostgreSQL/SQL Server/SQLite support
- **In-Memory Database**: For testing and demos
- **Repository Pattern**: All data access goes through `IBaseAsyncRepository<T>`
- **Soft Deletes**: Global query filters automatically exclude soft-deleted entities

### File Storage (Google Cloud Storage)

Cloud-based file storage for images and media:

- **Provider Pattern**: `IStorageProvider` abstraction with Google Cloud Storage implementation
- **Media Controller**: REST API endpoints for secure image upload (`/api/media/upload`)
- **Security**: 
  - Allowed directories only (listings, profiles, vendors)
  - File size limit: 10MB
  - Content type validation (image/jpeg, png, gif, webp)
  - Authenticated uploads only
- **Bucket**: `gs://ashare-images` in `me-central1` region
- **Client Integration**: `FilesClient.UploadMediaAsync()` for Blazor/MAUI apps

### Cross-Cutting Concerns

- **Exception Handling**: Global exception middleware with consistent error responses
- **Validation**: FluentValidation integrated into CQRS pipeline
- **Logging**: Serilog integration with structured logging
- **Swagger/OpenAPI**: Automatic API documentation with authentication support

## External Dependencies

### Databases
- PostgreSQL (primary production database)
- SQL Server (supported alternative)
- SQLite (development/testing)

### Third-Party Services
- **Nafath**: Saudi National Authentication (https://api.authentica.sa)
- **Moyasar**: Saudi payment gateway
- **Firebase**: Push notifications (optional)
- **Email Providers**: SMTP, SendGrid, AWS SES (configurable)
- **SMS Providers**: Twilio, AWS SNS (configurable)

### Key NuGet Packages
- MediatR 13.1.0 (CQRS)
- FluentValidation (validation)
- Entity Framework Core (data access)
- Microsoft.AspNetCore.SignalR (real-time)
- Serilog (logging)
- Swashbuckle (Swagger/OpenAPI)
- Syncfusion Blazor Components (UI templates - commercial license required)

### Development Tools
- .NET 9.0 SDK (required)
- Visual Studio 2022 / VS Code / Rider (recommended)
- Docker (optional, for databases)