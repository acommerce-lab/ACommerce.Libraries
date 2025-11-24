# 🗄️ دليل التجريد للتخزين (Storage Abstraction)

## 🎯 المشكلة

```
❌ كل مكتبة لها Entities خاصة بها
❌ DbContext لا يعرف عنها
❌ نحتاج سطر تهيئة لكل مكتبة
❌ تكرار وتعقيد
```

## ✅ الحل: Auto-Discovery + Modular Configuration

### **المبدأ الأساسي:**
> **سطر واحد يكفي! ApplicationDbContext يكتشف تلقائياً جميع Entities من جميع المكتبات** ✨

---

## 🏗️ البنية المعمارية

```
┌─────────────────────────────────────────────────┐
│           Your Application (Program.cs)          │
│                                                  │
│  builder.Services.AddACommerceInMemoryDatabase() │ ← سطر واحد!
│                                                  │
└──────────────────┬───────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│      ApplicationDbContext (Auto-Discovery)       │
│                                                  │
│  ┌───────────────────────────────────────────┐  │
│  │ 1. Scan all ACommerce.* Assemblies        │  │
│  │ 2. Find all IBaseEntity implementations   │  │
│  │ 3. Register them automatically            │  │
│  │ 4. Apply IEntityTypeConfiguration         │  │
│  └───────────────────────────────────────────┘  │
│                                                  │
└──────────────────┬───────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│              All Libraries                       │
│                                                  │
│  ACommerce.Profiles                             │
│  ├── Entities/Profile.cs (IBaseEntity) ✅       │
│  └── (لا تحتاج معرفة بـ EF Core!)               │
│                                                  │
│  ACommerce.Vendors                              │
│  ├── Entities/Vendor.cs (IBaseEntity) ✅        │
│  └── (لا تحتاج معرفة بـ EF Core!)               │
│                                                  │
│  ACommerce.Orders                               │
│  ├── Entities/Order.cs (IBaseEntity) ✅         │
│  └── (لا تحتاج معرفة بـ EF Core!)               │
│                                                  │
│  ... وهكذا لجميع المكتبات                       │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## 📝 الاستخدام

### **1. InMemory Database (للتجربة):**
```csharp
// Program.cs
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;

builder.Services.AddACommerceInMemoryDatabase("MyStore");
```

**ذلك كل شيء!** ✨
- يكتشف تلقائياً جميع Entities
- يسجل Repository Factory
- جاهز للاستخدام فوراً

---

### **2. SQL Server (للإنتاج):**
```csharp
builder.Services.AddACommerceSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
);
```

---

### **3. PostgreSQL:**
```csharp
builder.Services.AddACommercePostgreSQL(
    builder.Configuration.GetConnectionString("PostgresConnection")
);
```

---

### **4. SQLite:**
```csharp
builder.Services.AddACommerceSQLite("Data Source=mystore.db");
```

---

### **5. تهيئة مخصصة:**
```csharp
builder.Services.AddACommerceDbContext(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging(); // للتطوير فقط
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
```

---

## 🔍 كيف يعمل Auto-Discovery؟

### **ApplicationDbContext:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 1. اكتشاف جميع Types التي تنفذ IBaseEntity
    var entityTypes = DiscoverEntityTypes();

    foreach (var entityType in entityTypes)
    {
        modelBuilder.Entity(entityType); // ✅ تسجيل تلقائي!
    }

    // 2. تطبيق Configurations إذا وجدت
    ApplyConfigurationsFromAssemblies(modelBuilder);
}

private IEnumerable<Type> DiscoverEntityTypes()
{
    // البحث في جميع Assemblies التي تبدأ بـ ACommerce
    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName?.StartsWith("ACommerce") == true);

    // العثور على جميع IBaseEntity implementations
    var entityTypes = assemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IBaseEntity).IsAssignableFrom(t)
            && t.IsClass
            && !t.IsAbstract);

    return entityTypes;
}
```

---

## 🎨 Entity Configuration (اختياري)

إذا أردت تخصيص Entity mapping:

```csharp
// في أي مكتبة، مثلاً ACommerce.Vendors
public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.Property(v => v.StoreName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(v => v.StoreSlug)
            .IsUnique();

        builder.Property(v => v.CommissionValue)
            .HasPrecision(18, 2);
    }
}
```

**ApplicationDbContext سيكتشفها تلقائياً ويطبقها!** ✨

---

## 🔄 التبديل بين Storage Providers

### **Scenario: من InMemory إلى SQL Server**

#### **قبل (Development):**
```csharp
builder.Services.AddACommerceInMemoryDatabase("MyStore");
```

#### **بعد (Production):**
```csharp
builder.Services.AddACommerceSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
);
```

**لا حاجة لتغيير أي شيء آخر!** 🎯

---

## 🧪 Migrations (للـ Production Databases)

### **1. إنشاء Migration:**
```bash
dotnet ef migrations add InitialCreate \
    --project Examples/ACommerce.MarketplaceApi \
    --context ApplicationDbContext
```

### **2. تطبيق Migration:**
```bash
dotnet ef database update \
    --project Examples/ACommerce.MarketplaceApi \
    --context ApplicationDbContext
```

### **3. Migration في التطبيق (Auto):**
```csharp
var app = builder.Build();

// تطبيق Migrations تلقائياً عند التشغيل
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}
```

---

## 🌐 دعم Multiple Storage Types

### **Scenario: Mongo للـ Logs + SQL للـ Entities**

```csharp
// SQL Server للـ Entities
builder.Services.AddACommerceSqlServer(sqlConnectionString);

// Mongo لـ Logs (مثال)
builder.Services.AddSingleton<ILogRepository, MongoLogRepository>();
```

**المكتبات لا تحتاج معرفة نوع التخزين!** ✅

---

## 📊 مقارنة: قبل vs بعد

### **❌ الطريقة القديمة (المعقدة):**

```csharp
// Program.cs
builder.Services.AddDbContext<DbContext>(options =>
    options.UseInMemoryDatabase("MyStore"));

builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();

// ❌ المشكلة: DbContext لا يعرف عن Entities!
// ❌ خطأ: Cannot create a DbSet for 'Profile'
```

---

### **✅ الطريقة الجديدة (البسيطة):**

```csharp
// Program.cs
builder.Services.AddACommerceInMemoryDatabase("MyStore");

// ✅ كل شيء يعمل تلقائياً!
// ✅ Auto-Discovery لجميع Entities
// ✅ Repository Factory مسجل
// ✅ جاهز للاستخدام
```

---

## 🎯 الفوائد

### **1. سهولة الاستخدام:**
```
سطر واحد فقط! ✨
```

### **2. لا تكرار:**
```
لا حاجة لتسجيل كل Entity يدوياً ✅
```

### **3. المرونة:**
```
تبديل Storage Provider بسطر واحد ✅
```

### **4. Separation of Concerns:**
```
المكتبات لا تعرف عن EF Core ✅
```

### **5. Extensibility:**
```
IEntityTypeConfiguration للتخصيص ✅
```

### **6. Multi-Database Support:**
```
SQL + Mongo + Redis معاً ✅
```

---

## 🔧 حالات متقدمة

### **1. Multiple DbContexts:**

```csharp
// Context أساسي لـ Entities
builder.Services.AddACommerceSqlServer(mainConnectionString);

// Context منفصل للـ Analytics (مثال)
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlServer(analyticsConnectionString));
```

---

### **2. Read/Write Separation:**

```csharp
// Write DB
builder.Services.AddACommerceSqlServer(writeConnectionString);

// Read DB (Replica)
builder.Services.AddDbContext<ReadOnlyDbContext>(options =>
{
    options.UseSqlServer(readReplicaConnectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
```

---

### **3. Sharding (Multi-Tenant):**

```csharp
builder.Services.AddACommerceDbContext(options =>
{
    var tenantId = GetCurrentTenantId();
    var connectionString = GetConnectionStringForTenant(tenantId);
    options.UseSqlServer(connectionString);
});
```

---

## 📝 Best Practices

### **1. Entities في المكتبات:**
```csharp
// ✅ جيد: Entity بسيط، لا يعرف عن EF
public class Product : IBaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // ...
}
```

```csharp
// ❌ سيء: Entity يعتمد على EF
public class Product : IBaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } // ❌ Data Annotations
}
```

---

### **2. Configuration منفصلة:**
```csharp
// ✅ جيد: Configuration في ملف منفصل
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}
```

---

### **3. InMemory للتجربة، SQL للإنتاج:**
```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddACommerceInMemoryDatabase("DevDb");
}
else
{
    builder.Services.AddACommerceSqlServer(
        builder.Configuration.GetConnectionString("Production")
    );
}
```

---

## 🎉 الخلاصة

### **الحل يحقق:**

```
✅ سطر واحد = Database جاهز
✅ Auto-Discovery لجميع Entities
✅ تبديل Storage Provider بسهولة
✅ المكتبات مستقلة عن EF Core
✅ Extensible مع Configurations
✅ يدعم جميع EF Core Providers
✅ Production-ready
```

### **النتيجة:**

```
┌────────────────────────────────────────┐
│                                        │
│  من 15+ سطر كود معقد                  │
│  إلى سطر واحد بسيط! ✨                │
│                                        │
│  builder.Services                      │
│    .AddACommerceInMemoryDatabase();    │
│                                        │
│  هذه قوة التجريد الصحيح! 🚀           │
│                                        │
└────────────────────────────────────────┘
```

---

## 📚 المراجع

- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Modular Monolith Architecture](https://www.kamilgrzybek.com/design/modular-monolith-primer/)

---

**Built with ❤️ for flexible and scalable architecture**
