# 🎨 ACommerce Blazor Shop Template

## 📋 نظرة عامة

قالب **Blazor WebAssembly** كامل لمتجر إلكتروني متعدد التجار باستخدام:
- ✅ **Syncfusion Blazor Components** - أقوى وأسرع مكتبة UI
- ✅ **جميع 14 ACommerce Client SDK** - مدمجة بالكامل
- ✅ **Theme System** قابل للتخصيص الكامل عبر CSS Variables
- ✅ **Dynamic Properties** - دعم خصائص ديناميكية للمنتجات
- ✅ **RTL Support** - دعم العربية كامل
- ✅ **Dark Mode** جاهز

---

## 🚀 البدء السريع

### 1. المتطلبات:

```bash
# .NET 9 SDK
dotnet --version

# Service Registry يجب أن يكون شغال
cd Infrastructure/ACommerce.ServiceRegistry.Server
dotnet run  # http://localhost:5100
```

### 2. تشغيل القالب:

```bash
cd Templates/ACommerce.Blazor.Shop
dotnet run
```

سيعمل على: `https://localhost:5001`

---

## 📦 البنية

```
ACommerce.Blazor.Shop/
├── Pages/                        # الصفحات الرئيسية
│   ├── Index.razor              # الصفحة الرئيسية (Hero + Categories + Featured)
│   ├── Products.razor           # قائمة المنتجات (مع Dynamic Properties)
│   ├── Search.razor             # البحث والفلاتر المتقدمة
│   ├── Cart.razor               # سلة التسوق
│   ├── Checkout.razor           # إتمام الطلب (3 خطوات)
│   ├── Orders.razor             # طلباتي (مع التتبع)
│   ├── Profile.razor            # الملف الشخصي (معلومات + نقاط اتصال + أمان)
│   ├── Notifications.razor      # الإشعارات (InApp + Firebase)
│   ├── Onboarding.razor         # صفحات البداية (5 شاشات)
│   ├── Auth/
│   │   ├── Login.razor          # تسجيل الدخول (Phone/Email/Google/Apple/Nafath)
│   │   ├── Register.razor       # إنشاء حساب
│   │   ├── TwoFactor.razor      # المصادقة الثنائية (OTP)
│   │   ├── NafathCallback.razor # استقبال نفاذ
│   │   └── NafathSelectNumber.razor # اختيار رقم نفاذ
│   └── Chats/
│       ├── Conversations.razor  # قائمة المحادثات
│       └── ChatRoom.razor       # غرفة المحادثة (SignalR Realtime)
│
├── Components/                  # المكونات القابلة لإعادة الاستخدام
│   ├── MainLayout.razor        # التخطيط الرئيسي (Header + Sidebar)
│   ├── NavMenu.razor           # القائمة الجانبية
│   ├── ProductCard.razor       # بطاقة المنتج
│   ├── CartIcon.razor          # أيقونة السلة (مع العدد والإجمالي)
│   ├── ThemeToggle.razor       # تبديل الثيم (Light/Dark)
│   ├── NotificationsList.razor # قائمة الإشعارات
│   └── OrdersList.razor        # قائمة الطلبات
│
├── Services/                    # الخدمات
│   ├── CartStateService.cs     # حالة السلة (Shared State + Events)
│   ├── ThemeService.cs         # إدارة الثيم
│   └── NotificationService.cs  # الإشعارات (Toast)
│
├── wwwroot/
│   ├── css/
│   │   ├── theme-variables.css  # 🎨 المتغيرات - خصص هنا!
│   │   └── app.css              # Styles التطبيق
│   └── index.html
│
├── Program.cs                   # تسجيل الخدمات (All 15 Client SDKs)
└── _Imports.razor               # Imports عامة
```

---

## 🎨 تخصيص الثيم

### طريقة سهلة جداً - فقط غير القيم!

في `wwwroot/css/theme-variables.css`:

```css
:root {
	/* غير الألوان الأساسية */
	--primary-color: #6366f1;    /* اللون الأساسي */
	--secondary-color: #ec4899;  /* اللون الثانوي */
	--success-color: #10b981;    /* لون النجاح */

	/* غير المسافات */
	--spacing-md: 16px;
	--spacing-lg: 24px;

	/* غير الحدود */
	--border-radius: 8px;

	/* غير الخطوط */
	--font-family: 'Cairo', sans-serif;  /* للعربية */
	--font-size-base: 16px;
}
```

**فوراً** سيتغير الثيم بالكامل! ✨

### Dark Mode:

```css
[data-theme="dark"] {
	--bg-primary: #111827;
	--text-primary: #f9fafb;
	/* ... */
}
```

---

## 💡 Dynamic Properties Support

### المشكلة:
المنتجات لها خصائص مختلفة (مثلاً: ملابس لها مقاس/لون، موبايل له ذاكرة/لون، إلخ)

### الحل:
نستخدم `Dictionary<string, object>` في الـ Product Model:

```csharp
public class Product
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public decimal Price { get; set; }

	// 🎯 خصائص ديناميكية!
	public Dictionary<string, object> Properties { get; set; } = new();
}
```

### مثال الاستخدام:

```razor
@* في صفحة المنتج *@
@if (product.Properties.Any())
{
	<div class="product-properties">
		@foreach (var prop in product.Properties)
		{
			<div class="property">
				<strong>@prop.Key:</strong>
				<span>@prop.Value</span>
			</div>
		}
	</div>
}
```

### أمثلة:

```csharp
// منتج: قميص
product.Properties = new()
{
	{ "Size", "L" },
	{ "Color", "Blue" },
	{ "Material", "Cotton" }
};

// منتج: موبايل
product.Properties = new()
{
	{ "RAM", "8GB" },
	{ "Storage", "256GB" },
	{ "Color", "Black" },
	{ "ScreenSize", "6.7 inch" }
};

// منتج: كتاب
product.Properties = new()
{
	{ "Author", "محمد" },
	{ "Pages", 350 },
	{ "Publisher", "دار النشر" },
	{ "ISBN", "978-1234567890" }
};
```

### في Blazor Component:

```razor
@code {
	private async Task LoadProduct()
	{
		var product = await productsClient.GetByIdAsync(productId);

		// Dynamic Properties تلقائياً!
		if (product.Properties.ContainsKey("Size"))
		{
			selectedSize = product.Properties["Size"].ToString();
		}
	}
}
```

---

## 🛒 Cart State Management

### استخدام CartStateService:

```razor
@inject CartStateService CartState

<button @onclick="AddToCart">Add to Cart</button>

<CartIcon ItemCount="@CartState.ItemCount" Total="@CartState.Total" />

@code {
	protected override async Task OnInitializedAsync()
	{
		await CartState.LoadCartAsync();

		// الاستماع للتغييرات
		CartState.OnCartChanged += StateHasChanged;
	}

	private async Task AddToCart()
	{
		var success = await CartState.AddToCartAsync(productId, 1);
		if (success)
		{
			// السلة تحدثت تلقائياً + Event fired!
		}
	}

	public void Dispose()
	{
		CartState.OnCartChanged -= StateHasChanged;
	}
}
```

---

## 🎯 Syncfusion Components - أمثلة

### Grid للمنتجات:

```razor
<SfGrid DataSource="@products" AllowPaging="true" PageSize="12">
	<GridColumns>
		<GridColumn Field="Name" HeaderText="Product"></GridColumn>
		<GridColumn Field="Price" HeaderText="Price" Format="C2"></GridColumn>
		<GridColumn HeaderText="Actions">
			<Template>
				@{
					var product = (context as Product)!;
					<SfButton OnClick="() => AddToCart(product)">Add to Cart</SfButton>
				}
			</Template>
		</GridColumn>
	</GridColumns>
</SfGrid>
```

### Card للمنتجات:

```razor
<SfCard>
	<CardHeader Title="@product.Name" />
	<CardContent>
		<img src="@product.ImageUrl" class="product-image" />
		<p class="product-price">@product.Price.ToString("C")</p>
	</CardContent>
	<CardFooter>
		<SfButton CssClass="btn-primary" OnClick="AddToCart">Add to Cart</SfButton>
	</CardFooter>
</SfCard>
```

### Toast Notifications:

```razor
@inject NotificationService NotificationService

<SfToast @ref="toastObj" Position="X: Right, Y: Top">
	<ToastTemplates>
		<Template>
			<div>@currentMessage</div>
		</Template>
	</ToastTemplates>
</SfToast>

@code {
	private SfToast? toastObj;
	private string currentMessage = "";

	protected override void OnInitialized()
	{
		NotificationService.OnShow += ShowToast;
	}

	private async void ShowToast(string message, NotificationType type)
	{
		currentMessage = message;
		await toastObj!.ShowAsync();
	}
}
```

---

## 🌍 Localization (RTL Support)

### تفعيل العربية:

في `index.html`:

```html
<html lang="ar" dir="rtl">
```

في CSS تلقائياً:

```css
[dir="rtl"] {
	--font-family: 'Cairo', 'Segoe UI', Tahoma;
}
```

### في الكود:

```csharp
@inject ILocalizationProvider LocalizationProvider

await LocalizationProvider.SetLanguageAsync("ar");
// تلقائياً سيرسل Headers للـ Backend
```

---

## 📊 Complete E-Commerce Flow

```razor
@page "/checkout"
@inject CartStateService CartState
@inject PaymentsClient PaymentsClient
@inject OrdersClient OrdersClient
@inject NavigationManager Navigation

<h3>Checkout</h3>

@* 1. عرض السلة *@
<div class="cart-summary">
	@foreach (var item in CartState.CurrentCart?.Items ?? new())
	{
		<div>@item.ProductName x @item.Quantity</div>
	}
	<strong>Total: @CartState.Total.ToString("C")</strong>
</div>

@* 2. معلومات الشحن *@
<EditForm Model="@shippingAddress" OnValidSubmit="PlaceOrder">
	<DataAnnotationsValidator />
	<SfTextBox @bind-Value="shippingAddress.Street" Placeholder="Address"></SfTextBox>
	<SfTextBox @bind-Value="shippingAddress.City" Placeholder="City"></SfTextBox>
	<SfButton Type="Submit">Place Order</SfButton>
</EditForm>

@code {
	private ShippingAddress shippingAddress = new();

	private async Task PlaceOrder()
	{
		// 1. إنشاء الطلب
		var order = await OrdersClient.CreateAsync(new CreateOrderRequest
		{
			Items = MapCartItems(),
			ShippingAddress = shippingAddress.ToString()
		});

		// 2. إنشاء الدفعة
		var payment = await PaymentsClient.CreatePaymentAsync(new CreatePaymentRequest
		{
			OrderId = order.Id,
			Amount = CartState.Total,
			Currency = "SAR",
			PaymentMethod = "Mada"
		});

		// 3. توجيه للدفع
		if (payment?.PaymentUrl != null)
		{
			Navigation.NavigateTo(payment.PaymentUrl);
		}
	}
}
```

---

## 🎯 Best Practices

### ✅ Do's:

1. **استخدم CSS Variables للتخصيص**
   - لا تعدل في `app.css` مباشرة
   - غير فقط في `theme-variables.css`

2. **استخدم Services للـ Shared State**
   - `CartStateService` للسلة
   - `ThemeService` للثيم
   - لا تكرر الكود

3. **استخدم Syncfusion Components**
   - Grid للجداول
   - Card للبطاقات
   - Toast للإشعارات

4. **Dynamic Properties للمرونة**
   - كل منتج له خصائص مختلفة
   - استخدم `Dictionary<string, object>`

### ❌ Don'ts:

1. **لا تكتب Inline Styles**
   - استخدم CSS Classes
   - استخدم CSS Variables

2. **لا تستخدم Magic Strings**
   - استخدم Constants

3. **لا تنسى Dispose للـ Events**
   ```csharp
   public void Dispose()
   {
       CartState.OnCartChanged -= StateHasChanged;
   }
   ```

---

## 📦 Integration مع Backend

### في `Program.cs`:

```csharp
// Service Registry URL
const string registryUrl = "http://localhost:5100";

// كل الخدمات بسطر واحد!
builder.Services.AddAuthClient(registryUrl);
builder.Services.AddACommerceClient(registryUrl, options =>
{
	options.EnableAuthentication = true;
	options.EnableLocalization = true;
});
```

### في Components:

```razor
@inject ProductsClient ProductsClient
@inject CartClient CartClient

var products = await ProductsClient.GetAllAsync();
var cart = await CartClient.GetCartAsync(userId);
```

**كل شيء جاهز!** ✨

---

## 🚀 Production Deployment

### 1. Build:

```bash
dotnet publish -c Release
```

### 2. Deploy إلى:
- **Azure Static Web Apps** ✅
- **GitHub Pages** ✅
- **Netlify** ✅
- **Any Static Host** ✅

### 3. Environment Variables:

```json
{
	"ServiceRegistry": {
		"Url": "https://your-registry.com"
	}
}
```

---

## 📚 الخلاصة

✨ **هذا القالب يوفر لك:**
- ✅ **بنية كاملة** للمتجر الإلكتروني
- ✅ **Theme System** قابل للتخصيص بالكامل
- ✅ **Dynamic Properties** للمرونة
- ✅ **Syncfusion Components** للسرعة
- ✅ **All 15 Client SDKs** مدمجة
- ✅ **RTL + Dark Mode** جاهز
- ✅ **أقل عدد أسطر** ممكن
- ✅ **Production-ready**

---

## 🔐 Authentication Features

### طرق تسجيل الدخول المتعددة:

1. **📱 رقم الجوال (OTP)**
   - إرسال كود التحقق عبر SMS
   - صفحة TwoFactor للتحقق

2. **📧 البريد الإلكتروني**
   - تسجيل دخول تقليدي (Email + Password)
   - دعم المصادقة الثنائية

3. **🔵 Google OAuth**
   - تسجيل دخول فوري عبر Google

4. **🍎 Apple Sign In**
   - تسجيل دخول فوري عبر Apple

5. **🇸🇦 نفاذ (الهوية الوطنية)**
   - تكامل مع منصة نفاذ
   - اختيار رقم الجوال من الأرقام المسجلة

---

## 💬 Chat & Realtime Features

### المحادثات المباشرة:

- **SignalR Integration** - اتصال مباشر ثنائي الاتجاه
- **Real-time Messages** - الرسائل تصل فوراً
- **Online Status** - معرفة حالة الاتصال
- **Typing Indicators** - مؤشر الكتابة
- **Message Read Status** - حالة قراءة الرسائل

---

## 🔔 Notifications System

### نوعين من الإشعارات:

1. **InApp Notifications**
   - إشعارات داخل التطبيق
   - تصنيف حسب النوع (طلبات، رسائل، عروض)
   - SignalR للتحديثات الفورية

2. **Firebase Push Notifications**
   - إشعارات فورية على الجوال
   - إعدادات مخصصة (أنواع + أوقات هادئة)
   - تكامل مع Firebase Cloud Messaging

---

## 👤 Profile & Contact Points

### إدارة الملف الشخصي:

1. **المعلومات الشخصية**
   - الاسم، اسم المستخدم، تاريخ الميلاد
   - الصورة الشخصية، النبذة

2. **نقاط الاتصال** (Contact Points)
   - Email, Phone, Address, Social Media
   - تحقق OTP للإيميل والجوال
   - تعيين نقطة اتصال أساسية
   - دعم متعدد (عدة إيميلات/جوالات/عناوين)

3. **الأمان**
   - تغيير كلمة المرور
   - تفعيل/تعطيل المصادقة الثنائية
   - إدارة الجلسات النشطة

4. **التفضيلات**
   - اختيار اللغة (عربي/English)
   - العملة المفضلة
   - إعدادات الإشعارات

---

## 🎯 Complete Pages List

### ✅ Pages جاهزة:

- ✅ **/** - Home (Hero + Categories + Featured + New + Bestsellers)
- ✅ **/search** - Search & Advanced Filters
- ✅ **/products** - Products Grid with Pagination
- ✅ **/cart** - Shopping Cart with Coupon Support
- ✅ **/checkout** - 3-Step Checkout (Address + Shipping + Payment)
- ✅ **/orders** - My Orders with Tracking
- ✅ **/profile** - Complete Profile Management
- ✅ **/notifications** - InApp + Firebase Push Notifications
- ✅ **/chats** - Conversations List
- ✅ **/chats/{id}** - Chat Room with SignalR Realtime
- ✅ **/onboarding** - Welcome Slides (5 screens)
- ✅ **/login** - Multi-Auth Login (Phone/Email/OAuth/Nafath)
- ✅ **/register** - Registration with Email/Social
- ✅ **/auth/two-factor** - OTP Verification
- ✅ **/auth/nafath-select-number** - Nafath Number Selection

**ابدأ التطوير الآن!** 🚀
