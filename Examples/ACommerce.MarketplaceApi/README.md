# ACommerce Marketplace API

## 🎯 نظام تجارة إلكترونية متعدد البائعين - مبني بالكامل على ACommerce.Libraries

### ✨ الميزات:

- ✅ **Profiles**: إدارة العملاء والبائعين
- ✅ **Vendors**: إدارة البائعين مع نظام عمولات
- ✅ **Product Listings**: عروض المنتجات من البائعين
- ✅ **Cart**: سلة التسوق
- ✅ **Orders**: نظام الطلبات الكامل
- ✅ **Payments**: دعم Moyasar (بوابة دفع سعودية)
- ✅ **Shipping**: نظام شحن قابل للتبديل
- ✅ **Reviews**: تقييمات المنتجات والبائعين
- ✅ **Localization**: دعم متعدد اللغات

### 🚀 التشغيل:

```bash
cd Examples/ACommerce.MarketplaceApi
dotnet run
```

ثم افتح: https://localhost:5001/swagger

### 📊 API Endpoints:

#### Profiles
- `GET /api/profiles` - قائمة البروفايلات
- `POST /api/profiles` - إنشاء بروفايل
- `GET /api/profiles/{id}` - تفاصيل بروفايل

#### Vendors
- `GET /api/vendors` - قائمة البائعين
- `POST /api/vendors` - تسجيل بائع جديد
- `GET /api/vendors/by-slug/{slug}` - بائع بالـ slug

#### Product Listings
- `GET /api/productlistings` - جميع العروض
- `GET /api/productlistings/by-product/{productId}` - عروض منتج معين
- `GET /api/productlistings/by-vendor/{vendorId}` - عروض بائع معين
- `POST /api/productlistings` - إنشاء عرض جديد

#### Cart
- `POST /api/cart/add` - إضافة للسلة
- `GET /api/cart/{userIdOrSessionId}` - عرض السلة
- `DELETE /api/cart/{userIdOrSessionId}` - إفراغ السلة

#### Orders
- `GET /api/orders` - قائمة الطلبات
- `POST /api/orders` - إنشاء طلب
- `GET /api/orders/customer/{customerId}` - طلبات العميل
- `GET /api/orders/vendor/{vendorId}` - طلبات البائع
- `POST /api/orders/{id}/confirm` - تأكيد طلب
- `POST /api/orders/{id}/ship` - شحن طلب

### 🏗️ البنية:

```
Program.cs (50 lines)
├── Controllers (من المكتبات - صفر كود!)
├── CQRS (من المكتبات)
├── Repositories (من المكتبات)
├── Payment Provider (Moyasar)
└── Shipping Provider (Mock)
```

**النتيجة:** Backend كامل في **~50 سطر فقط!**

### 📦 المكتبات المستخدمة:

- ACommerce.Profiles
- ACommerce.Vendors
- ACommerce.Catalog.Listings
- ACommerce.Cart
- ACommerce.Orders
- ACommerce.Payments.Moyasar
- ACommerce.Shipping.Mock
- ACommerce.Reviews
- ACommerce.Localization

### ⚙️ الإعدادات:

كل شيء قابل للتهيئة عبر `appsettings.json`:
- معلومات الدفع (Moyasar)
- إعدادات المتجر
- قاعدة البيانات (InMemory للتجربة، SQL/Postgres للإنتاج)

### 🔧 التوسع:

1. **إضافة Authentication**: استخدم `ACommerce.Authentication.JWT`
2. **تغيير Database**: استبدل `InMemory` بـ SQL Server/PostgreSQL
3. **إضافة Notifications**: استخدم `ACommerce.Notifications`
4. **إضافة Chat**: استخدم `ACommerce.Chats`
5. **إضافة Products من الكتالوج**: استخدم `ACommerce.Catalog.Products`

---

**هذا مثال حي على قوة المكتبات - متجر كامل في ملف واحد!**
