# 🛍️ ACommerce Marketplace API

## 🎯 متجر متعدد البائعين كامل - مبني بالكامل على ACommerce.Libraries

### ✨ الميزات الكاملة:

#### **🔐 Authentication (NEW!)**
- ✅ تسجيل الدخول (Login)
- ✅ إنشاء حساب (Register)
- ✅ مستخدمين تجريبيين جاهزين
- ✅ Mock tokens للتجربة

#### **👥 User Management**
- ✅ **Profiles**: إدارة العملاء والبائعين والأدمن
- ✅ أنواع متعددة: Customer, Vendor, Admin, Employee, Support

#### **🏪 Vendor Management**
- ✅ تسجيل البائعين
- ✅ نظام عمولات مرن (Percentage/Fixed/Hybrid)
- ✅ إدارة الأرصدة (Available/Pending)
- ✅ تقييمات البائعين

#### **📦 Product Catalog (NEW!)**
- ✅ **Products**: إدارة المنتجات من الكتالوج
- ✅ **Product Listings**: عروض البائعين للمنتجات
- ✅ إدارة المخزون
- ✅ الأسعار والخصومات

#### **🛒 Shopping Experience**
- ✅ **Cart**: سلة التسوق (تدعم الضيوف)
- ✅ **Orders**: نظام طلبات كامل
- ✅ تتبع الطلبات (11 حالة)
- ✅ حساب العمولات تلقائياً

#### **💳 Payments & Shipping**
- ✅ **Payments**: دعم Moyasar (بوابة دفع سعودية)
- ✅ **Shipping**: نظام شحن قابل للتبديل
- ✅ Tracking numbers

#### **📝 Additional Features**
- ✅ **Reviews**: تقييمات المنتجات والبائعين
- ✅ **Localization**: دعم متعدد اللغات

---

## 🚀 البداية السريعة

### **1. التشغيل:**
```bash
cd Examples/ACommerce.MarketplaceApi
dotnet run
```

### **2. افتح Swagger:**
```
https://localhost:5001/swagger
```

### **3. افتح الصفحة الرئيسية:**
```
https://localhost:5001/
```

**ستجد:**
- معلومات عن API
- قائمة المستخدمين التجريبيين
- خطوات البداية السريعة
- جميع الـ endpoints المتاحة

---

## 👥 المستخدمون التجريبيون

### **الحسابات الجاهزة (كلمة المرور للجميع: `123456`):**

```
✅ العميل:
   Email: customer@example.com
   Role: Customer
   Name: أحمد محمد

✅ البائع:
   Email: vendor@example.com
   Role: Vendor
   Name: متجر الإلكترونيات

✅ الأدمن:
   Email: admin@example.com
   Role: Admin
   Name: المدير
```

### **عرض المستخدمين:**
```bash
GET /api/auth/test-users
```

---

## 📊 API Endpoints

### **🔐 Authentication**
```
POST   /api/auth/login          - تسجيل الدخول
POST   /api/auth/register       - إنشاء حساب جديد
GET    /api/auth/me             - معلومات المستخدم الحالي
GET    /api/auth/test-users     - قائمة المستخدمين التجريبيين
```

### **👤 Profiles**
```
GET    /api/profiles            - قائمة البروفايلات
POST   /api/profiles            - إنشاء بروفايل
GET    /api/profiles/{id}       - تفاصيل بروفايل
PUT    /api/profiles/{id}       - تحديث بروفايل
DELETE /api/profiles/{id}       - حذف بروفايل
```

### **🏪 Vendors**
```
GET    /api/vendors                    - قائمة البائعين
POST   /api/vendors                    - تسجيل بائع جديد
GET    /api/vendors/{id}               - تفاصيل بائع
GET    /api/vendors/by-slug/{slug}     - بائع بالـ slug
POST   /api/vendors/{id}/activate      - تفعيل بائع
POST   /api/vendors/{id}/suspend       - تعليق بائع
```

### **📦 Products**
```
GET    /api/products            - قائمة المنتجات
POST   /api/products            - إضافة منتج
GET    /api/products/{id}       - تفاصيل منتج
PUT    /api/products/{id}       - تحديث منتج
DELETE /api/products/{id}       - حذف منتج
```

### **🏷️ Product Listings**
```
GET    /api/productlistings                         - جميع العروض
POST   /api/productlistings                         - إنشاء عرض جديد
GET    /api/productlistings/{id}                    - تفاصيل عرض
GET    /api/productlistings/by-product/{productId}  - عروض منتج معين
GET    /api/productlistings/by-vendor/{vendorId}    - عروض بائع معين
```

### **🛒 Cart**
```
POST   /api/cart/add                    - إضافة للسلة
PUT    /api/cart/update                 - تحديث كمية
GET    /api/cart/{userIdOrSessionId}    - عرض السلة
DELETE /api/cart/{userIdOrSessionId}    - إفراغ السلة
```

### **📦 Orders**
```
GET    /api/orders                       - قائمة الطلبات
POST   /api/orders                       - إنشاء طلب
GET    /api/orders/{id}                  - تفاصيل طلب
GET    /api/orders/customer/{customerId} - طلبات العميل
GET    /api/orders/vendor/{vendorId}     - طلبات البائع
POST   /api/orders/{id}/confirm          - تأكيد طلب
POST   /api/orders/{id}/ship             - شحن طلب
POST   /api/orders/{id}/cancel           - إلغاء طلب
```

---

## 🎯 السيناريو الكامل

### **دليل شامل من التسجيل إلى الشراء:**
📖 **[اقرأ الدليل الكامل](COMPLETE_FLOW_GUIDE.md)**

### **الخطوات السريعة:**

```
1. تسجيل الدخول → POST /api/auth/login
2. عرض المنتجات → GET /api/productlistings
3. إضافة للسلة → POST /api/cart/add
4. إنشاء طلب → POST /api/orders
5. تأكيد الطلب → POST /api/orders/{id}/confirm
6. شحن الطلب → POST /api/orders/{id}/ship
```

---

## 🏗️ البنية

### **Program.cs (~113 سطر):**
```
Program.cs
├── Controllers (من المكتبات!)
│   ├── Profiles
│   ├── Vendors
│   ├── Products ✨ NEW
│   ├── ProductListings
│   ├── Cart
│   └── Orders
│
├── Custom Controllers (يدوي)
│   └── AuthController ✨ NEW (للتجربة فقط)
│
├── Services
│   ├── MockAuthService ✨ NEW
│   └── SeedDataService ✨ NEW
│
├── CQRS (من المكتبات)
├── Repositories (من المكتبات)
├── Payment Provider (Moyasar)
└── Shipping Provider (Mock)
```

### **البيانات التجريبية (Seed Data):**
```
✅ 3 Users (Customer, Vendor, Admin)
✅ 3 Profiles
✅ 1 Vendor (متجر الإلكترونيات)
✅ 3 Product Listings (Phone, Laptop, Watch)
```

---

## 📦 المكتبات المستخدمة

### **Core:**
- ACommerce.SharedKernel.Abstractions
- ACommerce.SharedKernel.CQRS
- ACommerce.SharedKernel.Infrastructure.EFCores
- ACommerce.Configuration

### **Identity:**
- ACommerce.Profiles
- ACommerce.Profiles.Api

### **Marketplace:**
- ACommerce.Vendors
- ACommerce.Vendors.Api

### **Catalog:**
- ACommerce.Catalog.Products ✨ **NEW**
- ACommerce.Catalog.Products.Api ✨ **NEW**
- ACommerce.Catalog.Listings
- ACommerce.Catalog.Listings.Api

### **Sales:**
- ACommerce.Cart
- ACommerce.Orders
- ACommerce.Orders.Api

### **Payments & Shipping:**
- ACommerce.Payments.Abstractions
- ACommerce.Payments.Moyasar
- ACommerce.Shipping.Abstractions
- ACommerce.Shipping.Mock

### **Modules:**
- ACommerce.Reviews
- ACommerce.Localization

---

## ⚙️ الإعدادات

### **appsettings.json:**
```json
{
  "Moyasar": {
    "ApiKey": "YOUR_API_KEY",
    "PublishableKey": "YOUR_PUBLISHABLE_KEY"
  },
  "Store": {
    "Name": "ACommerce Marketplace",
    "DefaultCurrency": "SAR",
    "DefaultLanguage": "ar",
    "EnableMultiVendor": true
  }
}
```

---

## 🔧 التوسع

### **1. Authentication حقيقي:**
استبدل `MockAuthService` بـ:
```csharp
builder.Services.AddJwtAuthentication(builder.Configuration);
```

### **2. قاعدة بيانات حقيقية:**
استبدل `InMemory` بـ SQL Server:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### **3. Payment Provider حقيقي:**
```csharp
// استبدل Mock config بـ Moyasar config حقيقي
options.ApiKey = builder.Configuration["Moyasar:ApiKey"];
options.UseSandbox = false; // للإنتاج
```

### **4. Shipping Provider حقيقي:**
```csharp
// استبدل Mock بـ Aramex أو SMSA
builder.Services.AddScoped<IShippingProvider, AramexShippingProvider>();
```

### **5. إضافة Notifications:**
```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

---

## 📈 الإحصائيات

```
✅ 113 سطر في Program.cs
✅ 1 Custom Controller (Auth - للتجربة فقط)
✅ 40+ API Endpoints جاهزة
✅ 0 Business Logic يدوي
✅ كل شيء من المكتبات!
```

---

## 🎯 ما تحصل عليه مجاناً

```
✓ Multi-Vendor System كامل
✓ Authentication & Authorization
✓ User Management (Profiles)
✓ Vendor Management مع عمولات
✓ Product Catalog
✓ Shopping Cart (guest support)
✓ Orders Management (11 states)
✓ Payment Integration (Moyasar)
✓ Shipping Integration (extensible)
✓ Commission Calculation (automatic)
✓ Order Tracking
✓ Reviews & Ratings
✓ Localization Support
✓ Seed Data للتجربة الفورية
✓ Swagger UI
✓ CQRS Pattern
✓ Repository Pattern
✓ Provider Pattern
```

---

## 💡 نصائح

### **1. استخدم Swagger UI:**
- واجهة تفاعلية لجميع APIs
- يمكنك نسخ الـ token وتجربته مباشرة

### **2. ابدأ بالمستخدمين التجريبيين:**
- جرّب `/api/auth/test-users` أولاً
- سجل دخول بأحد الحسابات الجاهزة

### **3. اتبع السيناريو الكامل:**
- راجع [COMPLETE_FLOW_GUIDE.md](COMPLETE_FLOW_GUIDE.md)
- جرب كل خطوة من التسجيل إلى الشراء

### **4. Mock Authentication:**
- للتجربة فقط!
- في الإنتاج، استخدم `ACommerce.Authentication.JWT`

---

## 🎉 الخلاصة

```
┌──────────────────────────────────────────────┐
│                                              │
│  متجر متعدد البائعين كامل في ملف واحد!    │
│                                              │
│  ✓ 113 سطر = Backend كامل                  │
│  ✓ 40+ API Endpoints                        │
│  ✓ Authentication + Seed Data               │
│  ✓ جاهز للتجربة الفورية                    │
│  ✓ جاهز للإنتاج بتعديلات بسيطة              │
│                                              │
│  هذه قوة ACommerce.Libraries! 🚀            │
│                                              │
└──────────────────────────────────────────────┘
```

---

**Built with ❤️ using ACommerce.Libraries - من الصفر إلى متجر في دقائق!**
