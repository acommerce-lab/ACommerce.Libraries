# 🎯 دليل التجربة الكاملة - من التسجيل إلى الشراء

## 🚀 البداية السريعة

### **1. تشغيل المشروع:**
```bash
cd Examples/ACommerce.MarketplaceApi
dotnet run
```

### **2. فتح Swagger:**
افتح: `https://localhost:5001/swagger`

---

## 👥 المستخدمون التجريبيون

### **الحسابات الجاهزة:**
```
✅ العميل:
   Email: customer@example.com
   Password: 123456

✅ البائع:
   Email: vendor@example.com
   Password: 123456

✅ الأدمن:
   Email: admin@example.com
   Password: 123456
```

---

## 📋 السيناريو الكامل: من التسجيل إلى الشراء

### **الخطوة 1: التعرف على المستخدمين التجريبيين** 🔍

```bash
GET /api/auth/test-users
```

**Response:**
```json
{
  "message": "مستخدمين تجريبيين - كلمة المرور للجميع: 123456",
  "users": [
    { "email": "customer@example.com", "role": "Customer", "name": "أحمد محمد" },
    { "email": "vendor@example.com", "role": "Vendor", "name": "متجر الإلكترونيات" },
    { "email": "admin@example.com", "role": "Admin", "name": "المدير" }
  ]
}
```

---

### **الخطوة 2: تسجيل الدخول** 🔐

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "123456"
}
```

**Response:**
```json
{
  "success": true,
  "token": "mock-token-customer-001",
  "user": {
    "id": "customer-001",
    "email": "customer@example.com",
    "fullName": "أحمد محمد",
    "role": "Customer"
  },
  "message": "تم تسجيل الدخول بنجاح"
}
```

**✅ احفظ الـ token للاستخدام في الطلبات القادمة!**

---

### **الخطوة 3: عرض البروفايل** 👤

```bash
GET /api/profiles
```

**Response:**
```json
[
  {
    "id": "guid-here",
    "userId": "customer-001",
    "type": "Customer",
    "fullName": "أحمد محمد",
    "isActive": true,
    "isVerified": true
  }
]
```

---

### **الخطوة 4: عرض البائعين** 🏪

```bash
GET /api/vendors
```

**Response:**
```json
[
  {
    "id": "vendor-guid",
    "storeName": "متجر الإلكترونيات المتقدم",
    "storeSlug": "electronics-advanced",
    "description": "نوفر أحدث الأجهزة الإلكترونية بأفضل الأسعار",
    "status": "Active",
    "commissionType": "Percentage",
    "commissionValue": 10.0,
    "rating": 4.5
  }
]
```

---

### **الخطوة 5: عرض المنتجات المعروضة** 📦

```bash
GET /api/productlistings
```

**Response:**
```json
[
  {
    "id": "listing-1",
    "vendorId": "vendor-guid",
    "productId": "11111111-1111-1111-1111-111111111111",
    "vendorSku": "PHONE-001",
    "status": "Active",
    "price": 2999.00,
    "compareAtPrice": 3499.00,
    "quantityAvailable": 50,
    "rating": 4.8
  },
  {
    "id": "listing-2",
    "vendorId": "vendor-guid",
    "productId": "22222222-2222-2222-2222-222222222222",
    "vendorSku": "LAPTOP-001",
    "status": "Active",
    "price": 4999.00,
    "compareAtPrice": 5999.00,
    "quantityAvailable": 30,
    "rating": 4.7
  },
  {
    "id": "listing-3",
    "vendorId": "vendor-guid",
    "productId": "33333333-3333-3333-3333-333333333333",
    "vendorSku": "WATCH-001",
    "status": "Active",
    "price": 1299.00,
    "compareAtPrice": 1699.00,
    "quantityAvailable": 100,
    "rating": 4.6
  }
]
```

---

### **الخطوة 6: إضافة للسلة** 🛒

```bash
POST /api/cart/add
Content-Type: application/json
Authorization: Bearer mock-token-customer-001

{
  "userIdOrSessionId": "customer-001",
  "listingId": "listing-1-guid",
  "quantity": 2
}
```

**Response:**
```json
{
  "id": "cart-guid",
  "userIdOrSessionId": "customer-001",
  "items": [
    {
      "listingId": "listing-1-guid",
      "quantity": 2,
      "price": 2999.00
    }
  ],
  "total": 5998.00
}
```

---

### **الخطوة 7: عرض السلة** 👁️

```bash
GET /api/cart/customer-001
Authorization: Bearer mock-token-customer-001
```

**Response:**
```json
{
  "id": "cart-guid",
  "userIdOrSessionId": "customer-001",
  "items": [
    {
      "listingId": "listing-1-guid",
      "quantity": 2,
      "price": 2999.00
    }
  ],
  "couponCode": null,
  "discountAmount": 0,
  "total": 5998.00
}
```

---

### **الخطوة 8: إنشاء طلب** 📝

```bash
POST /api/orders
Content-Type: application/json
Authorization: Bearer mock-token-customer-001

{
  "customerId": "customer-001",
  "items": [
    {
      "listingId": "listing-1-guid",
      "quantity": 2,
      "price": 2999.00
    }
  ],
  "shippingAddress": {
    "fullName": "أحمد محمد",
    "phoneNumber": "+966501234567",
    "addressLine1": "شارع الملك فهد",
    "city": "الرياض",
    "state": "الرياض",
    "postalCode": "12345",
    "country": "SA"
  }
}
```

**Response:**
```json
{
  "id": "order-guid",
  "orderNumber": "ORD-20250124-XXXX",
  "customerId": "customer-001",
  "status": "Draft",
  "subtotal": 5998.00,
  "taxAmount": 899.70,
  "shippingCost": 50.00,
  "total": 6947.70,
  "items": [
    {
      "id": "orderitem-guid",
      "listingId": "listing-1-guid",
      "vendorId": "vendor-guid",
      "quantity": 2,
      "price": 2999.00,
      "commissionAmount": 599.80,
      "vendorAmount": 5398.20
    }
  ]
}
```

---

### **الخطوة 9: تأكيد الطلب** ✅

```bash
POST /api/orders/{order-guid}/confirm
Authorization: Bearer mock-token-customer-001
```

**Response:**
```json
{
  "id": "order-guid",
  "orderNumber": "ORD-20250124-XXXX",
  "status": "Confirmed",
  "message": "تم تأكيد الطلب بنجاح"
}
```

---

### **الخطوة 10: شحن الطلب (كبائع)** 🚚

```bash
# أولاً: سجل دخول كبائع
POST /api/auth/login
{
  "email": "vendor@example.com",
  "password": "123456"
}

# ثم: شحن الطلب
POST /api/orders/{order-guid}/ship
Authorization: Bearer mock-token-vendor-001
Content-Type: application/json

{
  "trackingNumber": "TRACK123456789"
}
```

**Response:**
```json
{
  "id": "order-guid",
  "orderNumber": "ORD-20250124-XXXX",
  "status": "Shipped",
  "trackingNumber": "TRACK123456789",
  "message": "تم شحن الطلب بنجاح"
}
```

---

### **الخطوة 11: عرض طلبات العميل** 📋

```bash
GET /api/orders/customer/customer-001
Authorization: Bearer mock-token-customer-001
```

**Response:**
```json
[
  {
    "id": "order-guid",
    "orderNumber": "ORD-20250124-XXXX",
    "customerId": "customer-001",
    "status": "Shipped",
    "total": 6947.70,
    "trackingNumber": "TRACK123456789",
    "createdAt": "2025-01-24T..."
  }
]
```

---

### **الخطوة 12: عرض طلبات البائع** 🏪

```bash
GET /api/orders/vendor/vendor-guid
Authorization: Bearer mock-token-vendor-001
```

**Response:**
```json
[
  {
    "id": "order-guid",
    "orderNumber": "ORD-20250124-XXXX",
    "vendorId": "vendor-guid",
    "status": "Shipped",
    "vendorAmount": 5398.20,
    "commissionAmount": 599.80
  }
]
```

---

## 🎯 سيناريوهات إضافية

### **تسجيل حساب جديد:**
```bash
POST /api/auth/register
Content-Type: application/json

{
  "email": "newcustomer@example.com",
  "password": "mypassword",
  "fullName": "عميل جديد",
  "role": "Customer"
}
```

### **إضافة بائع جديد:**
```bash
# 1. سجل حساب
POST /api/auth/register
{
  "email": "newvendor@example.com",
  "password": "password",
  "fullName": "متجر جديد",
  "role": "Vendor"
}

# 2. أنشئ Profile
POST /api/profiles
{
  "userId": "new-user-id",
  "type": "Vendor",
  "fullName": "متجر جديد",
  "businessName": "المتجر الجديد"
}

# 3. سجل البائع
POST /api/vendors
{
  "profileId": "new-profile-guid",
  "storeName": "المتجر الجديد",
  "storeSlug": "new-store",
  "commissionType": "Percentage",
  "commissionValue": 12.0
}
```

### **إضافة منتج معروض:**
```bash
POST /api/productlistings
Authorization: Bearer mock-token-vendor-001
Content-Type: application/json

{
  "vendorId": "vendor-guid",
  "productId": "44444444-4444-4444-4444-444444444444",
  "vendorSku": "HEADPHONES-001",
  "status": "Active",
  "price": 499.00,
  "compareAtPrice": 699.00,
  "quantityAvailable": 200,
  "processingTime": 1
}
```

---

## 🔥 نصائح الاستخدام

### **1. استخدم Swagger UI:**
- افتح `/swagger` لتجربة جميع APIs بشكل تفاعلي
- يمكنك نسخ/لصق الـ token في "Authorize" button

### **2. Mock Authentication:**
- الـ tokens بصيغة: `mock-token-{userId}`
- في الإنتاج، استخدم `ACommerce.Authentication.JWT`

### **3. Seed Data:**
- البيانات التجريبية تُحمّل تلقائياً عند التشغيل
- 3 مستخدمين + 1 بائع + 3 منتجات معروضة

### **4. Testing Flow:**
```
Register → Login → Browse Products → Add to Cart → Create Order → Confirm → Ship → Deliver
```

---

## 📊 البيانات التجريبية المتاحة

### **Users:**
- 3 مستخدمين (Customer, Vendor, Admin)

### **Profiles:**
- 3 profiles مربوطة بالمستخدمين

### **Vendors:**
- 1 بائع ("متجر الإلكترونيات المتقدم")

### **Product Listings:**
- 3 منتجات معروضة (Phone, Laptop, Watch)

---

## 🎉 النتيجة

```
✅ Backend كامل مع Authentication
✅ Seed data جاهز للتجربة
✅ Flow كامل من التسجيل إلى الشراء
✅ Multi-Vendor system يعمل
✅ Commission calculation تلقائي
✅ Order tracking متاح
✅ كل شيء جاهز للاختبار!
```

---

**Built with ❤️ using ACommerce.Libraries**
