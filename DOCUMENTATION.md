# ACommerce.Libraries - التوثيق الرئيسي

## نظرة عامة
ACommerce.Libraries هو نظام متكامل لبناء تطبيقات التجارة الإلكترونية متعددة البائعين باستخدام .NET 9.0.

يوفر:
- **97 مكتبة** مستقلة وقابلة لإعادة الاستخدام
- **معمارية نظيفة** قائمة على CQRS و Domain-Driven Design
- **دعم Multi-Tenant** للمتاجر متعددة البائعين
- **مرونة التكامل** مع Provider Pattern

---

## بنية المستودع

```
ACommerce.Libraries/
├── AspNetCore/          # تكامل ASP.NET Core
├── Authentication/      # نظام المصادقة
├── Catalog/             # المنتجات والعروض
├── Clients/             # Client SDKs (15 SDK)
├── Core/                # الأساسيات المشتركة
├── Files/               # نظام الملفات والصور
├── Identity/            # الملفات الشخصية
├── Infrastructure/      # Service Registry
├── Marketplace/         # إدارة البائعين
├── Modules/             # وحدات إضافية
├── Other/               # مكتبات متنوعة
├── Payments/            # نظام المدفوعات
├── Sales/               # السلة والطلبات
├── Shipping/            # نظام الشحن
└── docs/                # التوثيق
```

---

## الفئات الرئيسية

### Core (الأساسيات)
| المكتبة | الوصف |
|---------|-------|
| `SharedKernel.Abstractions` | الكيانات والواجهات الأساسية |
| `SharedKernel.CQRS` | Commands, Queries, Handlers |
| `SharedKernel.Infrastructure.EFCore` | Repository و DbContext |
| `Notifications.Abstractions` | تجريدات الإشعارات |
| `Realtime.Abstractions` | SignalR Abstractions |

### Authentication (المصادقة)
| المكتبة | الوصف |
|---------|-------|
| `Authentication.Abstractions` | IAuthenticationProvider |
| `Authentication.JWT` | JWT Token Provider |
| `Authentication.TwoFactor.Nafath` | مصادقة نفاذ السعودية |
| `Authentication.TwoFactor.Abstractions` | تجريدات المصادقة الثنائية |

### Catalog (الكاتالوج)
| المكتبة | الوصف |
|---------|-------|
| `Catalog.Products` | إدارة المنتجات |
| `Catalog.Categories` | التصنيفات |
| `Catalog.Attributes` | السمات الديناميكية |
| `Catalog.Listings` | عروض المنتجات (Multi-Vendor) |

### Sales (المبيعات)
| المكتبة | الوصف |
|---------|-------|
| `Cart` | سلة التسوق |
| `Orders` | إدارة الطلبات |
| `Orders.Api` | API للطلبات |

### Payments (المدفوعات)
| المكتبة | الوصف |
|---------|-------|
| `Payments.Abstractions` | IPaymentProvider |
| `Payments.Moyasar` | بوابة Moyasar |

### Shipping (الشحن)
| المكتبة | الوصف |
|---------|-------|
| `Shipping.Abstractions` | IShippingProvider |
| `Shipping.Mock` | مزود وهمي للاختبار |

### Files (الملفات)
| المكتبة | الوصف |
|---------|-------|
| `Files.Abstractions` | IStorageProvider, IImageProcessor |
| `Files.Storage.Local` | تخزين محلي |
| `Files.ImageProcessing` | معالجة الصور |

### Notifications (الإشعارات)
| المكتبة | الوصف |
|---------|-------|
| `Notifications.Abstractions` | INotificationChannel |
| `Notifications.Channels.Email` | قناة البريد الإلكتروني |
| `Notifications.Channels.Firebase` | Push Notifications |
| `Notifications.Channels.InApp` | إشعارات داخل التطبيق |

### Modules (الوحدات)
| المكتبة | الوصف |
|---------|-------|
| `Reviews` | نظام التقييمات العام |
| `Localization` | دعم متعدد اللغات |

### Clients (SDKs)
15 Client SDK للتعامل مع الخدمات من التطبيقات الأمامية.

---

## الأنماط المعمارية

### 1. CQRS (Command Query Responsibility Segregation)
```csharp
// Command
public record CreateProductCommand(string Name, decimal Price) : ICommand<ProductDto>;

// Query
public record GetProductQuery(Guid Id) : IQuery<ProductDto?>;

// Handler
public class CreateProductHandler : ICommandHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand command, CancellationToken ct)
    {
        // التنفيذ
    }
}
```

### 2. Repository Pattern
```csharp
public interface IRepository<TEntity> where TEntity : class, IBaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<TEntity>> SearchAsync(SearchRequest request, CancellationToken ct = default);
}
```

### 3. Provider Pattern
```csharp
// تجريد
public interface IPaymentProvider
{
    string ProviderName { get; }
    Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct);
}

// تنفيذ
public class MoyasarProvider : IPaymentProvider
{
    public string ProviderName => "Moyasar";
    // ...
}
```

---

## البدء السريع

### 1. إضافة الحزم
```bash
dotnet add package ACommerce.SharedKernel.Abstractions
dotnet add package ACommerce.SharedKernel.CQRS
dotnet add package ACommerce.SharedKernel.Infrastructure.EFCore
```

### 2. تسجيل الخدمات
```csharp
// Program.cs
builder.Services.AddACommerceCQRS();
builder.Services.AddACommerceEFCore<AppDbContext>();
builder.Services.AddACommerceAuthentication();
```

### 3. استخدام Controllers
```csharp
[Route("api/products")]
public class ProductsController : BaseCrudController<Product, ProductDto, CreateProductDto, UpdateProductDto>
{
    public ProductsController(IRepository<Product> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}
```

---

## التوثيق التفصيلي

يتوفر توثيق تفصيلي لكل مكتبة في مجلد `docs/`:

```
docs/
├── articles/
│   └── Best-Practices.md
├── guides/
│   ├── MAUI-Blazor-Guide.md
│   ├── Microservices-Backend-Guide.md
│   └── Monolith-Backend-Guide.md
└── projects/
    ├── aspnetcore/
    ├── authentication/
    ├── catalog/
    ├── clients/
    ├── core/
    ├── files/
    ├── identity/
    ├── marketplace/
    ├── modules/
    ├── notifications/
    ├── payments/
    ├── sales/
    └── shipping/
```

---

## نسبة التغطية

| الفئة | المشاريع | الموثقة | النسبة |
|-------|---------|---------|--------|
| Core | 10 | 8 | 80% |
| Authentication | 6 | 5 | 83% |
| Catalog | 5 | 4 | 80% |
| Payments | 4 | 3 | 75% |
| Sales | 3 | 2 | 67% |
| Shipping | 4 | 3 | 75% |
| Files | 4 | 4 | 100% |
| Clients | 16 | 2 | 13% |
| Identity | 2 | 1 | 50% |
| Marketplace | 2 | 1 | 50% |
| Notifications | 9 | 3 | 33% |
| Modules | 2 | 2 | 100% |
| AspNetCore | 5 | 2 | 40% |
| **الإجمالي** | **97** | **~45** | **~46%** |

---

## المساهمة في التوثيق

1. اتبع نمط التوثيق الموجود
2. استخدم اللغة العربية
3. أضف أمثلة عملية
4. وثق جميع DTOs والـ Entities

---

## الرخصة
جميع المكتبات ملكية خاصة لـ ACommerce.
