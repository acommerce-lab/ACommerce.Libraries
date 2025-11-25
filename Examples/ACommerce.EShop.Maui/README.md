# 📱 ACommerce E-Shop MAUI Blazor App

Cross-platform mobile e-commerce app built with **.NET MAUI** and **Blazor Hybrid**.

## 🎯 Overview

This is a **production-ready** cross-platform mobile app that consumes the ACommerce E-Shop API.

## ✨ Features

### 📱 Cross-Platform
- ✅ Android
- ✅ iOS
- ✅ macOS (Catalyst)
- ✅ Windows

### 🛍️ E-Commerce Features
- Product browsing with filters
- Shopping cart management
- Order placement and tracking
- User profile management
- Real-time notifications
- Chat support

### 🎨 Modern UI
- **Syncfusion Blazor Components**
- Material Design 3
- Smooth animations
- RTL support (Arabic)
- Responsive layouts

### 🔐 Security
- JWT authentication
- Secure API communication
- Token management

## 🏗️ Architecture

### Blazor Hybrid
- Web UI (Blazor) + Native capabilities
- Shared codebase across platforms
- Full access to platform APIs

### Client SDKs
Uses all ACommerce Client SDKs:
- ProfilesClient
- ProductsClient
- AttributesClient
- UnitsClient
- CurrenciesClient
- CartsClient
- OrdersClient
- VendorsClient

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 17.8+ or VS Code
- MAUI workloads installed

### Install MAUI Workloads
```bash
dotnet workload install maui
```

### Run on Android
```bash
dotnet build -t:Run -f net9.0-android
```

### Run on iOS
```bash
dotnet build -t:Run -f net9.0-ios
```

### Run on Windows
```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

## 📦 Dependencies

### UI Framework
- Microsoft.Maui.Controls (9.0.0)
- Microsoft.AspNetCore.Components.WebView.Maui (9.0.0)

### Syncfusion Components
- Syncfusion.Blazor.Themes (27.2.5)
- Syncfusion.Blazor.Grid (27.2.5)
- Syncfusion.Blazor.Buttons (27.2.5)
- Syncfusion.Blazor.Inputs (27.2.5)
- Syncfusion.Blazor.Navigations (27.2.5)
- Syncfusion.Blazor.Cards (27.2.5)

### Client SDKs
- All ACommerce.Client.* libraries

## 🎨 Theme Customization

Edit `wwwroot/css/app.css` to customize:
- Color scheme
- Typography
- Component styles
- Animations

Current theme: **Material Design 3** (Purple/Teal)

## 📊 Statistics

### Frontend Code
- **Components**: 10+ Blazor components
- **Pages**: 8+ pages
- **Styles**: ~500 lines of CSS
- **Configuration**: ~200 lines
- **Total Lines**: ~3,000+ lines

### Blazor Components
- Home
- Products
- Cart
- Orders
- Profile
- Admin (Vendors, Currencies, Units)

## 🔧 Configuration

Edit `MauiProgram.cs`:

```csharp
builder.Services.AddHttpClient("EShopAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:5001/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### Syncfusion License
Get a free community license or commercial license:
https://www.syncfusion.com/sales/licensing

Add license key in `MauiProgram.cs`:
```csharp
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
```

## 📱 Screenshots

_(Screenshots will be added after first run)_

## 🎉 What Makes This Special?

### Blazor Hybrid Benefits
- ✅ Write once, run anywhere
- ✅ Shared code between web and mobile
- ✅ Full access to native APIs
- ✅ No WebView limitations

### Clean Architecture
- ✅ Uses all refactored v2.0 libraries
- ✅ Type-safe API calls via Client SDKs
- ✅ Dependency injection
- ✅ Testable components

### Production Ready
- ✅ Error handling
- ✅ Loading states
- ✅ Offline support (future)
- ✅ Push notifications ready

## 📝 License

Part of ACommerce Libraries - Multi-Vendor E-Commerce Platform

---

**Built with ❤️ using .NET MAUI + Blazor Hybrid**
