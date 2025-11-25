# 📱 ACommerce Client SDKs - دليل الاستخدام

## 📋 نظرة عامة

مكتبات **.NET Client SDKs** للتواصل مع الخدمات الخلفية من تطبيقات:
- **.NET MAUI** (iOS, Android, Windows, macOS)
- **Blazor** (WebAssembly, Server, Hybrid)
- **WPF** / **WinForms**
- **ASP.NET** (Server-side)

### ✨ المميزات الرئيسية:

- **Dynamic Service URLs** - اكتشاف تلقائي للخدمات من Service Registry
- **HTTP Client مرن** - Retry, Timeout, Authentication تلقائي
- **Type-safe** - Models مشتركة مع Backend
- **Async/Await** - Performance ممتاز
- **DI-ready** - تسجيل سهل في DI Container
- **SOLID + DRY** - معمارية نظيفة

---

## 📦 المكتبات

### 1️⃣ **ACommerce.Client.Core**
الطبقة الأساسية للـ HTTP مع Dynamic Service Discovery

**المكونات:**
- `DynamicHttpClient` - HTTP Client مع URLs ديناميكية
- `AuthenticationInterceptor` - إضافة Token تلقائياً
- `RetryInterceptor` - إعادة محاولة عند الفشل
- Integration مع `ServiceRegistryClient`

### 2️⃣ **ACommerce.Client.Auth**
مكتبة Authentication

**المكونات:**
- `AuthClient` - Login, Register, Logout, GetMe
- `TokenManager` - إدارة Authentication Token
- Models: `LoginRequest`, `LoginResponse`, `UserInfo`

### 3️⃣ **ACommerce.Client.Products**
مكتبة Products

**المكونات:**
- `ProductsClient` - CRUD operations للمنتجات
- Models مشتركة مع `ACommerce.Catalog.Products`

### 4️⃣ **ACommerce.Client.Orders**
مكتبة Orders

**المكونات:**
- `OrdersClient` - CRUD operations للطلبات
- Models مشتركة مع `ACommerce.Orders`

---

## 🚀 البدء السريع

### التثبيت

```bash
# Core (مطلوب)
dotnet add package ACommerce.Client.Core

# Auth (اختياري)
dotnet add package ACommerce.Client.Auth

# Products (اختياري)
dotnet add package ACommerce.Client.Products

# Orders (اختياري)
dotnet add package ACommerce.Client.Orders
```

### التسجيل في DI Container

#### Blazor WebAssembly:

```csharp
using ACommerce.Client.Auth.Extensions;
using ACommerce.Client.Products.Extensions;
using ACommerce.Client.Orders.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Service Registry URL
const string registryUrl = "http://localhost:5100";

// ✨ Auth Client (يشمل Core تلقائياً)
builder.Services.AddAuthClient(registryUrl);

// ✨ Products Client
builder.Services.AddProductsClient(registryUrl);

// ✨ Orders Client
builder.Services.AddOrdersClient(registryUrl);

await builder.Build().RunAsync();
```

#### .NET MAUI:

```csharp
using ACommerce.Client.Auth.Extensions;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>();

		// Service Registry URL
		const string registryUrl = "http://localhost:5100";

		// ✨ Client SDKs
		builder.Services.AddAuthClient(registryUrl);
		builder.Services.AddProductsClient(registryUrl);
		builder.Services.AddOrdersClient(registryUrl);

		return builder.Build();
	}
}
```

---

## 💻 أمثلة الاستخدام

### مثال 1: Authentication (Login)

```csharp
@page "/login"
@inject AuthClient AuthClient
@inject TokenManager TokenManager
@inject NavigationManager Navigation

<EditForm Model="loginModel" OnValidSubmit="HandleLogin">
	<InputText @bind-Value="loginModel.Username" placeholder="Username" />
	<InputText @bind-Value="loginModel.Password" type="password" placeholder="Password" />
	<button type="submit">Login</button>
	@if (!string.IsNullOrEmpty(errorMessage))
	{
		<p class="error">@errorMessage</p>
	}
</EditForm>

@code {
	private LoginRequest loginModel = new();
	private string errorMessage = string.Empty;

	private async Task HandleLogin()
	{
		try
		{
			// تسجيل دخول
			var response = await AuthClient.LoginAsync(loginModel);

			if (response != null)
			{
				// حفظ Token
				TokenManager.SetToken(response.Token, response.ExpiresAt);

				// الانتقال للصفحة الرئيسية
				Navigation.NavigateTo("/");
			}
			else
			{
				errorMessage = "Invalid username or password";
			}
		}
		catch (Exception ex)
		{
			errorMessage = ex.Message;
		}
	}
}
```

### مثال 2: عرض المنتجات (.NET MAUI)

```csharp
public class ProductsViewModel : ObservableObject
{
	private readonly ProductsClient _productsClient;
	private ObservableCollection<Product> _products = new();

	public ObservableCollection<Product> Products
	{
		get => _products;
		set => SetProperty(ref _products, value);
	}

	public ProductsViewModel(ProductsClient productsClient)
	{
		_productsClient = productsClient;
	}

	[RelayCommand]
	public async Task LoadProductsAsync()
	{
		try
		{
			var products = await _productsClient.GetAllAsync();
			if (products != null)
			{
				Products = new ObservableCollection<Product>(products);
			}
		}
		catch (Exception ex)
		{
			// عرض رسالة خطأ
			await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
		}
	}
}
```

### مثال 3: إنشاء طلب (Blazor)

```csharp
@page "/checkout"
@inject OrdersClient OrdersClient
@inject NavigationManager Navigation

<h3>Checkout</h3>

<EditForm Model="orderRequest" OnValidSubmit="CreateOrder">
	<p>Total: @orderRequest.Items.Sum(i => i.Quantity * i.UnitPrice).ToString("C")</p>
	<InputText @bind-Value="orderRequest.ShippingAddress" placeholder="Shipping Address" />
	<button type="submit">Place Order</button>
</EditForm>

@code {
	private CreateOrderRequest orderRequest = new()
	{
		Items = new List<OrderItemRequest>
		{
			new() { ProductId = "p1", Quantity = 2, UnitPrice = 99.99m },
			new() { ProductId = "p2", Quantity = 1, UnitPrice = 49.99m }
		}
	};

	private async Task CreateOrder()
	{
		try
		{
			var order = await OrdersClient.CreateAsync(orderRequest);

			if (order != null)
			{
				// الانتقال لصفحة التأكيد
				Navigation.NavigateTo($"/order-confirmation/{order.Id}");
			}
		}
		catch (Exception ex)
		{
			// عرض رسالة خطأ
			Console.WriteLine($"Error: {ex.Message}");
		}
	}
}
```

### مثال 4: الحصول على معلومات المستخدم

```csharp
@page "/profile"
@inject AuthClient AuthClient

<h3>Profile</h3>

@if (userInfo != null)
{
	<p>Username: @userInfo.Username</p>
	<p>Email: @userInfo.Email</p>
	<p>Role: @userInfo.Role</p>
}

@code {
	private UserInfo? userInfo;

	protected override async Task OnInitializedAsync()
	{
		try
		{
			userInfo = await AuthClient.GetMeAsync();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}
}
```

---

## ⚙️ تكوين متقدم

### استخدام Core فقط (بدون Auth/Products/Orders)

```csharp
using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Core.Http;

builder.Services.AddACommerceClient("http://localhost:5100", options =>
{
	options.TimeoutSeconds = 60;
	options.EnableRetry = true;
	options.MaxRetries = 5;
	options.EnableAuthentication = false; // لا نريد Authentication
});

// استخدام DynamicHttpClient مباشرة
public class MyService
{
	private readonly DynamicHttpClient _httpClient;

	public MyService(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<MyData?> GetDataAsync()
	{
		return await _httpClient.GetAsync<MyData>("MyService", "/api/data");
	}
}
```

### Custom Token Provider

```csharp
public class SecureTokenManager : ITokenProvider
{
	private readonly ISecureStorage _secureStorage;

	public SecureTokenManager(ISecureStorage secureStorage)
	{
		_secureStorage = secureStorage;
	}

	public async Task<string?> GetTokenAsync()
	{
		return await _secureStorage.GetAsync("auth_token");
	}

	public async Task SetTokenAsync(string token)
	{
		await _secureStorage.SetAsync("auth_token", token);
	}
}

// التسجيل
builder.Services.AddSingleton<ITokenProvider, SecureTokenManager>();
builder.Services.AddACommerceClient("http://localhost:5100", options =>
{
	options.EnableAuthentication = true;
	options.TokenProvider = sp => sp.GetRequiredService<ITokenProvider>();
});
```

---

## 🎯 Patterns & Best Practices

### ✅ Do's:

1. **استخدم DI دائماً**
   - لا تنشئ instances يدوياً
   - اعتمد على Constructor Injection

2. **استخدم CancellationToken**
   - لإلغاء الطلبات عند الحاجة
   - خصوصاً في MAUI/Blazor

3. **Handle Exceptions**
   - استخدم try-catch
   - اعرض رسائل خطأ واضحة للمستخدم

4. **Cache Token بأمان**
   - في MAUI: استخدم `SecureStorage`
   - في Blazor: استخدم `ProtectedLocalStorage`

### ❌ Don'ts:

1. **لا تكتب URLs يدوياً**
   - اعتمد على Service Discovery

2. **لا تخزن Token في Plain Text**
   - استخدم Secure Storage

3. **لا تنسى Dispose HttpClient**
   - DI يتعامل معها تلقائياً

---

## 🔐 أمان Token

### Blazor WebAssembly:

```csharp
@inject ProtectedLocalStorage ProtectedStorage

// حفظ
await ProtectedStorage.SetAsync("token", token);

// قراءة
var result = await ProtectedStorage.GetAsync<string>("token");
if (result.Success)
{
	var token = result.Value;
}
```

### .NET MAUI:

```csharp
// حفظ
await SecureStorage.SetAsync("auth_token", token);

// قراءة
var token = await SecureStorage.GetAsync("auth_token");
```

---

## 📊 المعمارية

```
┌─────────────────────────────────────────────────────┐
│              Client Application                     │
│  (Blazor / MAUI / WPF / ASP.NET)                   │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│          ACommerce.Client.* SDKs                    │
│  ├─ AuthClient                                      │
│  ├─ ProductsClient                                  │
│  └─ OrdersClient                                    │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│         ACommerce.Client.Core                       │
│  ├─ DynamicHttpClient (HTTP مع Dynamic URLs)       │
│  ├─ AuthenticationInterceptor                       │
│  └─ RetryInterceptor                                │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│      ACommerce.ServiceRegistry.Client               │
│  ├─ ServiceRegistryClient                           │
│  ├─ ServiceCache (5 دقائق + Stale 1 ساعة)         │
│  └─ Service Discovery ديناميكي                     │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│      Service Registry Server                        │
│  (http://localhost:5100)                            │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│           Backend Services                          │
│  ├─ Marketplace (http://localhost:5000)             │
│  ├─ Products (http://localhost:5001)                │
│  └─ Orders (http://localhost:5002)                  │
└─────────────────────────────────────────────────────┘
```

---

## 🚀 الخلاصة

✨ **ACommerce Client SDKs** توفر:
- **Dynamic Service Discovery** - لا حاجة لـ Hardcoded URLs
- **Type-safe APIs** - Models مشتركة مع Backend
- **Authentication تلقائي** - Token management مدمج
- **Retry + Timeout** - Resilient HTTP Client
- **Multi-platform** - MAUI, Blazor, WPF, ASP.NET
- **SOLID + DRY** - معمارية نظيفة ومرنة

🎯 **كل ما تحتاجه:** سطر واحد للتسجيل في DI، وابدأ الاستخدام!

```csharp
// التسجيل
builder.Services.AddAuthClient("http://localhost:5100");

// الاستخدام
var response = await authClient.LoginAsync(request);
```

بسيط، نظيف، وقوي! 🚀
