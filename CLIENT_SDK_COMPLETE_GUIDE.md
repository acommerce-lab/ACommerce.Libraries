# 📱 ACommerce Client SDKs - الدليل الشامل الكامل

## 📋 نظرة عامة

**14 مكتبة .NET Client SDK** كاملة للتواصل مع جميع خدمات ACommerce من تطبيقات:
- **.NET MAUI** (iOS, Android, Windows, macOS)
- **Blazor** (WebAssembly, Server, Hybrid)
- **WPF** / **WinForms**
- **ASP.NET** (Server-side)
- **Console Apps**

---

## 📦 المكتبات الكاملة (14 مكتبة)

### 🔹 Core & Infrastructure

#### **1. ACommerce.Client.Core** ⭐
الطبقة الأساسية - مطلوبة لجميع المكتبات

**المكونات:**
- `DynamicHttpClient` - HTTP Client مع Dynamic Service URLs
- `AuthenticationInterceptor` - إضافة Token تلقائياً
- `RetryInterceptor` - إعادة محاولة مع Exponential Backoff
- `LocalizationInterceptor` - إرسال اللغة الحالية في Headers
- Integration كامل مع Service Registry

**الاستخدام:**
```csharp
builder.Services.AddACommerceClient("http://localhost:5100", options =>
{
    options.EnableAuthentication = true;
    options.EnableLocalization = true;
    options.EnableRetry = true;
    options.MaxRetries = 3;
    options.TimeoutSeconds = 30;
});
```

---

### 🔹 Authentication & User Management

#### **2. ACommerce.Client.Auth**
مكتبة Authentication كاملة

**APIs:**
- `LoginAsync` - تسجيل دخول
- `RegisterAsync` - إنشاء حساب
- `GetMeAsync` - معلومات المستخدم الحالي
- `LogoutAsync` - تسجيل خروج

**TokenManager:**
- حفظ Token آمن
- التحقق من Expiry
- Clear Token

**مثال:**
```csharp
// التسجيل
builder.Services.AddAuthClient("http://localhost:5100");

// الاستخدام
var response = await authClient.LoginAsync(new LoginRequest
{
    Username = "user@example.com",
    Password = "password123"
});

if (response != null)
{
    tokenManager.SetToken(response.Token, response.ExpiresAt);
    // تلقائياً سيتم إضافة Token لكل طلب
}
```

#### **3. ACommerce.Client.Profiles**
إدارة الملفات الشخصية

**APIs:**
- `GetMyProfileAsync` - ملفي الشخصي
- `GetByIdAsync` - ملف شخصي محدد
- `CreateAsync` - إنشاء ملف جديد
- `UpdateAsync` - تحديث الملف

---

### 🔹 Products & Catalog

#### **4. ACommerce.Client.Products**
إدارة المنتجات

**APIs:**
- `GetAllAsync` - جميع المنتجات
- `GetByIdAsync` - منتج محدد
- `CreateAsync` - إضافة منتج
- `UpdateAsync` - تحديث منتج
- `DeleteAsync` - حذف منتج

#### **5. ACommerce.Client.ProductListings**
إدارة المعروضات (Vendor Listings)

**APIs:**
- `GetAllAsync` - جميع المعروضات
- `GetByIdAsync` - معروض محدد
- `GetByVendorAsync` - معروضات تاجر محدد
- `CreateAsync` - إضافة معروض
- `UpdateAsync` - تحديث معروض
- `DeleteAsync` - حذف معروض

**مثال:**
```csharp
// الحصول على منتجات تاجر معين
var listings = await productListingsClient.GetByVendorAsync(vendorId);

foreach (var listing in listings)
{
    Console.WriteLine($"{listing.ProductName}: {listing.Price:C}");
}
```

---

### 🔹 Shopping & Orders

#### **6. ACommerce.Client.Cart** 🛒
إدارة سلة التسوق الكاملة

**APIs:**
- `GetCartAsync` - الحصول على السلة
- `AddToCartAsync` - إضافة منتج
- `UpdateCartItemAsync` - تحديث الكمية
- `RemoveItemAsync` - حذف منتج
- `ClearCartAsync` - إفراغ السلة
- `ApplyCouponAsync` - تطبيق كود خصم
- `RemoveCouponAsync` - إزالة كود الخصم

**مثال Blazor:**
```csharp
@inject CartClient CartClient

<button @onclick="AddToCart">Add to Cart</button>

@code {
    private async Task AddToCart()
    {
        var cart = await CartClient.AddToCartAsync(new AddToCartRequest
        {
            UserIdOrSessionId = userId,
            ListingId = productId,
            Quantity = 1
        });

        if (cart != null)
        {
            Console.WriteLine($"Cart Total: {cart.Total:C}");
        }
    }
}
```

**مثال MAUI:**
```csharp
public class CartViewModel : ObservableObject
{
    private readonly CartClient _cartClient;
    private CartResponse? _cart;

    [RelayCommand]
    public async Task AddToCartAsync(Guid listingId)
    {
        Cart = await _cartClient.AddToCartAsync(new AddToCartRequest
        {
            UserIdOrSessionId = GetUserId(),
            ListingId = listingId,
            Quantity = 1
        });
    }

    [RelayCommand]
    public async Task ApplyCouponAsync(string couponCode)
    {
        Cart = await _cartClient.ApplyCouponAsync(
            GetUserId(),
            new ApplyCouponRequest { CouponCode = couponCode }
        );
    }
}
```

#### **7. ACommerce.Client.Orders**
إدارة الطلبات

**APIs:**
- `GetAllAsync` - جميع الطلبات
- `GetByIdAsync` - طلب محدد
- `CreateAsync` - إنشاء طلب جديد
- `UpdateStatusAsync` - تحديث حالة الطلب
- `CancelAsync` - إلغاء طلب

---

### 🔹 Payments & Shipping

#### **8. ACommerce.Client.Payments** 💳
إدارة المدفوعات الكاملة

**APIs:**
- `CreatePaymentAsync` - إنشاء دفعة
- `GetPaymentStatusAsync` - حالة الدفع
- `CancelPaymentAsync` - إلغاء دفعة
- `RefundPaymentAsync` - استرجاع مبلغ

**Payment Methods:**
- Credit Card
- PayPal
- Mada (السعودية)
- Apple Pay
- Google Pay

**مثال:**
```csharp
// إنشاء دفعة
var payment = await paymentsClient.CreatePaymentAsync(new CreatePaymentRequest
{
    OrderId = orderId,
    Amount = 299.99m,
    Currency = "SAR",
    PaymentMethod = "Mada"
});

if (payment?.PaymentUrl != null)
{
    // توجيه للمستخدم لصفحة الدفع
    await Browser.OpenAsync(payment.PaymentUrl);
}

// التحقق من حالة الدفع
var status = await paymentsClient.GetPaymentStatusAsync(payment.PaymentId);
if (status?.Status == "Completed")
{
    Console.WriteLine("✅ Payment successful!");
}
```

#### **9. ACommerce.Client.Shipping** 📦
إدارة الشحن الكاملة

**APIs:**
- `CalculateShippingAsync` - حساب تكلفة الشحن
- `CreateShipmentAsync` - إنشاء شحنة
- `TrackShipmentAsync` - تتبع الشحنة
- `GetProvidersAsync` - شركات الشحن المتاحة

**Shipping Providers:**
- SMSA
- Aramex
- DHL
- FedEx

**مثال:**
```csharp
// حساب تكلفة الشحن
var rates = await shippingClient.CalculateShippingAsync(new ShippingRateRequest
{
    FromCity = "Riyadh",
    ToCity = "Jeddah",
    Weight = 2.5m // كيلوجرام
});

foreach (var rate in rates.Rates)
{
    Console.WriteLine($"{rate.Provider} - {rate.ServiceType}: {rate.Cost:C} ({rate.EstimatedDays} days)");
}

// إنشاء شحنة
var shipment = await shippingClient.CreateShipmentAsync(new CreateShipmentRequest
{
    OrderId = orderId,
    ShippingProvider = "SMSA",
    ServiceType = "Express",
    FromAddress = vendorAddress,
    ToAddress = customerAddress,
    Weight = 2.5m
});

// تتبع الشحنة
var tracking = await shippingClient.TrackShipmentAsync(shipment.TrackingNumber);
foreach (var ev in tracking.Events)
{
    Console.WriteLine($"{ev.Timestamp}: {ev.Status} - {ev.Location}");
}
```

---

### 🔹 Vendors & Management

#### **10. ACommerce.Client.Vendors**
إدارة التجار

**APIs:**
- `GetAllAsync` - جميع التجار
- `GetByIdAsync` - تاجر محدد
- `CreateAsync` - إضافة تاجر
- `UpdateAsync` - تحديث تاجر
- `DeleteAsync` - حذف تاجر

---

### 🔹 Communication

#### **11. ACommerce.Client.Notifications** 🔔
إدارة الإشعارات

**APIs:**
- `GetNotificationsAsync` - الإشعارات مع Pagination
- `GetUnreadCountAsync` - عدد غير المقروءة
- `MarkAsReadAsync` - تعليم كمقروء
- `MarkAllAsReadAsync` - تعليم الكل كمقروء
- `DeleteNotificationAsync` - حذف إشعار
- `RegisterDeviceTokenAsync` - تسجيل للـ Push Notifications

**مثال MAUI:**
```csharp
public class NotificationsViewModel : ObservableObject
{
    private readonly NotificationsClient _notificationsClient;

    [ObservableProperty]
    private ObservableCollection<NotificationResponse> notifications = new();

    [ObservableProperty]
    private int unreadCount;

    public async Task LoadNotificationsAsync()
    {
        var notifs = await _notificationsClient.GetNotificationsAsync();
        Notifications = new ObservableCollection<NotificationResponse>(notifs);

        var count = await _notificationsClient.GetUnreadCountAsync();
        UnreadCount = count?.Count ?? 0;
    }

    [RelayCommand]
    public async Task MarkAsReadAsync(Guid notificationId)
    {
        await _notificationsClient.MarkAsReadAsync(notificationId);
        await LoadNotificationsAsync();
    }
}
```

**Push Notifications (MAUI):**
```csharp
// في MauiProgram.cs
builder.Services.AddNotificationsClient("http://localhost:5100");

// في App.xaml.cs
protected override async void OnStart()
{
    var token = await GetDeviceTokenAsync(); // من Firebase/APNS
    await notificationsClient.RegisterDeviceTokenAsync(new RegisterDeviceTokenRequest
    {
        DeviceToken = token,
        Platform = DeviceInfo.Platform.ToString()
    });
}
```

#### **12. ACommerce.Client.Chats** 💬
إدارة الدردشة

**APIs:**
- `GetConversationsAsync` - المحادثات
- `GetConversationAsync` - محادثة محددة
- `GetMessagesAsync` - الرسائل مع Pagination
- `SendMessageAsync` - إرسال رسالة
- `StartConversationAsync` - بدء محادثة جديدة
- `MarkAsReadAsync` - تعليم كمقروء

**Message Types:**
- Text
- Image
- File

**مثال Blazor:**
```razor
@page "/chat/{ConversationId:guid}"
@inject ChatsClient ChatsClient

<div class="chat-container">
    @foreach (var msg in messages)
    {
        <div class="message @(msg.SenderId == currentUserId ? "mine" : "theirs")">
            <strong>@msg.SenderName</strong>
            <p>@msg.Content</p>
            <small>@msg.CreatedAt.ToString("HH:mm")</small>
        </div>
    }
</div>

<input @bind="newMessage" placeholder="Type a message..." />
<button @onclick="SendMessage">Send</button>

@code {
    [Parameter] public Guid ConversationId { get; set; }
    private List<MessageResponse> messages = new();
    private string newMessage = "";
    private string currentUserId = "user123";

    protected override async Task OnInitializedAsync()
    {
        messages = await ChatsClient.GetMessagesAsync(ConversationId) ?? new();
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(newMessage)) return;

        await ChatsClient.SendMessageAsync(ConversationId, new SendMessageRequest
        {
            Content = newMessage,
            Type = "Text"
        });

        newMessage = "";
        await LoadMessagesAsync();
    }
}
```

#### **13. ACommerce.Client.Realtime** ⚡
SignalR للتحديثات الفورية

**APIs:**
- `ConnectAsync` - الاتصال بـ Hub
- `On<T>` - الاستماع لحدث
- `SendAsync` - إرسال رسالة
- `InvokeAsync<T>` - استدعاء method مع Response
- `DisconnectAsync` - قطع الاتصال

**Features:**
- Automatic Reconnection
- Connection State Management
- Type-safe Events

**مثال:**
```csharp
public class RealtimeService
{
    private readonly RealtimeClient _realtimeClient;

    public async Task InitializeAsync()
    {
        await _realtimeClient.ConnectAsync(
            serviceName: "Marketplace",
            hubPath: "/hubs/notifications"
        );

        // الاستماع للإشعارات الجديدة
        _realtimeClient.On<NotificationResponse>("ReceiveNotification", notification =>
        {
            Console.WriteLine($"New notification: {notification.Title}");
            // تحديث UI
        });

        // الاستماع لتحديثات الطلبات
        _realtimeClient.On<OrderUpdate>("OrderStatusChanged", update =>
        {
            Console.WriteLine($"Order {update.OrderId}: {update.NewStatus}");
        });

        // الاستماع للرسائل الجديدة
        _realtimeClient.On<MessageResponse>("ReceiveMessage", message =>
        {
            Console.WriteLine($"New message from {message.SenderName}");
        });
    }

    public async Task SendTypingIndicatorAsync(Guid conversationId)
    {
        await _realtimeClient.SendAsync("Typing", conversationId);
    }
}
```

---

### 🔹 Files & Media

#### **14. ACommerce.Client.Files** 📁
إدارة الملفات والصور

**APIs:**
- `UploadFileAsync` - رفع ملف
- `UploadImageAsync` - رفع صورة مع Thumbnails
- `DeleteFileAsync` - حذف ملف
- `GetFileInfoAsync` - معلومات ملف

**Features:**
- Image Resizing
- Thumbnail Generation
- Multiple Folders
- Progress Tracking (قريباً)

**مثال MAUI:**
```csharp
public class ProductFormViewModel
{
    private readonly FilesClient _filesClient;

    [RelayCommand]
    public async Task UploadProductImageAsync()
    {
        var photo = await MediaPicker.PickPhotoAsync();
        if (photo == null) return;

        using var stream = await photo.OpenReadAsync();

        var uploaded = await _filesClient.UploadImageAsync(
            stream,
            photo.FileName,
            new ImageUploadOptions
            {
                Folder = "products",
                GenerateThumbnail = true,
                MaxWidth = 1200,
                MaxHeight = 1200
            }
        );

        if (uploaded != null)
        {
            ProductImageUrl = uploaded.Url;
            ThumbnailUrl = uploaded.ThumbnailUrl;
        }
    }
}
```

**مثال Blazor:**
```razor
<InputFile OnChange="HandleFileUpload" />

@code {
    private async Task HandleFileUpload(InputFileChangeEventArgs e)
    {
        var file = e.File;
        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB

        var uploaded = await filesClient.UploadFileAsync(
            stream,
            file.Name,
            folder: "documents"
        );

        if (uploaded != null)
        {
            Console.WriteLine($"File uploaded: {uploaded.Url}");
        }
    }
}
```

---

## 🌍 Localization Support

**تلقائياً مفعل** في جميع الطلبات!

### كيف يعمل:

```csharp
// في Blazor
@inject ILocalizationProvider LocalizationProvider

// تغيير اللغة
await LocalizationProvider.SetLanguageAsync("ar");

// تلقائياً سيتم إرسال Headers:
// Accept-Language: ar
// X-Localization: ar
// X-Culture: ar-SA
```

### Custom Localization Provider (MAUI):

```csharp
public class MauiLocalizationProvider : ILocalizationProvider
{
    public async Task<string> GetCurrentLanguageAsync()
    {
        return await SecureStorage.GetAsync("app_language") ?? "en";
    }

    public async Task<string> GetCurrentCultureAsync()
    {
        var lang = await GetCurrentLanguageAsync();
        return lang switch
        {
            "ar" => "ar-SA",
            "en" => "en-US",
            _ => "en-US"
        };
    }

    public async Task SetLanguageAsync(string language)
    {
        await SecureStorage.SetAsync("app_language", language);
        // تحديث UI
    }
}

// التسجيل
builder.Services.AddACommerceClient("http://localhost:5100", options =>
{
    options.EnableLocalization = true;
    options.LocalizationProvider = sp => sp.GetRequiredService<MauiLocalizationProvider>();
});
```

---

## 🔒 Authentication Flow الكامل

### 1. Login & Token Management:

```csharp
// Login
var response = await authClient.LoginAsync(new LoginRequest
{
    Username = "user@example.com",
    Password = "password"
});

if (response != null)
{
    // حفظ Token
    tokenManager.SetToken(response.Token, response.ExpiresAt);

    // تلقائياً سيتم إضافة Token لكل طلب HTTP بعد هذا
    // Authorization: Bearer {token}
}
```

### 2. Secure Storage (MAUI):

```csharp
public class SecureTokenManager : ITokenProvider
{
    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.GetAsync("auth_token");
    }

    public async Task SetTokenAsync(string token, DateTime expiresAt)
    {
        await SecureStorage.SetAsync("auth_token", token);
        await SecureStorage.SetAsync("token_expiry", expiresAt.ToString("O"));
    }

    public async Task ClearAsync()
    {
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("token_expiry");
    }
}
```

### 3. Protected Storage (Blazor):

```csharp
@inject ProtectedLocalStorage ProtectedStorage

private async Task SaveTokenAsync(string token)
{
    await ProtectedStorage.SetAsync("auth_token", token);
}

private async Task<string?> GetTokenAsync()
{
    var result = await ProtectedStorage.GetAsync<string>("auth_token");
    return result.Success ? result.Value : null;
}
```

---

## 🎯 Complete E-Commerce Flow

### Blazor Example - كامل:

```csharp
@page "/checkout"
@inject CartClient CartClient
@inject PaymentsClient PaymentsClient
@inject ShippingClient ShippingClient
@inject OrdersClient OrdersClient

<h3>Checkout</h3>

@if (cart != null)
{
    <div class="cart-summary">
        <h4>Cart Items</h4>
        @foreach (var item in cart.Items)
        {
            <p>@item.ListingName x @item.Quantity = @item.Total.ToString("C")</p>
        }
        <strong>Total: @cart.Total.ToString("C")</strong>
    </div>

    <h4>Shipping</h4>
    @if (shippingRates != null)
    {
        @foreach (var rate in shippingRates.Rates)
        {
            <div>
                <input type="radio" name="shipping" value="@rate.Provider"
                       @onchange="() => selectedShipping = rate" />
                <label>@rate.Provider - @rate.ServiceType: @rate.Cost.ToString("C")</label>
            </div>
        }
    }

    <button @onclick="PlaceOrder">Place Order</button>
}

@code {
    private CartResponse? cart;
    private ShippingRateResponse? shippingRates;
    private ShippingRate? selectedShipping;

    protected override async Task OnInitializedAsync()
    {
        cart = await CartClient.GetCartAsync(GetUserId());
        shippingRates = await ShippingClient.CalculateShippingAsync(new ShippingRateRequest
        {
            FromCity = "Riyadh",
            ToCity = customerAddress.City,
            Weight = CalculateTotalWeight()
        });
    }

    private async Task PlaceOrder()
    {
        // 1. إنشاء الطلب
        var order = await OrdersClient.CreateAsync(new CreateOrderRequest
        {
            Items = cart!.Items.Select(i => new OrderItemRequest
            {
                ProductId = i.ListingId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList(),
            ShippingAddress = customerAddress.ToString()
        });

        // 2. إنشاء الدفعة
        var payment = await PaymentsClient.CreatePaymentAsync(new CreatePaymentRequest
        {
            OrderId = order!.Id,
            Amount = cart.Total + selectedShipping!.Cost,
            Currency = "SAR",
            PaymentMethod = "Mada"
        });

        if (payment?.PaymentUrl != null)
        {
            // 3. توجيه للدفع
            NavigationManager.NavigateTo(payment.PaymentUrl);
        }
    }
}
```

---

## 📊 الخلاصة

### ✅ ما تم إنجازه:

**14 مكتبة Client SDK كاملة:**
1. ✅ Core (HTTP + Interceptors)
2. ✅ Auth
3. ✅ Products
4. ✅ Orders
5. ✅ Cart
6. ✅ Payments
7. ✅ Shipping
8. ✅ Vendors
9. ✅ Profiles
10. ✅ ProductListings
11. ✅ Notifications
12. ✅ Chats
13. ✅ Realtime (SignalR)
14. ✅ Files

### 🎯 Features:

- ✅ Dynamic Service URLs (Service Registry)
- ✅ Auto Authentication (Token Interceptor)
- ✅ Auto Localization (Language Headers)
- ✅ Auto Retry (Exponential Backoff)
- ✅ Type-safe APIs
- ✅ Async/Await
- ✅ CancellationToken
- ✅ Multi-platform (MAUI, Blazor, WPF, ASP.NET)
- ✅ SOLID + DRY
- ✅ Production-ready

### 🚀 الاستخدام بسطر واحد:

```csharp
builder.Services.AddAuthClient("http://localhost:5100");
```

**جميع المكتبات جاهزة للاستخدام الفوري!** 🎉
