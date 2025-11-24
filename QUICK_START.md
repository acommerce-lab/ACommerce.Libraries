# ⚡ البداية السريعة - ACommerce Libraries

## 🎯 من الصفر إلى متجر كامل في 30 دقيقة

هذا الدليل سيأخذك من **لا شيء** إلى **متجر متعدد البائعين كامل** في 30 دقيقة فقط.

---

## ✅ المتطلبات (5 دقائق)

### 1. تثبيت .NET 9.0 SDK:
```bash
# Windows (PowerShell)
winget install Microsoft.DotNet.SDK.9

# macOS
brew install dotnet-sdk

# Linux (Ubuntu/Debian)
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# تحقق من التثبيت
dotnet --version
# يجب أن يظهر: 9.0.x
```

### 2. Clone المشروع:
```bash
git clone https://github.com/acommerce-lab/ACommerce.Libraries.git
cd ACommerce.Libraries
```

---

## 🚀 الطريقة 1: استخدام المثال الجاهز (10 دقائق)

### 1. تشغيل المثال:
```bash
cd Examples/ACommerce.MarketplaceApi
dotnet run
```

### 2. فتح Swagger:
افتح المتصفح على: `https://localhost:5001/swagger`

### 3. اختبار API:
```bash
# إنشاء بروفايل
curl -X POST "https://localhost:5001/api/profiles" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "test-user-1",
    "type": "Customer",
    "fullName": "أحمد محمد"
  }'
```

**🎉 تم! لديك الآن متجر كامل يعمل!**

---

## 🛠️ الطريقة 2: إنشاء مشروع جديد من الصفر (20 دقيقة)

### الخطوة 1: إنشاء مشروع جديد (2 دقيقة)
```bash
# إنشاء مشروع ASP.NET Core جديد
dotnet new webapi -n MyMarketplace
cd MyMarketplace

# إنشاء Solution
dotnet new sln -n MyMarketplace
dotnet sln add MyMarketplace.csproj
```

### الخطوة 2: إضافة المكتبات (3 دقائق)
```bash
# أضف المكتبات كـ Project References
# (أو يمكنك استخدام NuGet packages بعد نشرها)

# Core
dotnet add reference ../../ACommerce.Libraries/Core/ACommerce.SharedKernel.Abstractions
dotnet add reference ../../ACommerce.Libraries/Core/ACommerce.SharedKernel.CQRS
dotnet add reference ../../ACommerce.Libraries/Core/ACommerce.SharedKernel.Infrastructure.EFCores

# Identity
dotnet add reference ../../ACommerce.Libraries/Identity/ACommerce.Profiles
dotnet add reference ../../ACommerce.Libraries/Identity/ACommerce.Profiles.Api

# Marketplace
dotnet add reference ../../ACommerce.Libraries/Marketplace/ACommerce.Vendors
dotnet add reference ../../ACommerce.Libraries/Marketplace/ACommerce.Vendors.Api

# Catalog
dotnet add reference ../../ACommerce.Libraries/Catalog/ACommerce.Catalog.Listings
dotnet add reference ../../ACommerce.Libraries/Catalog/ACommerce.Catalog.Listings.Api

# Sales
dotnet add reference ../../ACommerce.Libraries/Sales/ACommerce.Cart
dotnet add reference ../../ACommerce.Libraries/Sales/ACommerce.Orders
dotnet add reference ../../ACommerce.Libraries/Sales/ACommerce.Orders.Api

# Payments
dotnet add reference ../../ACommerce.Libraries/Payments/ACommerce.Payments.Abstractions
dotnet add reference ../../ACommerce.Libraries/Payments/ACommerce.Payments.Moyasar

# Shipping
dotnet add reference ../../ACommerce.Libraries/Shipping/ACommerce.Shipping.Abstractions
dotnet add reference ../../ACommerce.Libraries/Shipping/ACommerce.Shipping.Mock
```

### الخطوة 3: كتابة Program.cs (10 دقائق)
```csharp
using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Moyasar.Services;
using ACommerce.Payments.Moyasar.Models;
using ACommerce.Shipping.Abstractions.Contracts;
using ACommerce.Shipping.Mock.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers من المكتبات
builder.Services.AddControllers()
    .AddApplicationPart(typeof(ACommerce.Profiles.Api.Controllers.ProfilesController).Assembly)
    .AddApplicationPart(typeof(ACommerce.Vendors.Api.Controllers.VendorsController).Assembly)
    .AddApplicationPart(typeof(ACommerce.Catalog.Listings.Api.Controllers.ProductListingsController).Assembly)
    .AddApplicationPart(typeof(ACommerce.Orders.Api.Controllers.OrdersController).Assembly);

// CQRS
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Database (InMemory للتطوير)
builder.Services.AddDbContext<DbContext>(options =>
    options.UseInMemoryDatabase("MyMarketplace"));

// Repository
builder.Services.AddScoped<ACommerce.SharedKernel.Abstractions.Repositories.IRepositoryFactory,
    ACommerce.SharedKernel.Infrastructure.EFCores.Factories.RepositoryFactory>();

// Payment Provider
builder.Services.Configure<MoyasarOptions>(options =>
{
    options.ApiKey = "test_key";
    options.PublishableKey = "test_pub_key";
    options.UseSandbox = true;
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentProvider, MoyasarPaymentProvider>();

// Shipping Provider
builder.Services.AddScoped<IShippingProvider, MockShippingProvider>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### الخطوة 4: التشغيل (5 دقائق)
```bash
dotnet restore
dotnet build
dotnet run
```

افتح: `https://localhost:5001/swagger`

**🎉 تم! متجرك الخاص جاهز!**

---

## 📊 ما تحصل عليه مجاناً

### **API Endpoints (32+):**
```
✓ Profiles Management (5 endpoints)
✓ Vendors Management (8 endpoints)
✓ Product Listings (6 endpoints)
✓ Shopping Cart (4 endpoints)
✓ Orders Management (9 endpoints)
```

### **Features:**
```
✓ Multi-Vendor System
✓ Commission Calculation
✓ Guest Checkout
✓ Order Tracking
✓ Payment Integration (Moyasar)
✓ Shipping Integration
✓ CRUD Operations
✓ Search & Filtering
✓ Swagger UI
```

### **Architecture:**
```
✓ CQRS Pattern
✓ Repository Pattern
✓ Provider Pattern
✓ Clean Architecture
✓ Dependency Injection
✓ AutoMapper
✓ MediatR
```

---

## 🔧 التخصيص

### تغيير قاعدة البيانات إلى SQL Server:

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyMarketplace;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}

// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### إضافة Authentication:

```csharp
// Add to Program.cs
builder.Services.AddControllers()
    .AddApplicationPart(typeof(AuthenticationController).Assembly);

builder.Services.AddJwtAuthentication(builder.Configuration);
```

### تغيير Payment Provider إلى Stripe:

```csharp
// استبدل هذا:
builder.Services.AddScoped<IPaymentProvider, MoyasarPaymentProvider>();

// بهذا:
builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
```

---

## 📚 الخطوات التالية

### **للتعلم:**
1. 📖 اقرأ [دليل الاختبار](TESTING_GUIDE.md)
2. 📊 اطلع على [تحليل النحافة](LEANNESS_ANALYSIS.md)
3. 🎉 راجع [ملخص الإنجاز](ACHIEVEMENT_SUMMARY.md)

### **للتطوير:**
1. أضف Products من `ACommerce.Catalog.Products`
2. أضف Authentication من `ACommerce.Authentication.*`
3. أضف Notifications من `ACommerce.Notifications.*`
4. أضف Reviews من `ACommerce.Reviews`

### **للإنتاج:**
1. استبدل InMemory بـ SQL Server/PostgreSQL
2. أضف Authentication & Authorization
3. أضف Rate Limiting
4. أضف Logging & Monitoring
5. أضف Caching (Redis)
6. استخدم Payment Provider حقيقي
7. استخدم Shipping Provider حقيقي

---

## 🆘 المشاكل الشائعة

### مشكلة: dotnet command not found
```bash
# الحل: تأكد من تثبيت .NET SDK
dotnet --version
```

### مشكلة: Port 5001 already in use
```bash
# الحل: غير البورت في launchSettings.json
"applicationUrl": "https://localhost:5002;http://localhost:5003"
```

### مشكلة: Cannot find assembly
```bash
# الحل: تأكد من المسارات في .csproj
dotnet restore
dotnet clean
dotnet build
```

---

## 🎓 المفاهيم الأساسية

### **Multi-Vendor Flow:**
```
1. Vendor ينشئ Profile
2. Vendor يسجل في النظام (مع نسبة عمولة)
3. Vendor ينشئ ProductListing (عرض لمنتج موجود)
4. Customer يضيف Listing للسلة
5. Customer ينشئ Order
6. System يحسب عمولة المنصة تلقائياً
7. Order يتحول من Draft → Confirmed → Shipped → Delivered
```

### **Provider Pattern:**
```
IPaymentProvider → يمكن تبديله بأي بوابة دفع
IShippingProvider → يمكن تبديله بأي شركة شحن
ITranslationService → يمكن تبديله بأي خدمة ترجمة
```

---

## ⏱️ الجدول الزمني

```
دقيقة 0-5:    تثبيت المتطلبات
دقيقة 5-10:   Clone المشروع
دقيقة 10-15:  تشغيل المثال
دقيقة 15-20:  اختبار APIs
دقيقة 20-25:  إنشاء مشروع جديد (optional)
دقيقة 25-30:  تخصيص المشروع (optional)

النتيجة: متجر جاهز في 30 دقيقة! 🎉
```

---

## 💡 نصائح

1. **ابدأ بالمثال الجاهز** قبل إنشاء مشروع جديد
2. **استخدم Swagger UI** لاختبار APIs بسهولة
3. **اقرأ التوثيق** لفهم المعمارية
4. **جرب كل endpoint** لفهم الـ flow
5. **استخدم InMemory DB** للتطوير السريع
6. **لا تضف مكتبات مكررة!** اقرأ [دليل إدارة المكتبات](TRANSITIVE_DEPENDENCIES_GUIDE.md)

---

## ⚠️ تنبيه مهم: إدارة المكتبات

### **لا تضف PackageReferences موجودة في ProjectReferences!**

```xml
<!-- ❌ خطأ شائع -->
<ItemGroup>
  <ProjectReference Include="ACommerce.Orders.Api" />
  <PackageReference Include="MediatR" /> <!-- ❌ موجود بالفعل في Orders.Api! -->
  <PackageReference Include="AutoMapper" /> <!-- ❌ موجود بالفعل! -->
</ItemGroup>

<!-- ✅ الصحيح -->
<ItemGroup>
  <ProjectReference Include="ACommerce.Orders.Api" />
  <!-- MediatR, AutoMapper, EF Core ورثناهم تلقائياً -->
  <!-- نضيف فقط ما لا يوجد في المكتبات -->
  <PackageReference Include="Swashbuckle.AspNetCore" />
</ItemGroup>
```

**الفوائد:**
- ✅ تجنب تعارض الإصدارات
- ✅ تقليل حجم المشروع
- ✅ سهولة الصيانة

📖 **اقرأ المزيد:** [دليل إدارة المكتبات والاعتمادات](TRANSITIVE_DEPENDENCIES_GUIDE.md)

---

## 🎯 الخلاصة

```
✅ 30 دقيقة = متجر متعدد البائعين كامل
✅ 84 سطر برمجي فقط
✅ 32+ API Endpoint
✅ 0 Controllers يدوية
✅ جاهز للإنتاج بتعديلات بسيطة

هذه قوة ACommerce.Libraries! 🚀
```

---

**Ready to build your marketplace? Let's go! 🎉**
