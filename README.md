# ACommerce.Libraries

> **مكتبات .NET قابلة لإعادة الاستخدام لبناء منصات تجارة إلكترونية متعددة البائعين**

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## 🎯 **الهدف**

مكتبات متكاملة تحول إنشاء متجر إلكتروني متعدد البائعين إلى **مسألة تهيئة فقط**.

### 🎉 **تم تحقيق الهدف:**
```
✅ متجر متعدد البائعين كامل = 84 سطر فقط
✅ 32+ API Endpoint جاهزة
✅ 0 Controllers يدوية
✅ الوقت: 30 دقيقة بدلاً من 3-4 أسابيع
✅ توفير 99% من الكود

📖 اقرأ المزيد: [تحليل النحافة](LEANNESS_ANALYSIS.md)
```

### ✨ **الميزات الرئيسية:**
- ✅ **Multi-Vendor**: دعم متعدد البائعين بالكامل
- ✅ **CQRS**: معمارية CQRS مع MediatR
- ✅ **Repository Pattern**: فصل المنطق عن التخزين
- ✅ **Provider Pattern**: تجريد كامل (Payments, Shipping)
- ✅ **Modular**: موديولات قابلة للتركيب (Reviews, Localization)
- ✅ **API-First**: Controllers جاهزة من المكتبات

---

## 📦 **البنية الكاملة**

### **🔷 Core - الأساس**
```
Core/
├── ACommerce.SharedKernel.Abstractions         # Entities, Repositories, Queries
├── ACommerce.SharedKernel.CQRS                 # CQRS implementation
├── ACommerce.SharedKernel.Infrastructure.EFCores # EF Core repositories
└── ACommerce.Configuration                     # Settings management
```

### **🔷 Identity - الهوية والبروفايلات**
```
Identity/
├── ACommerce.Profiles                          # Customer, Vendor, Admin profiles
└── ACommerce.Profiles.Api                      # Ready-to-use controllers
```

### **🔷 Authentication - المصادقة**
```
Authentication/
├── ACommerce.Authentication.Abstractions       # Auth contracts
├── ACommerce.Authentication.JWT                # JWT provider
├── ACommerce.Authentication.OpenIddict         # OpenIddict provider
├── ACommerce.Authentication.MicrosoftIdentity  # Microsoft Identity
├── ACommerce.Authentication.TwoFactor.*        # 2FA (Nafath, SMS, Email)
└── ACommerce.Authentication.Users.Abstractions # User management
```

### **🔷 Marketplace - السوق**
```
Marketplace/
├── ACommerce.Vendors                           # Vendor management + commissions
└── ACommerce.Vendors.Api                       # Vendor endpoints
```

### **🔷 Catalog - الكتالوج**
```
Catalog/
├── ACommerce.Catalog.Listings                  # Product listings (Vendor offers)
├── ACommerce.Catalog.Listings.Api              # Listings endpoints
Other/ACommerce.Catalog.Products                # Products catalog
Other/ACommerce.Catalog.Attributes              # Product attributes
Other/ACommerce.Catalog.Units                   # Units & measurements
Other/ACommerce.Catalog.Currencies              # Multi-currency support
```

**المفهوم الأساسي:**
```
Product (من الصانع) → ProductListing (عرض البائع) → Order
```

### **🔷 Sales - المبيعات**
```
Sales/
├── ACommerce.Cart                              # Shopping cart
├── ACommerce.Orders                            # Order management
└── ACommerce.Orders.Api                        # Order endpoints
```

### **🔷 Payments - الدفع**
```
Payments/
├── ACommerce.Payments.Abstractions             # IPaymentProvider interface
└── ACommerce.Payments.Moyasar                  # Moyasar integration (Saudi)
```

**جاهز للتوسع:**
- Stripe
- PayPal
- Tabby (BNPL)
- Tamara (BNPL)

### **🔷 Shipping - الشحن**
```
Shipping/
├── ACommerce.Shipping.Abstractions             # IShippingProvider interface
└── ACommerce.Shipping.Mock                     # Mock provider for testing
```

**جاهز للتوسع:**
- Aramex
- SMSA
- DHL

### **🔷 Communication - الاتصالات**
```
Communication/
├── ACommerce.Messaging.Abstractions            # Messaging contracts
├── ACommerce.Messaging.SignalR                 # SignalR implementation
├── ACommerce.Notifications.*                   # Multi-channel notifications
└── Other/ACommerce.Chats.*                     # Real-time chat
```

### **🔷 Modules - موديولات قابلة للتركيب**
```
Modules/
├── ACommerce.Reviews                           # Universal reviews module
└── ACommerce.Localization                      # Multi-language support
```

### **🔷 Other - مكتبات إضافية**
```
Other/
├── ACommerce.Accounting.Core                   # Double-entry bookkeeping
├── ACommerce.Transactions.Core                 # Document-driven architecture
└── ... (more)
```

### **🔷 Examples - أمثلة**
```
Examples/
└── ACommerce.MarketplaceApi                    # Complete marketplace in 84 lines!
```

---

## 🚀 **البداية السريعة**

### **1. Clone المشروع:**
```bash
git clone https://github.com/acommerce-lab/ACommerce.Libraries.git
cd ACommerce.Libraries
```

### **2. فتح Solution:**
```bash
# Visual Studio
start ACommerce.Libraries.sln

# Rider
rider ACommerce.Libraries.sln

# VS Code
code .
```

### **3. تشغيل المثال التجريبي:**
```bash
cd Examples/ACommerce.MarketplaceApi
dotnet run
```

ثم افتح: `https://localhost:5001/swagger`

---

## 📖 **أمثلة الاستخدام**

### **مثال 1: إنشاء متجر بسيط**
```csharp
// Program.cs - فقط 84 سطر!
var builder = WebApplication.CreateBuilder(args);

// Add controllers من المكتبات
builder.Services.AddControllers()
    .AddApplicationPart(typeof(ProfilesController).Assembly)
    .AddApplicationPart(typeof(VendorsController).Assembly)
    .AddApplicationPart(typeof(ProductListingsController).Assembly)
    .AddApplicationPart(typeof(OrdersController).Assembly);

// CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Repository
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();

// Payment Provider
builder.Services.AddScoped<IPaymentProvider, MoyasarPaymentProvider>();

// Shipping Provider
builder.Services.AddScoped<IShippingProvider, MockShippingProvider>();

var app = builder.Build();
app.MapControllers();
app.Run();
```

**النتيجة:** متجر كامل بدون كتابة Controllers يدوياً! 🎉

### **مثال 2: إضافة بائع**
```bash
POST /api/vendors
{
  "storeName": "متجر الإلكترونيات",
  "storeSlug": "electronics-store",
  "commissionValue": 10.0,
  "commissionType": "Percentage"
}
```

### **مثال 3: عرض منتج من بائع**
```bash
POST /api/productlistings
{
  "vendorId": "guid",
  "productId": "guid",
  "price": 299.99,
  "quantityAvailable": 50
}
```

---

## 🏗️ **المعمارية**

### **CQRS Pattern:**
```
Request → Command/Query → Handler → Repository → Database
```

### **Repository Pattern:**
```
Controller → IRepositoryFactory → IBaseAsyncRepository<T> → DbContext
```

### **Provider Pattern:**
```
Service → IPaymentProvider → MoyasarPaymentProvider (or any other)
```

---

## 📋 **المتطلبات**

- **.NET 9.0 SDK** أو أحدث
- **Visual Studio 2022** / **JetBrains Rider** / **VS Code**
- **SQL Server** / **PostgreSQL** (اختياري - يعمل مع InMemory للتطوير)

---

## 🔧 **Build & Pack**

### **Build:**
```bash
dotnet restore
dotnet build
```

### **Pack as NuGet:**
```bash
dotnet pack -c Release -o ./nupkg
```

### **Publish محدد:**
```bash
dotnet pack ACommerce.Profiles/ACommerce.Profiles.csproj -c Release
```

---

## 📚 **التوثيق**

### **📖 البداية:**
- [⚡ البداية السريعة (QUICK_START.md)](QUICK_START.md) - من الصفر إلى متجر في 30 دقيقة
- [🧪 دليل الاختبار (TESTING_GUIDE.md)](TESTING_GUIDE.md) - اختبار Backend خطوة بخطوة
- [📊 تحليل النحافة (LEANNESS_ANALYSIS.md)](LEANNESS_ANALYSIS.md) - مقارنة الأداء والمقاييس
- [📦 إدارة المكتبات (TRANSITIVE_DEPENDENCIES_GUIDE.md)](TRANSITIVE_DEPENDENCIES_GUIDE.md) - **مهم!** تجنب تعارض الإصدارات
- [🎉 ملخص الإنجاز (ACHIEVEMENT_SUMMARY.md)](ACHIEVEMENT_SUMMARY.md) - نظرة شاملة

### **📦 المكتبات:**
- [Identity & Profiles](Identity/README.md)
- [Marketplace & Vendors](Marketplace/README.md)
- [Product Listings](Catalog/README.md)
- [Orders System](Sales/README.md)
- [Payments Integration](Payments/README.md)
- [Shipping Providers](Shipping/README.md)
- [Example Backend](Examples/ACommerce.MarketplaceApi/README.md)

---

## 🎓 **المفاهيم الأساسية**

### **1. Multi-Vendor Architecture:**
```
Product (الصانع)
   ↓
ProductListing (البائع يعرض المنتج بسعره ومخزونه)
   ↓
Order (العميل يطلب من العرض)
   ↓
OrderItem (مع حساب عمولة المنصة)
```

### **2. Document-Driven:**
كل وثيقة (Order, Invoice, Shipment) هي:
- Entity مع علاقات
- Workflow states
- Accounting entries (تلقائية)
- Events & Notifications

### **3. Configuration-First:**
```json
{
  "Store": {
    "Name": "My Store",
    "DefaultCurrency": "SAR",
    "EnableMultiVendor": true
  },
  "Payments": {
    "Moyasar": { "ApiKey": "..." }
  }
}
```

---

## 🤝 **المساهمة**

نرحب بالمساهمات! يرجى:
1. Fork المشروع
2. إنشاء Branch جديد
3. Commit التغييرات
4. Push إلى Branch
5. فتح Pull Request

---

## 📄 **الترخيص**

MIT License - انظر [LICENSE](LICENSE) للتفاصيل.

---

## 🌟 **الميزات القادمة**

- [ ] Coupons & Discounts
- [ ] Returns & Refunds system
- [ ] Advanced Analytics
- [ ] Mobile SDK
- [ ] GraphQL API
- [ ] Microservices templates

---

## 📞 **الدعم**

- 📧 Email: support@acommerce.com
- 💬 Discord: [Join our community](https://discord.gg/acommerce)
- 📖 Docs: [docs.acommerce.com](https://docs.acommerce.com)

---

**Built with ❤️ for Saudi e-commerce ecosystem**
