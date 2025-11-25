# 🌐 Service Registry Pattern - دليل الاستخدام الشامل

## 📋 جدول المحتويات

1. [نظرة عامة](#نظرة-عامة)
2. [لماذا Service Registry بدلاً من API Gateway؟](#لماذا-service-registry-بدلاً-من-api-gateway)
3. [المعمارية](#المعمارية)
4. [المكتبات](#المكتبات)
5. [البدء السريع](#البدء-السريع)
6. [أمثلة متقدمة](#أمثلة-متقدمة)
7. [Best Practices](#best-practices)

---

## نظرة عامة

**Service Registry Pattern** هو نمط معماري يسمح للخدمات المصغرة (Microservices) بتسجيل نفسها ديناميكياً واكتشاف بعضها البعض بدون Hardcoded URLs.

### المشكلة:
- **API Gateway** = نقطة فشل واحدة (Single Point of Failure)
- عند تعطل Gateway، كل النظام يتعطل
- Bottleneck في الأداء

### الحل:
- **Service Registry** = خدمة خفيفة لتسجيل واكتشاف الخدمات
- التطبيقات تحفظ Cache محلي، فلو تعطل Registry لن تتعطل
- لا يوجد Bottleneck لأن Traffic الفعلي يذهب مباشرة للخدمة
- تغيير URLs نادر جداً، فالـ Cache يكفي معظم الوقت

---

## لماذا Service Registry بدلاً من API Gateway؟

### API Gateway (المشكلة):

```
Frontend → API Gateway → Service A
                     ↓
                  Service B
                     ↓
                  Service C
```

❌ **المشاكل:**
- نقطة فشل واحدة
- كل الـ Traffic يمر عبر Gateway = Bottleneck
- إذا تعطل Gateway، النظام كله يتعطل

### Service Registry (الحل):

```
                  ┌─────────────────┐
                  │ Service Registry│ ← خدمة خفيفة للاستعلام فقط
                  └─────────────────┘
                         ↑
                  (استعلام نادر + Cache)
                         ↑
Frontend → يحصل على URL مرة واحدة ثم يتصل مباشرة:
            ├─→ Service A
            ├─→ Service B
            └─→ Service C
```

✅ **المميزات:**
- لا توجد نقطة فشل حرجة (Cache محلي)
- Traffic الفعلي مباشر للخدمات = لا Bottleneck
- تغيير URLs نادر = لا ضغط على Registry
- Load Balancing مدمج
- Health Checks تلقائية

---

## المعمارية

### المكونات الأساسية:

```
┌─────────────────────────────────────────────────────────────┐
│                   Service Registry Server                   │
│  - تسجيل الخدمات                                            │
│  - اكتشاف الخدمات                                           │
│  - Health Checks دورية                                      │
│  - تنظيف الخدمات القديمة                                    │
└─────────────────────────────────────────────────────────────┘
                              ↑
                              │
                ┌─────────────┴─────────────┐
                ↓                           ↓
        ┌──────────────┐           ┌──────────────┐
        │  Service A   │           │  Service B   │
        │  (Client)    │           │  (Client)    │
        │              │           │              │
        │  - تسجيل نفسه│           │  - تسجيل نفسه│
        │  - Heartbeat │           │  - Heartbeat │
        │  - Cache محلي│           │  - Cache محلي│
        └──────────────┘           └──────────────┘
```

### تدفق العمل (Flow):

1. **Startup:**
   - الخدمة تسجل نفسها في Registry (اسم + URL + Version + Health endpoint)
   - Registry يحفظ المعلومات

2. **Runtime:**
   - الخدمات ترسل Heartbeat كل 30 ثانية
   - Registry يفحص الصحة (Health Checks) دورياً
   - إذا فشلت خدمة 3 مرات متتالية → تُعتبر Unhealthy

3. **Discovery:**
   - التطبيق الأمامي يسأل Registry عن خدمة معينة
   - Registry يرجع أحسن نسخة (Load Balancing)
   - التطبيق يحفظ النتيجة في Cache محلي (5 دقائق)
   - التطبيق يتصل مباشرة بالخدمة

4. **Failover:**
   - إذا تعطل Registry، التطبيقات تستخدم Cache
   - Cache طويل المدى (Stale Cache) لمدة 1 ساعة

---

## المكتبات

### 1️⃣ `ACommerce.ServiceRegistry.Abstractions`
**التعريفات الأساسية:**
- `ServiceEndpoint` - معلومات الخدمة
- `ServiceHealth` - حالة الصحة
- `ServiceRegistration` - بيانات التسجيل
- `IServiceRegistry` - واجهة تسجيل الخدمات
- `IServiceDiscovery` - واجهة اكتشاف الخدمات

### 2️⃣ `ACommerce.ServiceRegistry.Core`
**التنفيذ الأساسي:**
- `ServiceRegistry` - تسجيل وإدارة الخدمات
- `ServiceDiscovery` - اكتشاف الخدمات مع Load Balancing
- `HealthChecker` - فحص صحة الخدمات
- `InMemoryServiceStore` - تخزين في الذاكرة

### 3️⃣ `ACommerce.ServiceRegistry.Server`
**الخدمة المركزية:**
- REST API للتسجيل والاكتشاف
- Background Service للـ Health Checks
- Swagger UI للتجربة

### 4️⃣ `ACommerce.ServiceRegistry.Client`
**مكتبة للتطبيقات:**
- `ServiceRegistryClient` - للاتصال بـ Registry
- `ServiceCache` - تخزين مؤقت محلي
- `ServiceRegistrationHostedService` - تسجيل تلقائي

---

## البدء السريع

### الخطوة 1: تشغيل Service Registry Server

```bash
cd Infrastructure/ACommerce.ServiceRegistry.Server
dotnet run
```

سيعمل على: `http://localhost:5100`

Swagger: `http://localhost:5100/swagger`

### الخطوة 2: تسجيل خدمة تلقائياً

في `Program.cs` لخدمتك:

```csharp
using ACommerce.ServiceRegistry.Client.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ✨ إضافة Service Registry Client مع تسجيل تلقائي
builder.Services.AddServiceRegistryClient(
    registryUrl: "http://localhost:5100",
    options =>
    {
        options.AutoRegister = true;
        options.ServiceName = "Products";
        options.Version = "v1";
        options.BaseUrl = "http://localhost:5001";
        options.Environment = "Development";
        options.EnableHealthCheck = true;
        options.HealthCheckPath = "/health";
    });

// إضافة Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
```

### الخطوة 3: اكتشاف خدمة من تطبيق آخر

```csharp
using ACommerce.ServiceRegistry.Client;

public class ProductService
{
    private readonly ServiceRegistryClient _registryClient;
    private readonly HttpClient _httpClient;

    public ProductService(ServiceRegistryClient registryClient, IHttpClientFactory httpClientFactory)
    {
        _registryClient = registryClient;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        // 1. اكتشف خدمة Products (مع Cache تلقائي)
        var productsService = await _registryClient.DiscoverAsync("Products");

        if (productsService == null)
            throw new Exception("Products service not available");

        // 2. اتصل بالخدمة مباشرة
        var url = $"{productsService.BaseUrl}/api/products";
        var products = await _httpClient.GetFromJsonAsync<List<Product>>(url);

        return products ?? new List<Product>();
    }
}
```

---

## أمثلة متقدمة

### مثال 1: Load Balancing بين نسخ متعددة

```csharp
// تسجيل 3 نسخ من نفس الخدمة
var registration = new ServiceRegistration
{
    ServiceName = "Orders",
    Version = "v1",
    BaseUrl = "http://localhost:5002", // نسخة 1
    Weight = 100 // وزن عادي
};

await registryClient.RegisterAsync(registration);

// النسخة الثانية
registration.BaseUrl = "http://localhost:5003";
registration.Weight = 150; // وزن أكبر = طلبات أكثر
await registryClient.RegisterAsync(registration);

// عند الاكتشاف، Registry سيختار بناءً على الوزن (Weighted Random)
var ordersService = await registryClient.DiscoverAsync("Orders");
```

### مثال 2: استعلام متقدم بـ Tags

```csharp
// التسجيل مع Tags
var registration = new ServiceRegistration
{
    ServiceName = "Payments",
    BaseUrl = "http://localhost:5004",
    Tags = new Dictionary<string, string>
    {
        { "Region", "EU" },
        { "Provider", "Stripe" }
    }
};

await registryClient.RegisterAsync(registration);

// الاكتشاف بناءً على Tags
var query = new ServiceQuery
{
    ServiceName = "Payments",
    OnlyHealthy = true,
    Tags = new Dictionary<string, string>
    {
        { "Region", "EU" }
    }
};

var paymentsService = await registryClient.DiscoverAsync(query);
```

### مثال 3: Fallback عند فشل Registry

```csharp
try
{
    // محاولة اكتشاف من Registry
    var service = await registryClient.DiscoverAsync("Shipping");
}
catch (Exception ex)
{
    // Registry معطل، استخدم Hardcoded URL كـ Fallback
    _logger.LogWarning("Registry unavailable, using fallback URL");
    var fallbackUrl = configuration["Services:Shipping:FallbackUrl"];
    // استخدم الـ Fallback URL
}
```

### مثال 4: دمج مع MarketplaceApi الحالي

في `Program.cs` لـ `MarketplaceApi`:

```csharp
// ✨ Database
builder.Services.AddACommerceInMemoryDatabase("MarketplaceDb");

// ✨ Service Registry Client - تسجيل تلقائي
builder.Services.AddServiceRegistryClient(
    registryUrl: "http://localhost:5100",
    options =>
    {
        options.AutoRegister = true;
        options.ServiceName = "Marketplace";
        options.Version = "v1";
        options.BaseUrl = builder.Configuration["Urls"] ?? "http://localhost:5000";
        options.Environment = builder.Environment.EnvironmentName;
        options.Tags = new Dictionary<string, string>
        {
            { "Type", "FullStack" },
            { "Features", "Auth,Products,Orders,Vendors,Profiles" }
        };
    });

// ✨ Health Check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "Marketplace"
}));
```

---

## Best Practices

### ✅ Do's:

1. **استخدم Cache محلي دائماً**
   - لا تذهب لـ Registry في كل طلب
   - Cache لمدة 5 دقائق كافي

2. **استخدم Stale Cache كـ Fallback**
   - احتفظ بـ Cache قديم لمدة طويلة (1 ساعة)
   - استخدمه إذا تعطل Registry

3. **استخدم Health Checks**
   - أضف endpoint للـ `/health` في كل خدمة
   - اجعله خفيف (لا يفحص Database)

4. **استخدم Tags للتنظيم**
   - Region, Environment, Version, Provider, etc.
   - يسهل الاستعلام والفلترة

5. **استخدم Weighted Load Balancing**
   - خادم أقوى = وزن أكبر
   - يوزع الحمل بذكاء

### ❌ Don'ts:

1. **لا تستخدم Registry للـ Traffic الفعلي**
   - Registry للاستعلام فقط
   - Traffic الفعلي مباشرة للخدمة

2. **لا تجعل Health Check ثقيل**
   - لا تفحص Database أو External APIs
   - فقط تحقق أن التطبيق يعمل

3. **لا تعتمد 100% على Registry**
   - احتفظ بـ Fallback URLs في Configuration
   - استخدم Cache عند الفشل

4. **لا تنسى Deregister عند Shutdown**
   - Client يلغي التسجيل تلقائياً
   - لكن تأكد من Graceful Shutdown

---

## مقارنة شاملة

| الميزة | API Gateway | Service Registry |
|--------|-------------|------------------|
| **نقطة فشل واحدة** | ❌ نعم | ✅ لا (Cache محلي) |
| **Bottleneck** | ❌ نعم | ✅ لا (اتصال مباشر) |
| **Load Balancing** | ✅ نعم | ✅ نعم (Weighted) |
| **Health Checks** | ✅ نعم | ✅ نعم (دوري) |
| **تعقيد التنفيذ** | 🔴 معقد | 🟢 بسيط |
| **الأداء** | 🔴 متوسط | 🟢 ممتاز |
| **المرونة** | 🟡 متوسطة | 🟢 عالية |
| **التكلفة** | 🔴 مرتفعة | 🟢 منخفضة |

---

## الخلاصة

✨ **Service Registry Pattern** هو الحل المثالي لـ Microservices لأنه:
- **لا يوجد نقطة فشل حرجة** - Cache محلي يضمن العمل حتى لو تعطل Registry
- **أداء ممتاز** - الاتصال مباشر بالخدمات بدون Middleman
- **بسيط ومرن** - سهل التنفيذ والتطوير
- **قابل للتوسع** - يدعم آلاف الخدمات بدون مشاكل
- **Production-ready** - جاهز للإنتاج مع Health Checks و Load Balancing

🎯 **نصيحة أخيرة:** ابدأ بسيط (InMemory)، ثم انتقل لـ Redis أو Database للإنتاج إذا لزم الأمر.

---

## التالي: مكتبات Frontend

في الخطوة التالية سنبني:
- **TypeScript/JavaScript SDK** للتواصل مع Service Registry
- **React Hooks** لاكتشاف الخدمات ديناميكياً
- **HTTP Client مع Dynamic URLs**

Stay tuned! 🚀
