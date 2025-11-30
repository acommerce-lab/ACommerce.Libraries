# ACommerce.Libraries - فهرس التوثيق

## نظرة عامة
توثيق شامل لمكتبات ACommerce.Libraries - حل متكامل للتجارة الإلكترونية متعدد البائعين.

---

## الفئات الرئيسية

### Core (الأساسيات)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| SharedKernel.Abstractions | الكيانات والواجهات الأساسية | `/Core` |
| SharedKernel.CQRS | Commands, Queries, Handlers | `/Core` |
| SharedKernel.Infrastructure.EFCores | Repository و DbContext | `/Core` |
| Configuration | الإعدادات (Store, Vendor) | `/Core` |
| Notifications.Abstractions | الإشعارات متعددة القنوات | `/Core` |
| Realtime.Abstractions | SignalR Abstractions | `/Core` |
| Chats.Abstractions | نظام المحادثات | `/Core` |

### Authentication (المصادقة)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Authentication.Abstractions | IAuthenticationProvider | `/Authentication` |
| Authentication.JWT | JWT Token Provider | `/Authentication` |
| Authentication.TwoFactor.Nafath | مصادقة نفاذ | `/Authentication` |
| Authentication.TwoFactor.Abstractions | تجريدات 2FA | `/Authentication` |

### Catalog (الكاتالوج)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Catalog.Listings | عروض المنتجات (Multi-Vendor) | `/Catalog` |
| Catalog.Listings.Api | API للعروض | `/Catalog` |

### Sales (المبيعات)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Cart | سلة التسوق | `/Sales` |
| Orders | الطلبات | `/Sales` |
| Orders.Api | API للطلبات | `/Sales` |

### Payments (المدفوعات)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Payments.Abstractions | IPaymentProvider | `/Payments` |
| Payments.Moyasar | بوابة Moyasar | `/Payments` |
| Payments.Api | API للمدفوعات | `/Payments` |

### Shipping (الشحن)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Shipping.Abstractions | IShippingProvider | `/Shipping` |
| Shipping.Mock | مزود وهمي للاختبار | `/Shipping` |
| Shipping.Api | API للشحن | `/Shipping` |

### Files (الملفات)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Files.Abstractions | IStorageProvider, IImageProcessor | `/Files` |
| Files.Storage.Local | تخزين محلي | `/Files` |
| Files.ImageProcessing | معالجة الصور (ImageSharp) | `/Files` |

### Clients (SDKs العميل)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Client.Core | IApiClient, Interceptors | `/Clients` |
| Client.* (15 SDK) | SDKs للخدمات المختلفة | `/Clients` |

### Infrastructure (البنية التحتية)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| ServiceRegistry.* | Service Discovery | `/Infrastructure` |

### Identity (الهوية)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Profiles | الملفات الشخصية | `/Identity` |
| Profiles.Api | API للبروفايلات | `/Identity` |

### Messaging (الرسائل)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Messaging.Abstractions | IMessageBus, Pub/Sub | Root |
| Messaging.InMemory | للاختبار | Root |
| Messaging.SignalR | للاتصال اللحظي | Root |

### Marketplace (السوق)
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| Vendors | إدارة البائعين | `/Marketplace` |
| Vendors.Api | API للبائعين | `/Marketplace` |

### AspNetCore
| المكتبة | الوصف | الموقع |
|---------|-------|--------|
| SharedKernel.AspNetCore | BaseCrudController | `/AspNetCore` |
| Authentication.AspNetCore | Controllers للمصادقة | `/AspNetCore` |
| Files.AspNetCore | Controllers للملفات | `/AspNetCore` |

---

## الأنماط المستخدمة

1. **CQRS** - فصل القراءة عن الكتابة
2. **Repository Pattern** - تجريد الوصول للبيانات
3. **Provider Pattern** - تبديل التنفيذات
4. **Soft Delete** - حذف منطقي
5. **Domain Events** - أحداث المجال
6. **SmartSearch** - بحث متقدم مع فلترة

---

## كيفية الاستخدام

1. اختر المكتبات المناسبة لمشروعك
2. أضفها كـ NuGet packages
3. سجل الخدمات في `Program.cs`
4. استخدم الـ Base Controllers أو أنشئ خاصتك

---

## ملاحظات
- جميع المكتبات تعتمد على .NET 9.0
- التوثيق باللغة العربية
- يمكن استخدام هذا التوثيق كمصدر بيانات للـ AI Agent
